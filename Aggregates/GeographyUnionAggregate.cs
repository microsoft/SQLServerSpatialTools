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
			SqlGeography g = aggregate.Terminate();

			if (g.IsNull || g.STIsEmpty().Value)
				return g;

			// force self union of the collection
			return g.STUnion(g.STPointN(1));
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
		private bool m_error;

		public void Init()
		{
			m_srid = -1;
			m_builder = null;
			m_sink = null;
			m_error = false;
		}

		private bool IsInitialState()
		{
			return m_builder == null && !m_error;
		}

		public void Accumulate(SqlGeography g)
		{
			if (g == null || g.IsNull || m_error) return;
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
			else if (srid != m_srid)
			{
				m_srid = -1;
				m_builder = null;
				m_sink = null;
				m_error = true;
			}
		}

		private SqlGeography ConstructedGeography()
		{
			SqlGeography g = SqlGeography.Null;
			if (!m_error)
			{
				try
				{
					m_builder.EndGeography();
					g = m_builder.ConstructedGeography;
				}
				catch (ArgumentException)
				{
					// Result is larger than a hemisphere!
				}
			}
			Init();
			return g;
		}

		public void Merge(GeographyCollectionAggregate group)
		{
			if (group.IsInitialState()) return;

			SqlGeography g = group.ConstructedGeography();
			if (g.IsNull)
			{
				m_srid = -1;
				m_builder = null;
				m_sink = null;
				m_error = true;
			}
			else
			{
				Reset(g.STSrid.Value);
				g.Populate(new StripCollection(m_builder));
			}
		}

		public SqlGeography Terminate()
		{
			if (IsInitialState()) return SqlGeography.Null;

			SqlGeography g = ConstructedGeography();
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
				if (g.IsNull)
				{
					m_srid = -1;
					m_error = true;
					m_builder = null;
					m_sink = null;
				}
				else
				{
					m_srid = g.STSrid.Value;
					m_builder = new SqlGeographyBuilder();
					m_sink = new StripSRID(m_builder);
					m_builder.SetSrid(m_srid);
					m_builder.BeginGeography(OpenGisGeographyType.GeometryCollection);
					g.Populate(new StripCollection(m_builder));
				}
			}
		}

		public void Write(BinaryWriter w)
		{
			w.Write(IsInitialState());
			if (!IsInitialState())
			{
				ConstructedGeography().Write(w);
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

		public void SetSrid(int srid) { }
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