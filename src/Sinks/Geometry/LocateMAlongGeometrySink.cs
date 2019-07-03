// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using System;
using System.Data.SqlTypes;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that finds a point along a geography linestring instance and pipes
     * it to another sink.
     */
    class LocateMAlongGeometrySink : IGeometrySink110
    {

        double _measure;              // The running count of how much further we have to go.
        SqlGeometry _lastPoint;        // The last point in the LineString we have passed.
        SqlGeometry _foundPoint;       // This is the point we're looking for, assuming it isn't null, we're done.
        int _srid;                     // The _srid we are working in.
        SqlGeometryBuilder _target;    // Where we place our result.

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a measure, which is the point along the input linestring we will travel to.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public LocateMAlongGeometrySink(double measure, SqlGeometryBuilder target)
        {
            _target = target;
            _measure = measure;
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
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Memorize the point.
            //_lastPoint = SqlGeometry.Point(latitude, longitude, _srid);
            _lastPoint = SqlGeometry.STPointFromText(new SqlChars("POINT(" + latitude.ToString() + " " + longitude.ToString() + " " + ((z == null)?"NULL":z.ToString()) + " " + m.ToString() + ")"), _srid);
        }

        // This is where the real work is done.
        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // If we've already found a point, then we're done.  We just need to keep ignoring these
            // pesky calls.
            if (_foundPoint != null) return;

            // Make a point for our current position.
            //SqlGeometry thisPoint = SqlGeometry.Point(latitude, longitude, _srid);
            SqlGeometry thisPoint = SqlGeometry.STPointFromText(new SqlChars("POINT(" + latitude.ToString() + " " + longitude.ToString() + " " + ((z == null) ? "NULL" : z.ToString()) + " " + m.ToString() + ")"), _srid);

            // is the found point between this point and the last, or past this point?
            double rest =_measure - (double)m;
            if (rest > 0)
            {
                // it's past this point---just step along the line
                //_measure -= length;
                _lastPoint = thisPoint;
            }
            else
            {
                // now we need to do the hard work and find the point in between these two
                _foundPoint = Functions.InterpolateMBetweenGeom(_lastPoint, thisPoint, _measure);
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            if (_foundPoint != null)
            {
                // We could use a simple point constructor, but by targetting another sink we can use this
                // class in a pipeline.
                _target.SetSrid(_srid);
                _target.BeginGeometry(OpenGisGeometryType.Point);
                _target.BeginFigure(_foundPoint.STX.Value, _foundPoint.STY.Value, _foundPoint.Z.IsNull?(double?)null: _foundPoint.Z.Value, _foundPoint.M.Value);
                _target.EndFigure();
                _target.EndGeometry();
            }
            else
            {
                throw new ArgumentException("Measure provided is greater than maximum measure of LineString.");
            }
        }

    }
}
