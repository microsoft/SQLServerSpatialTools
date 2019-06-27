//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that extracts the polygon based upon the linestring.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    internal class ExtractPolygonFromLineGeometrySink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;
        private List<LRSPoint> _points;
        private int _srid;
        private bool _doReverse;
        private bool _isCircularString;

        public ExtractPolygonFromLineGeometrySink(SqlGeometryBuilder geomBuilder, bool doReverse)
        {
            _target = geomBuilder;
            _points = new List<LRSPoint>();
            _doReverse = doReverse;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            _srid = srid;
            _target.SetSrid(_srid);
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            _isCircularString = type == OpenGisGeometryType.CircularString;
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _points.Add(new LRSPoint(x, y, z, m, _srid));
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _points.Add(new LRSPoint(x, y, z, m, _srid));
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            _points.Add(new LRSPoint(x1, y1, z1, m1, _srid));
            _points.Add(new LRSPoint(x2, y2, z2, m2, _srid));
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            // no-op
        }

        // This is a NO-OP
        public void EndGeometry()
        {

            var first = true;
            if (_doReverse)
                _points.Reverse();

            if (_isCircularString)
            {
                _target.BeginGeometry(OpenGisGeometryType.CurvePolygon);

                for (var iterator = 0; iterator < _points.Count; iterator++)
                {
                    var point = _points[iterator];
                    if (first)
                    {
                        _target.BeginFigure(point.X, point.Y, point.Z, point.M);
                        first = false;
                    }
                    else
                    {
                        var nextPoint = _points[iterator + 1];
                        _target.AddCircularArc(point.X, point.Y, point.Z, point.M, nextPoint.X, nextPoint.Y, nextPoint.Z, nextPoint.M);
                        iterator++;
                    }
                }
            }
            else
            {
                _target.BeginGeometry(OpenGisGeometryType.Polygon);

                foreach (var point in _points)
                {
                    if (first)
                    {
                        _target.BeginFigure(point.X, point.Y, point.Z, point.M);
                        first = false;
                    }
                    else
                    {
                        _target.AddLine(point.X, point.Y, point.Z, point.M);
                    }
                }
            }

            _target.EndFigure();
            _target.EndGeometry();
        }
    }
}
