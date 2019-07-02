//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types.SQL;

namespace SQLSpatialTools.Sinks.Geography
{
    /// <summary>
    /// This class projects a geography segment based on specified project to a geometry segment.
    /// </summary>
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

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
            _projection.ProjectPoint(latitude, longitude, out var x, out var y);
            _sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
            _projection.ProjectPoint(latitude, longitude, out var x, out var y);
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