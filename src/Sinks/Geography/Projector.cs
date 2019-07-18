//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class Projector : IGeographySink110
	{
		private readonly SqlProjection _projection;
		private readonly IGeometrySink110 _sink;

		public Projector(SqlProjection projection, IGeometrySink110 sink)
		{
			_projection = projection;
			_sink = sink;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			_sink.BeginGeometry((OpenGisGeometryType)type);
		}

		public void EndGeography()
		{
			_sink.EndGeometry();
		}

		public void BeginFigure(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
			double x, y;
			_projection.ProjectPoint(latitude, longitude, out x, out y);
			_sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
			double x, y;
			_projection.ProjectPoint(latitude, longitude, out x, out y);
			_sink.AddLine(x, y, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}
	}
}