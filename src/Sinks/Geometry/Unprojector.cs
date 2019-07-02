//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types.SQL;

namespace SQLSpatialTools.Sinks.Geometry
{
	public sealed class UnProjector : IGeometrySink110
	{
		private readonly SqlProjection _projection;
		private readonly IGeographySink110 _sink;

		public UnProjector(SqlProjection projection, IGeographySink110 sink, int newSrid)
		{
			_projection = projection;
			_sink = sink;
			_sink.SetSrid(newSrid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeography((OpenGisGeographyType)type);
		}

		public void EndGeometry()
		{
			_sink.EndGeography();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
            _projection.UnprojectPoint(x, y, out double latitude, out double longitude);
            _sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
            _projection.UnprojectPoint(x, y, out double latitude, out double longitude);
            _sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			// Input argument not used since a new srid is defined in the constructor.
		}
	}
}