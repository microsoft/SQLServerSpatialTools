// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that finds a point along a geography linestring instance and pipes
     * it to another sink.
     */
    class PopulateGeometryMeasuresSink : IGeometrySink110
    {
        SqlGeometry _lastPoint;
        SqlGeometry _thisPoint;
        double _startMeasure;
        double _endMeasure;
        double _totalLength;
        double _currentLength = 0;
        int _srid;                     // The _srid we are working in.
        SqlGeometryBuilder _target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public PopulateGeometryMeasuresSink(double startMeasure, double endMeasure, double length, SqlGeometryBuilder target)
        {
            _target = target;
            _startMeasure = startMeasure;
            _endMeasure = endMeasure;
            _totalLength = length;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _srid = srid;
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            _target.SetSrid(_srid);
            _target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the starting point.
            _target.BeginFigure(latitude, longitude, null, _startMeasure);
            _lastPoint = SqlGeometry.Point(latitude, longitude, _srid);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            _thisPoint = SqlGeometry.Point(latitude, longitude, _srid);
            _currentLength += _lastPoint.STDistance(_thisPoint).Value;
            double currentM = _startMeasure + (_currentLength / _totalLength) * (_endMeasure - _startMeasure);
            _target.AddLine(latitude, longitude, null, currentM);
            _lastPoint = _thisPoint;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            _target.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            _target.EndGeometry();
        }

    }
}
