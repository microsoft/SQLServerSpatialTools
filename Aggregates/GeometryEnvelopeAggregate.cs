//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[SqlUserDefinedAggregate(
		Format.Native,
		IsInvariantToDuplicates = true,
		IsInvariantToNulls = true,
		IsInvariantToOrder = true,
		IsNullIfEmpty = true)]
	public class GeometryEnvelopeAggregate : IGeometrySink
	{
		private double minX, maxX, minY, maxY;
		private int lastSrid;
		private bool failed;

		public void SetSrid(int srid)
		{
			if (lastSrid != -1 && lastSrid != srid) failed = true;
			lastSrid = srid;
		}

		public void BeginGeometry(OpenGisGeometryType type) { }

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			IncludePoint(x, y);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			IncludePoint(x, y);
		}

		public void EndFigure() { }

		public void EndGeometry() {	}

		public void Init()
		{
			minX = minY = Double.PositiveInfinity;
			maxX = maxY = Double.NegativeInfinity;
			lastSrid = -1;
			failed = false;
		}

		public void Accumulate(SqlGeometry geometry)
		{
			if (geometry != null) geometry.Populate(this);
		}

		public void Merge(GeometryEnvelopeAggregate group)
		{
			minX = Math.Min(minX, group.minX);
			maxX = Math.Max(maxX, group.maxX);
			minY = Math.Min(minY, group.minY);
			maxY = Math.Max(maxY, group.maxY);

			if (group.lastSrid != -1)
			{
				if (lastSrid != -1 && lastSrid != group.lastSrid) failed = true;
				lastSrid = group.lastSrid;
			}
			if (group.failed) failed = true;
		}

		public SqlGeometry Terminate()
		{
			if (failed) return SqlGeometry.Null;

			SqlGeometryBuilder b = new SqlGeometryBuilder();
			b.SetSrid(lastSrid);
			b.BeginGeometry(OpenGisGeometryType.Polygon);
			b.BeginFigure(minX, minY);
			b.AddLine(maxX, minY);
			b.AddLine(maxX, maxY);
			b.AddLine(minX, maxY);
			b.AddLine(minX, minY);
			b.EndFigure();
			b.EndGeometry();
			return b.ConstructedGeometry;
		}

		private void IncludePoint(double x, double y)
		{
			minX = Math.Min(minX, x);
			maxX = Math.Max(maxX, x);
			minY = Math.Min(minY, y);
			maxY = Math.Max(maxY, y);
		}
	}
}