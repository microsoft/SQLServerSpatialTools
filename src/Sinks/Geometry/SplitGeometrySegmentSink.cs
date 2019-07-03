// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that finds a point along a geography linestring instance and pipes
     * it to another sink.
     */
    class SplitGeometrySegmentSink : IGeometrySink110
    {
        SqlGeometry _splitPoint;
        int _srid;                     // The _srid we are working in.
        SqlGeometryBuilder _target1;    // Where we place our result.
        SqlGeometryBuilder _target2;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public SplitGeometrySegmentSink(SqlGeometry splitPoint, SqlGeometryBuilder target1, SqlGeometryBuilder target2)
        {
            _target1 = target1;
            _target2 = target2;
            _splitPoint = splitPoint;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _srid = srid;
            _target1.SetSrid(_srid);
            _target2.SetSrid(_srid);
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");
            
            _target1.BeginGeometry(OpenGisGeometryType.LineString);            
            _target2.BeginGeometry(OpenGisGeometryType.LineString);
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the starting point.
            _target1.BeginFigure(latitude, longitude, z, m);
            _target2.BeginFigure(_splitPoint.STX.Value, _splitPoint.STY.Value, _splitPoint.Z.IsNull?(double?)null: _splitPoint.Z.Value, _splitPoint.M.Value);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // If current measure is between start measure and end measure, we should add segment to the first result linestring
            if (m < _splitPoint.M.Value)
            {
                _target1.AddLine(latitude, longitude, z, m);
            }

            if (m > _splitPoint.M.Value)
            {
                _target2.AddLine(latitude, longitude, z, m);
            }

        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            _target1.AddLine(_splitPoint.STX.Value, _splitPoint.STY.Value, null, _splitPoint.M.Value);
            _target1.EndFigure();
            _target2.EndFigure();
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            _target1.EndGeometry();
            _target2.EndGeometry();
        }

    }
}
