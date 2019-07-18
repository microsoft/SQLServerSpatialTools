//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that clips a line segment based on measure.
    /// </summary>
    internal class ClipMGeometrySegmentSink : IGeometrySink110
    {
        // Where we place our result
        private readonly SqlGeometryBuilder _target;
        private readonly double _tolerance;
        private readonly bool _retainClipMeasure;

        private readonly double _clipStartMeasure;
        private readonly double _clipEndMeasure;
        private double _previousX;
        private double _previousY;
        private double _previousM;
        private bool _started;
        private bool _finished;

        // The _srid we are working in.
        private int _srid;

        /// <summary>
        /// We target another builder, to which we will send a point representing the point we find.
        /// We also take a distance, which is the point along the input linestring we will travel.
        /// Note that we only operate on LineString instances: anything else will throw an exception.
        /// </summary>
        /// <param name="startMeasure">Start Measure to be clipped</param>
        /// <param name="endMeasure">End Measure to be clipped</param>
        /// <param name="target">SqlGeometry builder</param>
        /// <param name="tolerance">tolerance value</param>
        /// <param name="retainClipMeasure">Flag to retain ClipMeasure values</param>
        public ClipMGeometrySegmentSink(double startMeasure, double endMeasure, SqlGeometryBuilder target, double tolerance, bool retainClipMeasure = false)
        {
            _target = target;
            _clipStartMeasure = startMeasure;
            _clipEndMeasure = endMeasure;
            _tolerance = tolerance;
            _retainClipMeasure = retainClipMeasure;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _srid = srid;
            _target.SetSrid(_srid);
        }

        // Start the geometry.  Throw if it isn't a LineString.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type != OpenGisGeometryType.LineString)
                throw new ArgumentException("This operation may only be executed on LineString instances.");

            _target.BeginGeometry(OpenGisGeometryType.LineString);
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            // Add the first shape point if either the start or end measure matches the clip start and end measure
            if (m.EqualsTo(_clipStartMeasure) || m.EqualsTo(_clipEndMeasure))
            {
                _target.BeginFigure(x, y, z, m);
                _started = true;
            }

            UpdateLastPoint(x, y, m);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // To unify code for ascending and descending measures - clipPointMeasure
            double? clipPointMeasure = GetClipPointMeasure(m);
            double newX, newY;

            // If current measure is between start measure and end measure, 
            // we should add segment to the result linestring
            if (m.IsWithinRange(_clipStartMeasure, _clipEndMeasure))
            {
                // if the geometry is started, just add the point to line
                if (_started)
                {
                    _target.AddLine(x, y, z, m);
                }
                // Else we need to begin the geom figure first
                else
                {
                    var isShapePoint = false;
                    // if clip point is shape point measure then add the point without computation
                    if (m.EqualsTo(clipPointMeasure))
                    {
                        _target.BeginFigure(x, y, null, m);
                        isShapePoint = true;
                    }
                    else
                    {
                        ComputePointCoordinates(clipPointMeasure, m, x, y, out newX, out newY);

                        // if computed point is within tolerance of last point then begin figure with last point
                        if (Ext.IsWithinTolerance(_previousX, _previousY, newX, newY, _tolerance))
                            _target.BeginFigure(_previousX, _previousY, null, _retainClipMeasure ? _clipStartMeasure : _previousM);
                        
                        // check with current point against new computed point
                        else if (Ext.IsWithinTolerance(x, y, newX, newY, _tolerance))
                        {
                            _target.BeginFigure(x, y, null, m);
                            isShapePoint = true;
                        }
                        
                        // else begin figure with clipped point
                        else
                            _target.BeginFigure(newX, newY, null, clipPointMeasure);
                    }

                    _started = true;
                    if (_clipStartMeasure.EqualsTo(_clipEndMeasure) || isShapePoint)
                    {
                        UpdateLastPoint(x, y, m);
                        return;
                    }

                    _target.AddLine(x, y, z, m);
                }
            }
            // We may still need to add last segment, 
            // if current point is the first one after we passed range of interest
            else
            {
                if (!_started)
                {
                    var isShapePoint = false;
                    if (clipPointMeasure.IsWithinRange(m, _previousM))
                    {
                        ComputePointCoordinates(clipPointMeasure, m, x, y, out newX, out newY);

                        // if computed point is within tolerance of last point then begin figure with last point
                        if (Ext.IsWithinTolerance(_previousX, _previousY, newX, newY, _tolerance))
                            _target.BeginFigure(_previousX, _previousY, null, _retainClipMeasure ? clipPointMeasure : _previousM);
                        // check with current point against new computed point
                        else if (Ext.IsWithinTolerance(x, y, newX, newY, _tolerance))
                        {
                            _target.BeginFigure(x, y, null, _retainClipMeasure ? clipPointMeasure : m);
                            isShapePoint = true;
                        }
                        // else begin figure with clipped point
                        else
                            _target.BeginFigure(newX, newY, null, clipPointMeasure);

                        _started = true;
                        if (_clipStartMeasure.EqualsTo(_clipEndMeasure) || isShapePoint)
                        {
                            UpdateLastPoint(x, y, m);
                            return;
                        }
                    }
                }
                if (_started && !_finished)
                {
                    // re calculate clip point measure as it can be changed from above condition.
                    clipPointMeasure = GetClipPointMeasure(m);

                    if (clipPointMeasure.IsWithinRange(m, _previousM))
                    {
                        ComputePointCoordinates(clipPointMeasure, m, x, y, out newX, out newY);

                        var isWithinLastPoint = Ext.IsWithinTolerance(_previousX, _previousY, newX, newY, _tolerance);
                        var isWithinCurrentPoint = Ext.IsWithinTolerance(x, y, newX, newY, _tolerance);

                        // if computed point is within tolerance of last point then skip
                        if (!isWithinLastPoint)
                        {
                            // if within current point then add current point
                            if (isWithinCurrentPoint)
                                _target.AddLine(x, y, null, _retainClipMeasure ? clipPointMeasure : m);
                            // else add computed point
                            else
                                _target.AddLine(newX, newY, null, clipPointMeasure);
                        }

                        _finished = true;
                    }
                }
            }

            // re-assign the current co-ordinates to match for next iteration.
            UpdateLastPoint(x, y, m);
        }

        private void UpdateLastPoint(double x, double y, double? m)
        {
            _previousX = x;
            _previousY = y;
            _previousM = m ?? 0;
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
            _started = false;
            _finished = false;
            _previousX = default(double);
            _previousY = default(double);
            _previousM = default(double);
        }

        /// <summary>
        /// Gets the clip start or end measure.
        /// </summary>
        /// <param name="currentPointMeasure">The current point measure.</param>
        /// <returns>Start or End Clip Measure</returns>
        private double GetClipPointMeasure(double? currentPointMeasure)
        {
            double clipPointMeasure;
            // increasing measures
            if(_previousM < currentPointMeasure)
                clipPointMeasure = _started ? Math.Max(_clipStartMeasure, _clipEndMeasure) : Math.Min(_clipStartMeasure, _clipEndMeasure);
            // decreasing measures
            else
                clipPointMeasure = _started ? Math.Min(_clipStartMeasure, _clipEndMeasure) : Math.Max(_clipStartMeasure, _clipEndMeasure);

            return clipPointMeasure;
        }

        /// <summary>
        /// Computes the point coordinates.
        /// </summary>
        /// <param name="computePointMeasure">The compute point measure.</param>
        /// <param name="currentPointMeasure">The current point measure.</param>
        /// <param name="currentXCoordinate">The current x coordinate.</param>
        /// <param name="currentYCoordinate">The current y coordinate.</param>
        /// <param name="newX">The new x.</param>
        /// <param name="newY">The new y.</param>
        private void ComputePointCoordinates(double? computePointMeasure, double? currentPointMeasure,
            double currentXCoordinate, double currentYCoordinate, out double newX, out double newY)
        {
            newX = 0.0;
            newY = 0.0;
            if (!currentPointMeasure.HasValue || !computePointMeasure.HasValue)
                return;

            var currentM = (double) currentPointMeasure;
            // The fraction of the way from start to end.
            var fraction = ((double) computePointMeasure - _previousM) / (currentM - _previousM);
            newX = (_previousX * (1 - fraction)) + (currentXCoordinate * fraction);
            newY = (_previousY * (1 - fraction)) + (currentYCoordinate * fraction);
        }
    }
}
