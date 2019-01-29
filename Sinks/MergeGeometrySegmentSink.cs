// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that finds a point along a geography linestring instance and pipes
     * it to another sink.
     */
    class MergeGeometrySegmentSink : IGeometrySink110
    {
        int _srid;                     // The _srid we are working in.
        SqlGeometryBuilder _target;    // Where we place our result.
        bool _isFirst;                 // We begin geometry/figure for the first one only 
        bool _isLast;                  // We end geometry/figure for the last one only
        double? _mOffset;               // If measures at the merge point are not equal, measures for second geometry will be adjusted

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public MergeGeometrySegmentSink(SqlGeometryBuilder target, bool isFirst, bool isLast, double? mOffset)
        {
            _target = target;
            _isFirst = isFirst;
            _isLast = isLast;
            _mOffset = mOffset;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            if (_isFirst)
            {
                _srid = srid;
                _target.SetSrid(_srid);
            }
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            if (_isFirst)
                _target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the starting point.
            if (_isFirst)
                _target.BeginFigure(latitude, longitude, z, m);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            if (_isFirst)
                _target.AddLine(latitude, longitude, z, m);
            else
                _target.AddLine(latitude, longitude, z, m + _mOffset);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            if (_isLast)
                _target.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            if (_isLast)
                _target.EndGeometry();
        }

    }
}
