//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Data structure to capture LINESTRING geometry type.
    /// </summary>
    internal class LRSLine : IEnumerable
    {
        private List<LRSPoint> _points;
        private string _wkt;
        internal int SRID;
        internal bool IsInRange;
        internal bool IsCompletelyInRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSLine"/> class.
        /// </summary>
        /// <param name="srid">The srid.</param>
        internal LRSLine(int srid)
        {
            SRID = srid;
            _points = new List<LRSPoint>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsEmpty => !_points.Any() || _points.Count == 0;

        /// <summary>
        /// Gets a value indicating whether this instance has points.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has points; otherwise, <c>false</c>.
        /// </value>
        public bool HasPoints => _points != null && _points.Any();

        /// <summary>
        /// Gets the number of POINT in this LINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _points.Any() ? _points.Count : 0;

        /// <summary>
        /// Determines whether this instance is LINESTRING.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is line; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsLine => _points.Any() && _points.Count > 1;

        /// <summary>
        /// Determines whether this instance is POINT.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is point; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsPoint => _points.Any() && _points.Count == 1;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        internal double Length { get; private set; }

        #region Add Points

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="lrsPoint">The l rs point.</param>
        internal void AddPoint(LRSPoint lrsPoint)
        {
            UpdateLength(lrsPoint);
            _points.Add(lrsPoint);
        }

        /// <summary>
        /// Adds multiple points.
        /// </summary>
        /// <param name="lrsPoints">List of lrs point.</param>
        internal void AddPoint(List<LRSPoint> lrsPoints)
        {
            lrsPoints.ForEach(AddPoint);
        }

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="points">The lrs points.</param>
        internal void AddPoint(params LRSPoint[] points)
        {
            foreach (var lrsPoint in points)
            {
                AddPoint(lrsPoint);
            }
        }

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="pointGeometry">The SqlGeometry point.</param>
        internal void AddPoint(SqlGeometry pointGeometry)
        {
            AddPoint(new LRSPoint(pointGeometry));
        }

        /// <summary>
        /// Adds the point.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        internal void AddPoint(double x, double y, double? z, double? m)
        {
            AddPoint(new LRSPoint(x, y, z, m, SRID));
        }

        /// <summary>
        /// Updates the length.
        /// </summary>
        /// <param name="currentPoint">The current point.</param>
        private void UpdateLength(LRSPoint currentPoint)
        {
            if (!_points.Any() || _points.Count <= 0) return;
            var previousPoint = _points.Last();
            Length += previousPoint.GetDistance(currentPoint);
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Reverses the points of the line.
        /// </summary>
        internal void ReversePoints()
        {
            _points.Reverse();
        }

        /// <summary>
        /// Gets the start point.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetStartPoint()
        {
            return _points.Any() ? _points.First() : null;
        }

        /// <summary>
        /// Gets the start point measure.
        /// </summary>
        /// <returns></returns>
        internal double GetStartPointM()
        {
            var currentPoint = GetStartPoint().M;
            return currentPoint ?? 0;
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetEndPoint()
        {
            return _points.Any() ? _points.Last() : null;
        }

        /// <summary>
        /// Gets the end point m.
        /// </summary>
        /// <returns></returns>
        internal double GetEndPointM()
        {
            var currentPoint = GetEndPoint().M;
            return currentPoint ?? 0;
        }

        /// <summary>
        /// Locates the point.
        /// </summary>
        /// <param name="measure">The measure.</param>
        /// <param name="firstPoint"></param>
        /// <returns></returns>
        internal LRSPoint LocatePoint(double measure, LRSPoint firstPoint = null)
        {
            var startPoint = firstPoint ?? GetStartPoint();
            var endPoint = GetEndPoint();

            if (startPoint.M == null || endPoint.M == null) return null;

            var fraction = (measure - startPoint.M.Value) / (endPoint.M.Value - startPoint.M.Value);
            var newX = (startPoint.X * (1 - fraction)) + (endPoint.X * fraction);
            var newY = (startPoint.Y * (1 - fraction)) + (endPoint.Y * fraction);

            return new LRSPoint(newX, newY, null, measure, SRID);
        }

        /// <summary>
        /// Gets the point at m.
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetPointAtM(double measure)
        {
            return _points.FirstOrDefault(e => e.M.EqualsTo(measure));
        }

        /// <summary>
        /// Scale the existing measure of geometry by multiplying existing measure with offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            _points.ForEach(point => point.ScaleMeasure(offsetMeasure));
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            _points.ForEach(point => point.TranslateMeasure(offsetMeasure));
        }

        /// <summary>
        /// Removes the collinear points.
        /// </summary>
        internal void RemoveCollinearPoints()
        {
            // If the input segment has only two points; then there is no way of collinear
            // so its no-op
            if (_points.Count <= 2)
                return;

            var pointCounter = 0;
            var pointsToRemove = new List<LRSPoint>();

            foreach (var point in this)
            {
                // ignore first point and last point
                if (pointCounter == 0 || pointCounter == _points.Count - 1)
                {
                    pointCounter++;
                    continue;
                }

                // previous point is A
                var previousPoint = _points[pointCounter - 1];
                // slope AB
                var slopeAB = previousPoint.Slope;
                var slopeABType = previousPoint.SlopeType;

                // current point is B, slope is BC
                var slopeBC = point.Slope;
                var slopeBCType = point.SlopeType;

                // next point is C
                var nextPoint = _points[pointCounter + 1];
                // calculate AC slope
                var slopeAC = previousPoint.GetSlope(nextPoint, out var slopeACType);

                // for each point previous and next point slope is compared
                // if slope of AB = BC = CA
                var isABBCSlopeEqual = slopeAB.HasValue && slopeBC.HasValue
                    ? slopeAB.Value.EqualsTo(slopeBC.Value)
                    : slopeABType == slopeBCType;
                var isBCACSlopeEqual = slopeBC.HasValue && slopeAC.HasValue
                    ? slopeBC.Value.EqualsTo(slopeAC.Value)
                    : slopeBCType == slopeACType;

                if (isABBCSlopeEqual && isBCACSlopeEqual)
                    pointsToRemove.Add(point);
                pointCounter++;
            }

            // remove the collinear points
            RemovePoints(pointsToRemove);
        }

        /// <summary>
        /// Removes the points.
        /// </summary>
        /// <param name="pointsToRemove">The points to remove.</param>
        internal void RemovePoints(List<LRSPoint> pointsToRemove)
        {
            pointsToRemove.ForEach(pointToRemove => _points.Remove(pointToRemove));
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        internal void CalculateSlope()
        {
            var pointIterator = 1;
            foreach (var point in _points)
            {
                // last point; next point is the first point
                point.CalculateSlope(pointIterator == _points.Count ? _points.First() : _points[pointIterator]);

                pointIterator++;
            }
        }

        #endregion

        #region Data Structure Conversions

        /// <summary>
        /// Converts to WKT format.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsEmpty)
            {
                _wkt = "LINESTRING EMPTY";
                return _wkt;
            }

            var wktBuilder = new StringBuilder();

            if (IsLine)
                wktBuilder.Append("LINESTRING (");
            else if (IsPoint)
                wktBuilder.Append("POINT (");

            var pointIterator = 1;

            foreach (var point in _points)
            {
                wktBuilder.AppendFormat("{0} {1} {2}", point.X, point.Y, point.M);
                if (pointIterator != _points.Count)
                    wktBuilder.Append(", ");
                pointIterator++;
            }
            wktBuilder.Append(")");
            _wkt = wktBuilder.ToString();

            return _wkt;
        }

        /// <summary>
        /// Converts to SqlGeometry.
        /// </summary>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            return ToSqlGeometry(ref geomBuilder);
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <param name="geomBuilder">Reference SqlGeometryBuilder to be used for building Geometry.</param>
        /// <returns>SqlGeometry</returns>
        internal SqlGeometry ToSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            // ignore if the line has only one point.
            if (_points.Count < 2)
                return GetStartPoint().ToSqlGeometry(ref geomBuilder);
            BuildSqlGeometry(ref geomBuilder);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Builds the SqlGeometry form LINESTRING or POINT
        /// </summary>
        /// <param name="geomBuilder">The geom builder.</param>
        /// <param name="isFromMultiLine"></param>
        internal void BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder, bool isFromMultiLine = false)
        {
            if (!isFromMultiLine)
                geomBuilder.SetSrid(SRID);
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            var pointIterator = 1;
            foreach (var point in _points)
            {
                if (pointIterator == 1)
                    geomBuilder.BeginFigure(point.X, point.Y, point.Z, point.M);
                else
                    geomBuilder.AddLine(point.X, point.Y, point.Z, point.M);
                pointIterator++;
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
        }

        #endregion

        #region Modules for Offset Angle Calculation
        /// <summary>
        /// Calculate offset bearings for all points.
        /// </summary>
        private void CalculateOffsetBearing()
        {
            var pointCount = _points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = _points[i];
                if (i != pointCount - 1)
                {
                    var nextPoint = _points[i + 1];
                    currentPoint.SetOffsetBearing(nextPoint);
                }
                else
                {
                    currentPoint.OffsetBearing = null;
                }
            }
        }

        /// <summary>
        /// Calculate offset angle between points
        /// </summary>
        /// <param name="isNegativeOffset"></param>
        private void CalculateOffsetAngle(bool isNegativeOffset)
        {
            var pointCount = _points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = _points[i];
                // first point
                if (i == 0)
                {
                    _points[i].SetOffsetAngle(null, isNegativeOffset);
                }
                // except first point
                else
                {
                    var previousPoint = _points[i - 1];
                    currentPoint.SetOffsetAngle(previousPoint, isNegativeOffset);
                }
            }
        }

        /// <summary>
        /// Calculate offset distance between points 
        /// </summary>
        /// <param name="offset">Offset value</param>
        private void CalculateOffset(double offset)
        {
            var pointCount = _points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                var currentPoint = _points[i];
                // if not last point
                if (i != pointCount - 1)
                {
                    // other points point
                    currentPoint.SetOffsetDistance(offset);
                }
                else
                {
                    // for the last point specified offset is offset distance.
                    // if decrease offset distance should be negative
                    currentPoint.OffsetDistance = Math.Abs(offset);
                }
            }
        }

        /// <summary> 
        /// Compute a parallel line to input line segment 
        /// </summary>
        /// <param name="offset">Offset Value</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>Parallel Line</returns>
        internal LRSLine ComputeParallelLine(double offset, double tolerance)
        {
            CalculateOffsetBearing();
            CalculateOffsetAngle(offset < 0);
            CalculateOffset(offset);

            var parallelLine = new LRSLine(SRID);

            // compute parallel point and add additional vertices
            foreach (var point in _points)
            {
                parallelLine.AddPoint(point.GetAndPopulateParallelPoints(offset, tolerance, ref _points));
            }

            return parallelLine;
        }

        /// <summary>
        /// Populates the measures.
        /// </summary>
        /// <param name="segmentLength">Length of the segment.</param>
        /// <param name="currentLength">Length of the current.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        /// <returns></returns>
        internal void PopulateMeasures(double segmentLength, ref double currentLength, double startMeasure, double endMeasure)
        {
            var pointCount = _points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                if (i == pointCount - 1)
                    _points[i].M = GetEndPointM();
                else if (i == 0)
                    _points[i].M = GetStartPointM();
                else
                {
                    var currentPoint = _points[i];
                    currentPoint.ReCalculateMeasure(_points[i - 1], ref currentLength, segmentLength, startMeasure, endMeasure);
                }
            }
        }

        #endregion

        #region Enumeration

        /// <summary>
        /// Returns an enumerator that iterates through each POINT in a LINESTRING.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public LRSEnumerator<LRSPoint> GetEnumerator()
        {
            return new LRSEnumerator<LRSPoint>(_points);
        }

        #endregion
    }
}