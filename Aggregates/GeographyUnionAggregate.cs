//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[SqlUserDefinedAggregate(
		Format.UserDefined,
		IsInvariantToDuplicates = true,
		IsInvariantToNulls = true,
		IsInvariantToOrder = true,
		IsNullIfEmpty = false,
		MaxByteSize = -1)]
	public class GeographyUnionAggregate : IBinarySerialize
	{
		private GeographyCollectionAggregate aggregate;

		public void Init()
		{
			if (aggregate == null) aggregate = new GeographyCollectionAggregate();
			aggregate.Init();
		}

		public void Accumulate(SqlGeography g)
		{
			aggregate.Accumulate(g);
		}

		public void Merge(GeographyUnionAggregate group)
		{
			aggregate.Merge(group.aggregate);
		}

		public SqlGeography Terminate()
		{
			// force self union of the collection
			SqlGeography g = aggregate.Terminate();
			return g.STNumPoints().Value == 0 ? g : g.STUnion(g.STPointN(1));
		}

		public void Read(BinaryReader r)
		{
			if (aggregate == null) aggregate = new GeographyCollectionAggregate();
			aggregate.Read(r);
		}

		public void Write(BinaryWriter w)
		{
			aggregate.Write(w);
		}
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[SqlUserDefinedAggregate(
		Format.UserDefined,
		IsInvariantToDuplicates = true,
		IsInvariantToNulls = true,
		IsInvariantToOrder = true,
		IsNullIfEmpty = false,
		MaxByteSize = -1)]
	public class GeographyCollectionAggregate : IBinarySerialize
	{
		private SqlGeographyBuilder m_builder;
		private IGeographySink m_sink;
		private int m_srid;

		public void Init()
		{
			m_srid = -1;
			m_builder = null;
			m_sink = null;
		}

		public void Accumulate(SqlGeography g)
		{
			Debug.Assert(g != null && !g.IsNull);
			Reset(g.STSrid.Value);			
			g.Populate(m_sink);
		}

		private void Reset(int srid)
		{
			if (m_builder == null)
			{
				m_srid = srid;
				m_builder = new SqlGeographyBuilder();
				m_builder.SetSrid(m_srid);
				m_builder.BeginGeography(OpenGisGeographyType.GeometryCollection);
				m_sink = new StripSRID(m_builder);
			}
			else if (srid != m_srid) throw new Exception("different SRIDs");
		}

		public void Merge(GeographyCollectionAggregate group)
		{
			if (group.m_builder == null) return;
			group.m_builder.EndGeography();
			SqlGeography g = m_builder.ConstructedGeography;
			group.Init();
			Reset(g.STSrid.Value);
			g.Populate(new StripCollection(m_builder));
		}

		public SqlGeography Terminate()
		{
			if (m_builder == null) return null;
			m_builder.EndGeography();
			SqlGeography g = m_builder.ConstructedGeography;
			Init();
			return g;
		}

		public void Read(BinaryReader r)
		{
			if (r.ReadBoolean())
			{
				Init();
			}
			else
			{
				SqlGeography g = new SqlGeography();
				g.Read(r);
				m_srid = g.STSrid.Value;
				m_builder = new SqlGeographyBuilder();
				m_sink = new StripSRID(m_builder);
				m_builder.SetSrid(m_srid);
				m_builder.BeginGeography(OpenGisGeographyType.GeometryCollection);
				g.Populate(new StripCollection(m_builder));
			}
		}

		public void Write(BinaryWriter w)
		{
			w.Write(m_builder == null);
			if (m_builder != null)
			{
				m_builder.EndGeography();
				SqlGeography g = m_builder.ConstructedGeography;
				g.Write(w);
				Init();
			}
		}
	}

	internal class StripSRID : IGeographySink
	{
		private readonly IGeographySink m_sink;

		public StripSRID(IGeographySink sink)
		{
			m_sink = sink;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			m_sink.BeginGeography(type);
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			m_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			m_sink.AddLine(latitude, longitude, z, m);
		}

		public void EndFigure()
		{
			m_sink.EndFigure();
		}
		
		public void EndGeography()
		{
			m_sink.EndGeography();
		}
		
		public void SetSrid(int srid) {	}
	}

	internal class StripCollection : IGeographySink
	{
		private readonly IGeographySink m_sink;
		private int m_depth;

		public StripCollection(IGeographySink sink)
		{
			m_sink = sink;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (m_depth > 0)
			{
				m_sink.BeginGeography(type);
			}
			else
			{
				Debug.Assert(OpenGisGeographyType.GeometryCollection == type);
			}
			m_depth += 1;
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			m_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			m_sink.AddLine(latitude, longitude, z, m);
		}

		public void EndFigure()
		{
			m_sink.EndFigure();
		}

		public void EndGeography()
		{
			m_depth -= 1;
			if (m_depth > 0) m_sink.EndGeography();
		}

		public void SetSrid(int srid)
		{
		}
	}
}