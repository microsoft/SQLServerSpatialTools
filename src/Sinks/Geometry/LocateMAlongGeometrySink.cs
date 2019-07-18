//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using System;
using SQLSpatialTools.Utility;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that finds a point along a geometry linestring instance and pipes
    /// it to another sink.
    /// </summary>
    internal class LocateMAlongGeometrySink : IGeometrySink110
    {
        private readonly double _measure;             // The running count of how much further we have to go.
        private readonly double _tolerance;           // The tolerance.
        private readonly SqlGeometryBuilder _target;  // Where we place our result.
        private int _srid;                            // The srid we are working in.
        private SqlGeometry _lastPoint;               // The last point in the LineString we have passed.
        private SqlGeometry _foundPoint;              // This is the point we're looking for, assuming it isn't null, we're done.
        public bool IsPointDerived;
        public bool IsShapePoint;

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a measure, which is the point along the input linestring we will travel to.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public LocateMAlongGeometrySink(double measure, SqlGeometryBuilder target, double tolerance = Constants.Tolerance)
        {
            _target = target;
            _measure = measure;
            _tolerance = tolerance;
            IsPointDerived = false;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _srid = srid;
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
        }

        // Start the figure.  Note that since we only operate on LineStrings, this should only be executed
        // once.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (_foundPoint != null)
                return;

            // Memorize the point.
            _lastPoint = CheckShapePointAndGet(m, Ext.GetPoint(x, y, z, m, _srid));
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // If we've already found a point, then we're done.  We just need to keep ignoring these
            // pesky calls.
            if (_foundPoint != null)
                return;

            // Make a point for our current position.
            var thisPoint = CheckShapePointAndGet(m, Ext.GetPoint(x, y, z, m, _srid));

            // is the found point between this point and the last, or past this point?
            if (m != null && _measure.IsWithinRange(_lastPoint.M.Value, m.Value))
            {
                // now we need to do the hard work and find the point in between these two
                _foundPoint = Functions.LRS.Geometry.InterpolateBetweenGeom(_lastPoint, thisPoint, _measure);
                if (_lastPoint.IsWithinTolerance(_foundPoint, _tolerance))
                {
                    _foundPoint = _lastPoint;
                    IsShapePoint = true;
                }
                else if (thisPoint.IsWithinTolerance(_foundPoint, _tolerance))
                {
                    _foundPoint = thisPoint;
                    IsShapePoint = true;
                }
            }
            else
            {
                // it's past this point---just step along the line
                _lastPoint = thisPoint;
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        /// <summary>
        /// Checks if the point is a shape point.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        private SqlGeometry CheckShapePointAndGet(double? m, SqlGeometry geometry)
        {
            IsShapePoint = m.EqualsTo(_measure);
            if (IsShapePoint)
                _foundPoint = geometry;
            return geometry;
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // When we end, we'll make all of our output calls to our target.
        // Here's also where we catch whether we've run off the end of our LineString.
        public void EndGeometry()
        {
            if (_foundPoint == null || IsPointDerived) return;
            // We could use a simple point constructor, but by targeting another sink we can use this
            // class in a pipeline.
            _target.SetSrid(_srid);
            _target.BeginGeometry(OpenGisGeometryType.Point);
            _target.BeginFigure(_foundPoint.STX.Value, _foundPoint.STY.Value, _foundPoint.Z.IsNull ? (double?)null : _foundPoint.Z.Value, _foundPoint.M.Value);
            _target.EndFigure();
            _target.EndGeometry();
            IsPointDerived = true;
        }
    }
}
