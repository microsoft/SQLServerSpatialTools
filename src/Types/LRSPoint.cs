//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Data structure to capture POINT geometry type.
    /// </summary>
    internal class LRSPoint
    {
        // Fields.
        private string _wkt;
        internal readonly double X;
        internal readonly double Y;
        internal double? Z, M;
        private readonly int _srid;
        internal double? Slope;
        private double _angle;
        internal SlopeValue SlopeType;

        internal double? OffsetBearing;
        private double _offsetAngle;
        internal double OffsetDistance;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="m">The m.</param>
        /// <param name="srid">The srid.</param>
        internal LRSPoint(double x, double y, double? z, double? m, int srid)
        {
            X = x;
            Y = y;
            Z = m.HasValue ? z : null;
            M = m ?? z;
            _srid = srid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSPoint"/> class.
        /// </summary>
        /// <param name="sqlGeometry">The SQL geometry.</param>
        internal LRSPoint(SqlGeometry sqlGeometry)
        {
            if (sqlGeometry.IsNullOrEmpty() || !sqlGeometry.IsPoint())
                return;

            X = sqlGeometry.STX.Value;
            Y = sqlGeometry.STY.Value;
            Z = sqlGeometry.HasZ ? sqlGeometry.Z.Value : (double?)null;
            M = sqlGeometry.HasM ? sqlGeometry.M.Value : (double?)null;
            _srid = (int)sqlGeometry.STSrid;
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Translating measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            M += offsetMeasure;
        }

        /// <summary>
        /// Scaling Measure of LRSPoint
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            M *= offsetMeasure;
        }

        /// <summary>
        /// Gets the distance from point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        internal double GetDistance(LRSPoint nextPoint)
        {
            return SpatialExtensions.GetDistance(X, Y, nextPoint.X, nextPoint.Y);
        }

        /// <summary>
        /// Determines whether X, Y co-ordinates of current and second point is within tolerance
        /// </summary>
        /// <param name="secondPoint">The second point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns>
        ///   <c>true</c> if X, Y co-ordinates of current and second point is within tolerance; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsXYWithinTolerance(LRSPoint secondPoint, double tolerance)
        {
            return (Math.Abs(X - secondPoint.X) <= tolerance && Math.Abs(Y - secondPoint.Y) <= tolerance);
        }

        /// <summary>
        /// Re calculate the measure.
        /// </summary>
        /// <param name="previousPoint">The previous point.</param>
        /// <param name="currentLength"></param>
        /// <param name="totalLength">The total length.</param>
        /// <param name="startMeasure">The start measure.</param>
        /// <param name="endMeasure">The end measure.</param>
        internal void ReCalculateMeasure(LRSPoint previousPoint, ref double currentLength, double totalLength, double startMeasure, double endMeasure)
        {
            currentLength += GetDistance(previousPoint);
            M = startMeasure + (currentLength / totalLength) * (endMeasure - startMeasure);
        }

        /// <summary>
        /// Gets the previous point.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="currentPoint">The current point.</param>
        /// <returns></returns>
        internal static LRSPoint GetPreviousPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
        {
            var index = points.IndexOf(currentPoint);
            // return null if index is -1; null for first point
            return index > 0 ? points[index - 1] : null;
        }

        /// <summary>
        /// Gets the next point.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="currentPoint">The current point.</param>
        /// <returns></returns>
        internal static LRSPoint GetNextPoint(ref List<LRSPoint> points, LRSPoint currentPoint)
        {
            var index = points.IndexOf(currentPoint);
            // return null if index is -1 or last index
            return index >= 0 && index < points.Count - 1 ? points[index + 1] : null;
        }

        #endregion

        #region Operator Overloading

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static LRSPoint operator -(LRSPoint a, LRSPoint b)
        {
            return new LRSPoint(b.X - a.X, b.Y - a.Y, null, null, a._srid);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>

        public static bool operator ==(LRSPoint a, LRSPoint b)
        {
            if (ReferenceEquals(b, null) && ReferenceEquals(a, null))
                return true;
            if (ReferenceEquals(b, null) || ReferenceEquals(a, null))
                return false;
            return a.X.EqualsTo(b.X) && a.Y.EqualsTo(b.Y) && EqualityComparer<double?>.Default.Equals(a.M, b.M);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">LRS Point 1.</param>
        /// <param name="b">LRS Point 2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(LRSPoint a, LRSPoint b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var point = obj as LRSPoint;
            return point != null &&
                   X.EqualsTo(point.X) &&
                   Y.EqualsTo(point.Y) &&
                   EqualityComparer<double?>.Default.Equals(M, point.M);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode()
        {
            var hashCode = 1911090832;
            hashCode = hashCode * 1521134295 + X.GetHashCode();
            hashCode = hashCode * 1521134295 + Y.GetHashCode();
            hashCode = hashCode * 1521134295 + EqualityComparer<double?>.Default.GetHashCode(Z);
            hashCode = hashCode * 1521134295 + EqualityComparer<double?>.Default.GetHashCode(M);
            return hashCode;
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode

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
            _wkt = $"POINT ({X} {Y} {M})";

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
        /// Converts to SqlGeometry.
        /// </summary>
        /// <param name="geometryBuilder">The geometry builder.</param>
        /// <returns></returns>
        internal SqlGeometry ToSqlGeometry(ref SqlGeometryBuilder geometryBuilder)
        {
            geometryBuilder.SetSrid(_srid);
            geometryBuilder.BeginGeometry(OpenGisGeometryType.Point);
            geometryBuilder.BeginFigure(X, Y, Z, M);
            geometryBuilder.EndFigure();
            geometryBuilder.EndGeometry();
            return geometryBuilder.ConstructedGeometry;
        }

        #endregion

        #region Parallel Point Computation

        /// <summary>
        /// Gets the offset point.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>Offset Point.</returns>
        private LRSPoint GetOffsetPoint(LRSPoint nextPoint)
        {
            return this - nextPoint;
        }

        /// <summary>
        /// Gets the arc to tangent.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <returns>In Radian</returns>
        private double GetAtanInRadian(LRSPoint nextPoint)
        {
            var offsetPoint = GetOffsetPoint(nextPoint);
            return Math.Atan2(offsetPoint.Y, offsetPoint.X);
        }

        /// <summary>
        /// Gets the atan2 in degrees.
        /// This does angle correct when atan2 value is negative
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <returns>Atan2 in degrees</returns>
        private double GetAtanInDegree(LRSPoint point1, LRSPoint point2)
        {
            var atan = point1.GetAtanInRadian(point2);
            return SpatialUtil.ToDegrees(atan <= 0 ? (2 * Math.PI) + atan : atan);
        }

        /// <summary>
        /// Gets the first point radian.
        /// </summary>
        /// <param name="previousPoint">The previous point.</param>
        /// <param name="middlePoint">The middle point.</param>
        /// <returns></returns>
        private double GetFirstPointRadian(LRSPoint previousPoint, LRSPoint middlePoint)
        {
            var atan = GetAtanInDegree(middlePoint, previousPoint);
            atan = 90 - atan;
            atan = atan <= 0 ? 360 + atan : atan;

            return SpatialUtil.ToRadians(360 - atan);
        }

        /// <summary>
        /// Gets the second point radian.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        /// <param name="middlePoint">The middle point.</param>
        /// <returns></returns>
        private double GetSecondPointRadian(LRSPoint nextPoint, LRSPoint middlePoint)
        {
            var atan = GetAtanInDegree(middlePoint, nextPoint);
            atan = 90 - atan;
            atan = atan <= 0 ? 360 + atan : atan;

            return SpatialUtil.ToRadians(180 - atan);
        }

        /// <summary>
        /// Gets the deviation angle of 3 points.
        /// </summary>
        /// <param name="pointA">The point a.</param>
        /// <param name="pointO">The point o.</param>
        /// <param name="pointB">The point b.</param>
        /// <param name="isNegativeOffset">if set to <c>true</c> [is negative offset].</param>
        /// <returns></returns>
        private double GetAOBAngle(LRSPoint pointA, LRSPoint pointO, LRSPoint pointB, bool isNegativeOffset)
        {
            const double angleCorrection = Math.PI / 2;
            const double angleConversion = 2 * Math.PI;

            var atanAo = pointA.GetAtanInRadian(pointO) + angleCorrection;
            var atanBo = pointB.GetAtanInRadian(pointO) + angleCorrection;

            // angle conversion
            atanAo = SpatialUtil.ToDegrees(atanAo <= 0 ? angleConversion + atanAo : atanAo);
            atanBo = SpatialUtil.ToDegrees(atanBo <= 0 ? angleConversion + atanBo : atanBo);

            var deviationAngle =
                360 - (atanAo > atanBo
                    ? 360 - (atanAo - atanBo)
                    : atanBo - atanAo);

            // for positive offset; offset curve will be to the left of input geom;
            // so for positive deviation angle the computed angle should be subtracted from 360
            // for negative offset; offset curve will be to the right of input geom
            return isNegativeOffset ? deviationAngle : 360 - deviationAngle;
        }

        /// <summary>
        /// Sets the offset bearing.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        internal void SetOffsetBearing(LRSPoint nextPoint)
        {
            if (nextPoint != null)
                OffsetBearing = CalculateOffsetBearing(nextPoint);
        }

        /// <summary>
        /// Calculates the offset bearing.
        /// </summary>
        /// <param name="nextPoint">The next point.</param>
        private double CalculateOffsetBearing(LRSPoint nextPoint)
        {
            _angle = SpatialUtil.ToDegrees(GetAtanInRadian(nextPoint));
            return (90 - _angle + 360) % 360;
        }

        /// <summary>
        /// Sets the offset angle.
        /// </summary>
        /// <param name="previousPoint">The current point.</param>
        /// <param name="isNegativeOffset">Is Offset is Negative</param>
        internal void SetOffsetAngle(LRSPoint previousPoint, bool isNegativeOffset)
        {
            _offsetAngle = CalculateOffsetAngle(previousPoint, isNegativeOffset);
        }

        /// <summary>
        /// Calculates the offset angle.
        /// </summary>
        /// <param name="previousPoint">The current point.</param>
        /// <param name="isNegativeOffset">Is Offset is Negative</param>
        private double CalculateOffsetAngle(LRSPoint previousPoint, bool isNegativeOffset)
        {
            double offsetAngle = 0;

            var previousPointOffsetBearing = previousPoint?.OffsetBearing;

            // Left
            if (!isNegativeOffset)
            {
                if (OffsetBearing == null)
                {
                    if (previousPointOffsetBearing != null) offsetAngle = (double)previousPointOffsetBearing - 90;
                }
                else if (previousPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing - 90;
                else
                    //(360 + b1.OffsetBearing - ((360 - ((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = (
                                      360 + (double)OffsetBearing -
                                      (
                                          (
                                              360 - (
                                                     ((double)previousPointOffsetBearing + 180) - (double)OffsetBearing
                                                    )
                                          ) / 2
                                      )
                                   ) % 360;
            }
            // Right
            else
            {

                if (OffsetBearing == null)
                {
                    if (previousPointOffsetBearing != null) offsetAngle = (double)previousPointOffsetBearing + 90;
                }
                else if (previousPointOffsetBearing == null)
                    offsetAngle = (double)OffsetBearing + 90;
                else
                    // (b1.OffsetBearing + ((((b2.OffsetBearing + 180) - b1.OffsetBearing)) / 2)) % 360
                    offsetAngle = ((double)OffsetBearing + (((((double)previousPointOffsetBearing + 180) - (double)OffsetBearing)) / 2)) % 360;
            }

            return offsetAngle;
        }

        /// <summary>
        /// Sets the offset distance.
        /// </summary>
        /// <param name="offset">The offset.</param>
        internal void SetOffsetDistance(double offset)
        {
            var offsetBearing = OffsetBearing ?? default(double);
            // offset / (SIN(RADIANS(((OffsetBearing - OffsetAngleLeft) + 360) % 360)))
            OffsetDistance = CalculateOffsetDistance(offset, offsetBearing, _offsetAngle);
        }

        /// <summary>
        /// Calculates the offset distance.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="offsetBearing"></param>
        /// <param name="offsetAngle"></param>
        private static double CalculateOffsetDistance(double offset, double offsetBearing, double offsetAngle)
        {
            // offset / (SIN(RADIANS(((OffsetBearing - OffsetAngleLeft) + 360) % 360)))
            var denominator = (Math.Sin(SpatialUtil.ToRadians(((offsetBearing - offsetAngle) + 360) % 360)));
            return offset / denominator;
        }

        /// <summary>
        /// Gets the parallel point.
        /// </summary>
        /// <returns>Point parallel to the current point.</returns>
        private LRSPoint GetParallelPoint()
        {
            var newX = X + (OffsetDistance * Math.Cos(SpatialUtil.ToRadians(90 - _offsetAngle)));
            var newY = Y + (OffsetDistance * Math.Sin(SpatialUtil.ToRadians(90 - _offsetAngle)));

            return new LRSPoint(
                newX,
                newY,
                null,
                M,
                _srid
                );
        }

        /// <summary>
        /// Compute and populate parallel points on bend lines.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="points">The points.</param>
        /// <param name="tolerance"></param>
        /// <returns>Point parallel to the current point.</returns>
        internal List<LRSPoint> GetAndPopulateParallelPoints(double offset, double tolerance, ref List<LRSPoint> points)
        {
            // list to capture additional vertices.
            var lrsPoints = new List<LRSPoint>();

            // parallel point to the current point
            var parallelPoint = GetParallelPoint();

            // get previous and next point
            var previousPoint = GetPreviousPoint(ref points, this);
            var nextPoint = GetNextPoint(ref points, this);

            // offset distance between parallel point and input point
            var diffInDistance = Math.Round(parallelPoint.GetDistance(this), 5);
            // offset distance difference between parallel point and input point
            var offsetDiff = Math.Abs(Math.Abs(diffInDistance) - Math.Abs(offset));

            if (offsetDiff <= tolerance || previousPoint == null || nextPoint == null)
            {
                lrsPoints.Add(parallelPoint);
            }
            else
            {
                var negativeOffset = offset < 0;
                var deviationAngle = GetAOBAngle(previousPoint, this, nextPoint, negativeOffset);

                if (deviationAngle <= 90)
                {
                    var firsPointRadian = GetFirstPointRadian(previousPoint, this);
                    var nextPointRadian = GetSecondPointRadian(nextPoint, this);

                    // first point
                    var firstPointX = X + (offset * Math.Cos(firsPointRadian));
                    var firstPointY = Y + (offset * Math.Sin(firsPointRadian));
                    var firstPoint = new LRSPoint(firstPointX, firstPointY, null, M, _srid);

                    // second point
                    var secondPointX = X + (offset * Math.Cos(nextPointRadian));
                    var secondPointY = Y + (offset * Math.Sin(nextPointRadian));
                    var secondPoint = new LRSPoint(secondPointX, secondPointY, null, M, _srid);

                    // if computed first point is within tolerance of second point then add only first point
                    if (firstPoint.GetDistance(secondPoint) <= tolerance)
                    {
                        lrsPoints.Add(firstPoint);
                    }
                    else
                    {
                        // add first point
                        lrsPoints.Add(firstPoint);

                        // compute middle point
                        var fraction = Math.Abs(offset / OffsetDistance);
                        var middleX = (X * (1 - fraction)) + (parallelPoint.X * fraction);
                        var middleY = (Y * (1 - fraction)) + (parallelPoint.Y * fraction);
                        var middlePoint = new LRSPoint(middleX, middleY, null, M, _srid);

                        // if not within tolerance add middle point
                        if (firstPoint.GetDistance(middlePoint) > tolerance)
                            lrsPoints.Add(middlePoint);

                        // add second point
                        lrsPoints.Add(secondPoint);
                    }
                }
                else
                    lrsPoints.Add(parallelPoint);
            }

            return lrsPoints;
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        internal void CalculateSlope(LRSPoint nextLRSPoint)
        {
            Slope = GetSlope(nextLRSPoint, out SlopeType);
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        /// <param name="nextLRSPoint">The next LRS point.</param>
        /// <param name="slopeValue"></param>
        internal double? GetSlope(LRSPoint nextLRSPoint, out SlopeValue slopeValue)
        {
            slopeValue = SlopeValue.None;
            var xDifference = nextLRSPoint.X - X;
            var yDifference = nextLRSPoint.Y - Y;

            if (xDifference.EqualsTo(0))
            {
                slopeValue = yDifference > 0 ? SlopeValue.PositiveInfinity : SlopeValue.NegativeInfinity;
                return null;
            }
            else if (yDifference.EqualsTo(0))
            {
                slopeValue = xDifference > 0 ? SlopeValue.PositiveZero : SlopeValue.NegativeZero;
                return null;
            }

            return yDifference / xDifference;
        }

        #endregion
    }
}