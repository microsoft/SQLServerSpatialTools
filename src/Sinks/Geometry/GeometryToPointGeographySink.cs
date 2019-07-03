//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// Sink which extracts points from a geometry instance and forwards them to a geography sink.
    /// </summary>
    public sealed class GeometryToPointGeographySink : IGeometrySink110
	{
		private readonly IGeographySink110 _sink;
		private int _count;

		public GeometryToPointGeographySink(IGeographySink110 sink)
		{
			_sink = sink;
			_count = 0;
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (_count == 0)
			{
				_sink.BeginGeography(OpenGisGeographyType.MultiPoint);
			}
			_count++;
		}

		public void EndGeometry()
		{
			_count--;
			if (_count == 0)
			{
				_sink.EndGeography();
			}
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_sink.BeginGeography(OpenGisGeographyType.Point);
			_sink.BeginFigure(y, x, z, m);
			_sink.EndFigure();
			_sink.EndGeography();
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			BeginFigure(x, y, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			// we ignore these calls
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}
	}
}