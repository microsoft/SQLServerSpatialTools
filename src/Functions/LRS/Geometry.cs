//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;
using Ext = SQLSpatialTools.Utility.SpatialExtensions;

namespace SQLSpatialTools.Functions.LRS
{
    /// <summary>
    /// This provides LRS data manipulation on planar Geometry data type.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Clip a geometry segment based on specified measure.
        /// <br /> If the clipped start and end point is within tolerance of shape point then shape point is returned as start and end of clipped Geom segment. 
        /// <br /> This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="clipStartMeasure">Start Measure</param>
        /// <param name="clipEndMeasure">End Measure</param>
        /// <param name="tolerance">Tolerance Value</param>
        /// <returns>Clipped Segment</returns>
        public static SqlGeometry ClipGeometrySegment(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance = Constants.Tolerance)
        {
            return ClipAndRetainMeasure(geometry, clipStartMeasure, clipEndMeasure, tolerance, false);
        }

        /// <summary>
        /// Clip a geometry segment and retains its measure.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="clipStartMeasure">The clip start measure.</param>
        /// <param name="clipEndMeasure">The clip end measure.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="retainMeasure">if set to <c>true</c> [retain measure].</param>
        /// <returns></returns>
        private static SqlGeometry ClipAndRetainMeasure(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance, bool retainMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if not multiline just return the line segment or point post clipping
            if (!geometry.IsMultiLineString())
                return ClipLineSegment(geometry, clipStartMeasure, clipEndMeasure, tolerance, retainMeasure);

            // for multi line
            var multiLine = geometry.GetLRSMultiLine();

            var clippedSegments = new List<SqlGeometry>();
            foreach (var line in multiLine)
            {
                var segment = ClipLineSegment(line.ToSqlGeometry(), clipStartMeasure, clipEndMeasure, tolerance, retainMeasure);
                // add only line segments
                if (segment.IsNotNullOrEmpty())
                    clippedSegments.Add(segment);
            }

            if (!clippedSegments.Any()) return SqlGeometry.Null;
            {
                // if one segment then it is a POINT or LINESTRING, so return straight away.
                if (clippedSegments.Count == 1)
                    return clippedSegments.First();

                var geomBuilder = new SqlGeometryBuilder();
                // count only LINESTRING
                var multiLineGeomSink = new BuildMultiLineFromLinesSink(geomBuilder, clippedSegments.Count(segment => segment.IsLineString()));

                foreach (var geom in clippedSegments)
                {
                    // ignore points
                    if (geom.IsLineString())
                        geom.Populate(multiLineGeomSink);
                }

                return geomBuilder.ConstructedGeometry;
            }

        }

        /// <summary>
        /// Clip a geometry segment based on specified measure.
        /// <br /> If the clipped start and end point is within tolerance of shape point then shape point is returned as start and end of clipped Geom segment.
        /// <br /> This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="clipStartMeasure">Start Measure</param>
        /// <param name="clipEndMeasure">End Measure</param>
        /// <param name="tolerance">Tolerance Value</param>
        /// <param name="retainClipMeasure">Flag to retain clip measures</param>
        /// <returns>Clipped Segment</returns>
        private static SqlGeometry ClipLineSegment(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance, bool retainClipMeasure)
        {
            var startMeasureInvalid = false;
            var endMeasureInvalid = false;

            // reassign clip start and end measure based upon there difference
            if (clipStartMeasure > clipEndMeasure)
            {
                var shiftObj = clipStartMeasure;
                clipStartMeasure = clipEndMeasure;
                clipEndMeasure = shiftObj;
            }

            // if point then compute here and return
            if (geometry.IsPoint())
            {
                var pointMeasure = geometry.HasM ? geometry.M.Value : 0;
                var isClipMeasureEqual = clipStartMeasure.EqualsTo(clipEndMeasure);
                // no tolerance check, if both start and end measure is point measure then return point
                if (isClipMeasureEqual && pointMeasure.EqualsTo(clipStartMeasure))
                    return geometry;

                if (isClipMeasureEqual && (clipStartMeasure > pointMeasure || clipStartMeasure < pointMeasure))
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);
                // if clip measure fall behind or beyond point measure then return null
                else if ((clipStartMeasure < pointMeasure && clipEndMeasure < pointMeasure) || (clipStartMeasure > pointMeasure && clipEndMeasure > pointMeasure))
                    return null;
                // else throw invalid LRS error.
                else
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRS);
            }

            // Get the measure progress of linear geometry and reassign the start and end measures based upon the progression
            var measureProgress = geometry.STLinearMeasureProgress();
            var geomStartMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetStartPointMeasure() : geometry.GetEndPointMeasure();
            var geomEndMeasure = measureProgress == LinearMeasureProgress.Increasing ? geometry.GetEndPointMeasure() : geometry.GetStartPointMeasure();

            // if clip start measure matches geom start measure and
            // clip end measure matches geom end measure then return the input geom
            if (clipStartMeasure.EqualsTo(geomStartMeasure) && clipEndMeasure.EqualsTo(geomEndMeasure))
                return geometry;

            // Check if clip start and end measures are beyond geom start and end point measures
            var isStartBeyond = clipStartMeasure < geomStartMeasure;
            var isEndBeyond = clipEndMeasure > geomEndMeasure;

            // When clip measure range is not beyond range; then don't consider tolerance on extreme math; as per Oracle
            var isExtremeMeasuresMatch = Ext.IsExtremeMeasuresMatch(geomStartMeasure, geomEndMeasure, clipStartMeasure, clipEndMeasure);

            // don't throw error when measure is not in the range
            // rather reassign segment start and end measure 
            // if they are beyond the range or matching with the start and end point measure of input geometry
            if (!clipStartMeasure.IsWithinRange(geometry))
            {
                if (isStartBeyond || isExtremeMeasuresMatch)
                {
                    if (clipStartMeasure <= geomStartMeasure)
                        clipStartMeasure = geomStartMeasure;
                }
                else
                    startMeasureInvalid = true;
            }

            // end point check
            if (!clipEndMeasure.IsWithinRange(geometry))
            {
                if (isEndBeyond || isExtremeMeasuresMatch)
                {
                    if (clipEndMeasure >= geomEndMeasure)
                        clipEndMeasure = geomEndMeasure;
                }
                else
                    endMeasureInvalid = true;
            }

            // if both clip start and end measure are reassigned to invalid then return null
            if (startMeasureInvalid || endMeasureInvalid)
                return null;

            // Post adjusting if clip start measure matches geom start measure and
            // clip end measure matches geom end measure then return the input geom
            if (clipStartMeasure.EqualsTo(geomStartMeasure) && clipEndMeasure.EqualsTo(geomEndMeasure))
                return geometry;

            // if clip start and end measure are equal post adjusting then we will return a shape point
            if (clipStartMeasure.EqualsTo(clipEndMeasure) && (isStartBeyond || isEndBeyond))
            {
                if (isStartBeyond)
                    return measureProgress == LinearMeasureProgress.Increasing ? geometry.STStartPoint() : geometry.STEndPoint();
                return measureProgress == LinearMeasureProgress.Increasing ? geometry.STEndPoint() : geometry.STStartPoint();
            }

            // if both clip start and end measure is same then don't check for distance tolerance
            if (clipStartMeasure.NotEqualsTo(clipEndMeasure))
            {
                var clipStartPoint = LocatePointWithTolerance(geometry, clipStartMeasure, out bool isClipStartShapePoint, tolerance);
                var clipEndPoint = LocatePointWithTolerance(geometry, clipEndMeasure, out bool isClipEndShapePoint, tolerance);
                if (clipStartPoint.IsWithinTolerance(clipEndPoint, tolerance))
                {
                    // if any one of them is a shape point return null
                    if (isClipStartShapePoint || isClipEndShapePoint)
                        return null;
                    else
                    {
                        var lrsLine = new LRSLine((int)clipStartPoint.STSrid);
                        // based on measure progress re-arrange the points.
                        if (measureProgress == LinearMeasureProgress.Increasing)
                        {
                            lrsLine.AddPoint(clipStartPoint);
                            lrsLine.AddPoint(clipEndPoint);
                        }
                        else
                        {
                            lrsLine.AddPoint(clipEndPoint);
                            lrsLine.AddPoint(clipStartPoint);
                        }
                        return lrsLine.ToSqlGeometry();
                    }
                }
            }

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ClipMGeometrySegmentSink(clipStartMeasure, clipEndMeasure, geometryBuilder, tolerance, retainClipMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// calculate measure value across shape points.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static SqlGeometry ConvertToLrsGeom(SqlGeometry geometry, double? startMeasure, double? endMeasure)
        {   //Line string geometry should not contain measure information.
            Ext.ThrowIfNotLRSType(geometry);
            if (geometry.HasZ || geometry.HasM)
                throw new ArgumentException(ErrorMessage.LineOrMultiLineStringCompatible);

            var NullConditonFirst = (startMeasure == null || endMeasure == null);
            var NullConditionSecond = (startMeasure == null && endMeasure == null);

            //if start measure or end measure is null then it returns null geometry
            if (NullConditonFirst &&!NullConditionSecond)
            {
                return null;
            }
             //point always takes start measure value if start and end measure values are not null
            if (geometry.IsPoint())
            {
                var pointMeasure = NullConditionSecond ? 0 : (double)startMeasure;
                return Ext.GetPointWithUpdatedM(geometry, pointMeasure);
            }

            // segment length
            var segmentLength = geometry.STLength().Value;

            // As per requirement; 
            // the default value of start point is 0 when null is specified
            // the default value of end point is cartographic length of the segment when null is specified
            var localStartMeasure = startMeasure ?? 0;
            var localEndMeasure = endMeasure ?? segmentLength;
            //internally ConvertToLrsGeom uses PopulateGeometry functionalities
            var geomSink = new PopulateGeometryMeasuresSink(localStartMeasure, localEndMeasure, segmentLength);
            geometry.Populate(geomSink);
            return geomSink.GetConstructedGeom();
        }

        /// <summary>
        /// Get end point measure of a LRS Geom Segment.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns>End measure</returns>
        public static SqlDouble GetEndMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            return geometry.GetEndPointMeasure();
        }

        /// <summary>
        /// Get start point measure of a LRS Geom Segment.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns>Start measure</returns>
        public static SqlDouble GetStartMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            return geometry.GetStartPointMeasure();
        }

        /// <summary>
        /// Find the point with specified measure, going from the start point in the direction of the end point.
        /// The measure must be between measures of these two points.
        /// </summary>
        /// <param name="startPoint">Start Geometry Point</param>
        /// <param name="endPoint">End Geometry Point</param>
        /// <param name="measure">Measure at which the point is to be found</param>
        /// <returns></returns>
        public static SqlGeometry InterpolateBetweenGeom(SqlGeometry startPoint, SqlGeometry endPoint, double measure)
        {
            // We need to check a few prerequisite.
            // We only operate on points.
            Ext.ThrowIfNotPoint(startPoint, endPoint);
            Ext.ThrowIfSRIDDoesNotMatch(startPoint, endPoint);
            Ext.ValidateLRSDimensions(ref startPoint);
            Ext.ValidateLRSDimensions(ref endPoint);
            Ext.ThrowIfMeasureIsNotInRange(measure, startPoint, endPoint);

            // The SRIDs also have to match
            var srid = startPoint.STSrid.Value;

            // Since we're working on a Cartesian plane, this is now pretty simple.
            // The fraction of the way from start to end.
            var fraction = (measure - startPoint.M.Value) / (endPoint.M.Value - startPoint.M.Value);
            var newX = (startPoint.STX.Value * (1 - fraction)) + (endPoint.STX.Value * fraction);
            var newY = (startPoint.STY.Value * (1 - fraction)) + (endPoint.STY.Value * fraction);

            //There's no way to know Z, so just put NULL there
            return Ext.GetPoint(newX, newY, null, measure, srid);
        }

        /// <summary>
        /// Method returns the Merge position of two LRS segments bound by the tolerance, if the segments are connected
        /// Otherwise return False in string format
        /// </summary>
        /// <param name="geometry1">Geometry Segment 1</param>
        /// <param name="geometry2">Geometry Segment 2</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns>Merge position if connected else false</returns>
        public static string GetMergePosition(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = Constants.Tolerance)
        {
            // if the segments are connected, return the merge position
            var isConnected = CheckIfConnected(geometry1, geometry2, tolerance, out var mergePosition);
            // for not connected segments, return false in string type
            return isConnected ? mergePosition.ToString() : "false";
        }

        /// <summary>
        /// Checks if two geometric segments are spatially connected.
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerance"></param>
        /// <returns>SqlBoolean</returns>
        public static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = Constants.Tolerance)
        {
            return CheckIfConnected(geometry1, geometry2, tolerance, out _);
        }

        /// <summary>
        /// Checks if two geometric segments are spatially connected with merge position information
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <param name="tolerance">Distance Threshold range; default 0.01F</param>
        /// <param name="mergePosition"></param>
        /// <returns>SqlBoolean</returns>
        private static SqlBoolean CheckIfConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance, out MergePosition mergePosition)
        {
            Ext.ThrowIfNotLRSType(geometry1, geometry2);
            Ext.ThrowIfSRIDDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            // check for point type
            if (geometry1.IsPoint() && geometry2.IsPoint())
            {
                var result = geometry1.IsXYWithinRange(geometry2, tolerance);
                mergePosition = result ? MergePosition.BothEnds : MergePosition.None;
                return result;
            }

            // Geometry 1 points
            var geometry1StartPoint = geometry1.STStartPoint();
            var geometry1EndPoint = geometry1.STEndPoint();

            // Geometry 2 points
            var geometry2StartPoint = geometry2.STStartPoint();
            var geometry2EndPoint = geometry2.STEndPoint();

            // If the points doesn't coincide, check for the point co-ordinate difference and whether it falls within the tolerance
            // distance not considered as per Oracle.
            // Comparing geom1 start point x and y co-ordinate difference against geom2 start and end point x and y co-ordinates
            var isStartStartConnected = geometry1StartPoint.STEquals(geometry2StartPoint) || geometry1StartPoint.IsXYWithinRange(geometry2StartPoint, tolerance);
            var isStartEndConnected = geometry1StartPoint.STEquals(geometry2EndPoint) || geometry1StartPoint.IsXYWithinRange(geometry2EndPoint, tolerance);
            var isEndStartConnected = geometry1EndPoint.STEquals(geometry2StartPoint) || geometry1EndPoint.IsXYWithinRange(geometry2StartPoint, tolerance);
            var isEndEndConnected = geometry1EndPoint.STEquals(geometry2EndPoint) || geometry1EndPoint.IsXYWithinRange(geometry2EndPoint, tolerance);
            var isBothEndsConnected = isStartStartConnected && isEndEndConnected;
            var isCrossEndsConnected = isStartEndConnected && isEndStartConnected;

            mergePosition = MergePosition.None;

            if (isStartStartConnected)
                mergePosition = MergePosition.StartStart;
            if (isStartEndConnected)
                mergePosition = MergePosition.StartEnd;

            if (isEndStartConnected)
                mergePosition = MergePosition.EndStart;
            if (isEndEndConnected)
                mergePosition = MergePosition.EndEnd;

            if (isBothEndsConnected)
                mergePosition = MergePosition.BothEnds;
            if (isCrossEndsConnected)
                mergePosition = MergePosition.CrossEnds;

            if (isStartStartConnected || isStartEndConnected || isEndStartConnected || isEndEndConnected)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if an LRS point is valid.
        /// </summary>
        /// <param name="geometry">Sql Geometry.</param>
        /// <returns></returns>
        public static SqlBoolean IsValidPoint(SqlGeometry geometry)
        {
            if (geometry.IsNullOrEmpty() || !geometry.STIsValid() || !geometry.IsPoint())
                return false;

            // check if the point has measure value
            if (!geometry.M.IsNull)
                return true;

            // if m is null; the check if frame from x,y,z where z is m
            if (geometry.STGetDimension() != DimensionalInfo.Dim3D) return false;
            geometry = geometry.ConvertTo2DimensionWithMeasure();
            return !geometry.M.IsNull;
        }

        /// <summary>
        /// Locate the Geometry Point along the specified measure on the Geometry.
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="measure">Measure of the Geometry point to locate</param>
        /// <returns>Geometry Point</returns>
        public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double measure)
        {
            // Invoking locate point without tolerance
            return LocatePointWithTolerance(geometry, measure, out _, 0);
        }

        /// <summary>
        /// Locates the point with tolerance.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="measure">The measure.</param>
        /// <param name="isShapePoint"></param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        private static SqlGeometry LocatePointWithTolerance(SqlGeometry geometry, double measure, out bool isShapePoint, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            Ext.ThrowIfMeasureIsNotInRange(measure, geometry);

            // If input geom is point; its a no-op just return the same.
            if (geometry.IsPoint())
            {
                isShapePoint = true;
                return geometry;
            }

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new LocateMAlongGeometrySink(measure, geomBuilder, tolerance);
            geometry.Populate(geomSink);

            // if point is not derived then the measure is not in range.
            if (!geomSink.IsPointDerived)
                Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);

            isShapePoint = geomSink.IsShapePoint;
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Merge two geometry segments to one geometry.
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry1">First Geometry</param>
        /// <param name="geometry2">Second Geometry</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Returns Merged Geometry Segments</returns>
        public static SqlGeometry MergeGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = Constants.Tolerance)
        {
            Ext.ThrowIfNotLRSType(geometry1, geometry2);
            Ext.ThrowIfSRIDDoesNotMatch(geometry1, geometry2);
            Ext.ValidateLRSDimensions(ref geometry1);
            Ext.ValidateLRSDimensions(ref geometry2);

            // return object 
            SqlGeometry returnGeom = null;

            // returning geometry2 if both the geometries are points
            if (geometry1.CheckGeomPoint() && geometry2.CheckGeomPoint())
                return geometry2;

            // If either of the input geom is point; then return the other geometry.
            if (geometry1.CheckGeomPoint())
                return geometry2;

            if (geometry2.CheckGeomPoint())
                return geometry1;

            var isConnected = CheckIfConnected(geometry1, geometry2, tolerance, out var mergePosition);
            var mergeType = geometry1.GetMergeType(geometry2);

            if (isConnected)
            {
                switch (mergeType)
                {
                    case MergeInputType.LSLS:
                        returnGeom = MergeConnectedLineStrings(geometry1, geometry2, mergePosition, out _);
                        break;
                    case MergeInputType.LSMLS:
                    case MergeInputType.MLSLS:
                    case MergeInputType.MLSMLS:
                        returnGeom = MergeConnectedMultiLineStrings(geometry1, geometry2, mergePosition);
                        break;
                }
            }
            else
            {
                // construct multi line
                returnGeom = MergeDisconnectedLineSegments(geometry1, geometry2);
            }
            return returnGeom;
        }

        /// <summary>
        /// Merge the segments bound with tolerance and resets the measure from zero
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="tolerance"></param>
        /// <returns>SqlGeometry</returns>
        public static SqlGeometry MergeAndResetGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = Constants.Tolerance)
        {
            var resultantGeometry = MergeGeometrySegments(geometry1, geometry2, tolerance);
            return PopulateGeometryMeasures(resultantGeometry, null, null);
        }

        /// <summary>
        /// Method will merge simple line strings with tolerance and returns the merged line segment by considering measure and direction of the first geometry.
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="mergePosition"></param>
        /// <param name="measureDifference"></param>
        /// <returns>SqlGeometry</returns>
        private static SqlGeometry MergeConnectedLineStrings(SqlGeometry geometry1, SqlGeometry geometry2, MergePosition mergePosition, out double measureDifference)
        {
            // geometry 1 and geometry 2 to be 2D line strings with measure 'm'
            Ext.ThrowIfNotLine(geometry1, geometry2);
            // offset measure difference.
            double offsetM;

            // references governs the order of geometries to get merge
            SqlGeometry targetSegment, sourceSegment;

            // check direction of measure.
            var isSameDirection = geometry1.STSameDirection(geometry2);

            // segments must be connected in any of the following position.
            switch (mergePosition)
            {
                case MergePosition.EndStart:
                case MergePosition.CrossEnds:
                    {
                        // Single negation of measure is needed for geometry 2,
                        // if both segments are differ in measure variation
                        if (!isSameDirection)
                            geometry2 = MultiplyGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STEndPoint().GetPointOffset(geometry2.STStartPoint());
                        geometry2 = TranslateMeasure(geometry2, offsetM);
                        sourceSegment = geometry1;
                        targetSegment = geometry2;
                        break;
                    }
                case MergePosition.EndEnd:
                case MergePosition.BothEnds:
                    {
                        // Double negation is needed for geometry 2, i.e., both segments differ from measure variation,
                        // also, geometry 2 has been traversed from ending point to the starting point
                        if (isSameDirection)
                            geometry2 = MultiplyGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STEndPoint().GetPointOffset(geometry2.STEndPoint());
                        // Reverse the geometry 2, since it has been traversed from ending point to the starting point
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = ReverseAndTranslateGeometry(geometry2, offsetM);
                        // start traversing from the geometry 1, hence g1 would be the source geometry
                        sourceSegment = geometry1;
                        targetSegment = geometry2;
                        break;
                    }
                case MergePosition.StartStart:
                    {
                        // Double negation is needed for geometry 2, i.e., both segments differ from measure variation,
                        // also, geometry 2 has been traversed from ending point to the starting point
                        if (isSameDirection)
                            geometry2 = MultiplyGeometryMeasures(geometry2, -1);

                        offsetM = geometry1.STStartPoint().GetPointOffset(geometry2.STStartPoint());
                        // Reverse the geometry 2, since it has been traversed from ending point to the starting point
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = ReverseAndTranslateGeometry(geometry2, offsetM);
                        // the starting point of g1 will become the intermediate point of resultant, so source geometry would be geometry 2
                        sourceSegment = geometry2;
                        targetSegment = geometry1;
                        break;
                    }
                case MergePosition.StartEnd:
                    {
                        // Single negation of measure is needed for geometry 2
                        // if both segments are differ in measure variation
                        if (!isSameDirection)
                            geometry2 = MultiplyGeometryMeasures(geometry2, -1);

                        offsetM = (geometry1.STStartPoint().M.Value - geometry2.STEndPoint().M.Value);
                        // scale the measures of geometry 2 based on the offset measure difference between them
                        geometry2 = TranslateMeasure(geometry2, offsetM);
                        // the starting point of g1 will become the intermediate point of resultant, so source geometry would be geometry 2
                        sourceSegment = geometry2;
                        targetSegment = geometry1;
                        break;
                    }
                default:
                    throw new Exception("Invalid Merge Coordinate position");
            }
            measureDifference = offsetM;    // gives the offset measure difference for the caller method consumption
            // Builder for resultant merged geometry to store
            var geomBuilder = new SqlGeometryBuilder();

            // Building a line segment from the range of points by excluding the last point ( Merging point )
            var segment1 = new LineStringMergeGeometrySink(geomBuilder, true, sourceSegment.STNumPoints());
            sourceSegment.Populate(segment1);

            // Continuing to build segment from the points of second geometry
            var segment2 = new LineStringMergeGeometrySink(geomBuilder, false, targetSegment.STNumPoints());
            targetSegment.Populate(segment2);

            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Merges the Multi line and line string combinations with connected structure
        /// </summary>
        /// <param name="geometry1"></param>
        /// <param name="geometry2"></param>
        /// <param name="mergePosition"></param>
        /// <returns>SqlGeometry of type MultiLineString</returns>
        private static SqlGeometry MergeConnectedMultiLineStrings(SqlGeometry geometry1, SqlGeometry geometry2, MergePosition mergePosition)
        {
            // check direction of measure.
            var isSameDirection = geometry1.STSameDirection(geometry2);

            SqlGeometry sourceSegment, targetSegment, mergedSegment;

            var segment1 = geometry1.GetLRSMultiLine();
            var segment2 = geometry2.GetLRSMultiLine();

            switch (mergePosition)
            {
                case MergePosition.EndEnd:
                case MergePosition.BothEnds:
                    {
                        //  Double Negation of measure is needed, since geometry2 has been traversed from end point to the starting point also differ from measure variation
                        if (isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetLastLine().ToSqlGeometry();
                        targetSegment = segment2.GetLastLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, mergePosition, out var measureDifference);

                        var mergedGeom = mergedSegment.GetLRSMultiLine();

                        segment1.RemoveLast();                          //  Removing merging line segment from the geometry1
                        segment2.RemoveLast();                          //  Removing merging line segment from the geometry2

                        segment2.ReverseLinesAndPoints();               //  Traversing from end to the start of geometry2. So reversing the 
                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment1.Add(mergedGeom);                       //  appending merged segment line to the segment1 , since geometry1 would be the beginning geometry of the resultant geometry
                        segment1.Add(segment2);                         //  appending remaining segment of updated geometry2 with the segment1
                        return segment1.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.EndStart:
                case MergePosition.CrossEnds:
                    {
                        //  Negation of measure is needed, since measure variation of geometry2 differs from that of geometry1
                        if (!isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetLastLine().ToSqlGeometry();
                        targetSegment = segment2.GetFirstLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, mergePosition, out var measureDifference);

                        var mergedGeom = mergedSegment.GetLRSMultiLine();

                        segment1.RemoveLast();                          //  Removing merging line segment from the geometry1
                        segment2.RemoveFirst();                         //  Removing merging line segment from the geometry2

                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment1.Add(mergedGeom);                       //  Appending merged segment line to the segment1 , since geometry1 would be the beginning geometry of the resultant geometry
                        segment1.Add(segment2);                         //  Appending remaining segment of updated geometry2 with the segment1
                        return segment1.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.StartEnd:
                    {
                        //  Negation of measure is needed, since measure variation of geometry2 differs from that of geometry1
                        if (!isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetFirstLine().ToSqlGeometry();
                        targetSegment = segment2.GetLastLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, mergePosition, out var measureDifference);

                        var mergedGeom = mergedSegment.GetLRSMultiLine();

                        segment1.RemoveFirst();                         //  Removing merging line segment from the geometry1
                        segment2.RemoveLast();                          //  Removing merging line segment from the geometry2

                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment2.Add(mergedGeom);                       //  Appending merged segment line to the segment2 ,since geometry1 would be the beginning geometry of the resultant geometry
                        segment2.Add(segment1);                         //  Appending remaining segments of geometry1 with the geometry2
                        return segment2.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                case MergePosition.StartStart:
                    {
                        //  Double Negation of measure is needed, since geometry2 has been traversed from end point to the starting point also differ from measure variation
                        if (isSameDirection)
                            segment2.ScaleMeasure(-1);

                        sourceSegment = segment1.GetFirstLine().ToSqlGeometry();
                        targetSegment = segment2.GetFirstLine().ToSqlGeometry();
                        //  Generating merged segment of geometry1 and geometry2
                        mergedSegment = MergeConnectedLineStrings(sourceSegment, targetSegment, mergePosition, out var measureDifference);

                        var mergedGeom = mergedSegment.GetLRSMultiLine();

                        segment1.RemoveFirst();                         //  Removing merging line segment from the geometry1
                        segment2.RemoveFirst();                         //  Removing merging line segment from the geometry2

                        segment2.ReverseLinesAndPoints();               //  Reversing the lines and its corresponding points of segment2, Since it has been traversed from end to start
                        segment2.TranslateMeasure(measureDifference);   //  Translating the offset measure difference in segment2

                        segment2.Add(mergedGeom);                       //  Appending merged segment line to the segment2 ,since geometry1 would be the beginning geometry of the resultant geometry
                        segment2.Add(segment1);                         //  Appending remaining segments of geometry1 with the geometry2
                        return segment2.ToSqlGeometry();                //  converting to SqlGeometry type
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Build MULTILINESTRING from two input geometry segments [LINESTRING, MULTILINESTRING]
        /// This should be called for merging geom segments when they are not connected.
        /// Here the offset measure is updated with the measure of second segment.
        /// </summary>
        /// <param name="geometry1">The geometry1.</param>
        /// <param name="geometry2">The geometry2.</param>
        /// <returns></returns>
        private static SqlGeometry MergeDisconnectedLineSegments(SqlGeometry geometry1, SqlGeometry geometry2)
        {
            var isSameDirection = geometry1.STSameDirection(geometry2);
            var firstSegmentDirection = geometry1.STLinearMeasureProgress();
            if (!isSameDirection)
                geometry2 = MultiplyGeometryMeasures(geometry2, -1);

            var offsetM = geometry1.GetOffset(geometry2);
            var doUpdateM = false;

            if (isSameDirection)
            {
                if (firstSegmentDirection == LinearMeasureProgress.Increasing && offsetM > 0)
                    doUpdateM = true;

                if (firstSegmentDirection == LinearMeasureProgress.Decreasing && offsetM < 0)
                    doUpdateM = true;
            }
            else
                doUpdateM = true;

            var mergedLRSMultiLine = geometry1.GetLRSMultiLine();
            mergedLRSMultiLine.Add(geometry2.GetLRSMultiLine(doUpdateM, offsetM));

            return mergedLRSMultiLine.ToSqlGeometry();
        }

        /// <summary>
        /// Multiply the measure values of Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="multiplyMeasure">Measure to be Multiplied</param>
        /// <returns></returns>
        public static SqlGeometry MultiplyGeometryMeasures(SqlGeometry geometry, double multiplyMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if scale measure is zero; return the input
            if (multiplyMeasure.EqualsTo(0))
                return geometry;

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new MultiplyMeasureGeometrySink(geometryBuilder, multiplyMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Returns the geometric segment at a specified offset from a geometric segment.
        /// Works only for LineString and MultiLineString Geometry; Point is not supported.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="offset">Offset value</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Offset Geometry Segment</returns>
        public static SqlGeometry OffsetGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double offset, double tolerance = Constants.Tolerance)
        {
            // If point throw invalid LRS Segment error.
            if (geometry.IsPoint())
                Ext.ThrowLRSError(LRSErrorCodes.InvalidLRS);

            Ext.ThrowIfNotLineOrMultiLine(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // to retain clip measures on offset
            var clippedGeometry = ClipAndRetainMeasure(geometry, startMeasure, endMeasure, tolerance, true);

            // if clipped segment is null; then return null.
            if (clippedGeometry == null)
                return null;

            // Explicit handle if clipped segment is Point
            // As point has a single co-ordinate we need to consider the angle from input segment, not from the clipped segment
            var lrsSegment = clippedGeometry.IsPoint() ? geometry.GetLRSMultiLine() : clippedGeometry.GetLRSMultiLine();

            if (clippedGeometry.IsPoint())
            {
                // Computing offset
                var parallelSegment = lrsSegment.ComputeOffset(offset, tolerance);

                // get the offset point at clipped measure
                var offsetPoint = parallelSegment.GetPointAtM(clippedGeometry.M.Value);
                return offsetPoint?.ToSqlGeometry();
            }
            else
            {
                // removing collinear points
                lrsSegment.RemoveCollinearPoints();

                // Computing offset
                var parallelSegment = lrsSegment.ComputeOffset(offset, tolerance);

                // if it is a two point line string; then check for distance
                if (parallelSegment.Is2PointLine)
                {
                    var firstPoint = parallelSegment.GetFirstLine().GetStartPoint();
                    var secondPoint = parallelSegment.GetFirstLine().GetEndPoint();

                    if (firstPoint.IsXYWithinTolerance(secondPoint, tolerance))
                    {
                        // always the resultant is first point from the parallel segment
                        // and measure being updated with minimum of start and end measure;
                        firstPoint.M = Math.Min(startMeasure, endMeasure);
                        return firstPoint.ToSqlGeometry();
                    }
                }
                parallelSegment.PopulateMeasures(parallelSegment.GetStartPointM(), parallelSegment.GetEndPointM());

                return parallelSegment.ToSqlGeometry();
            }
        }

        /// <summary>
        /// (Re)populate measures across shape points.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <returns></returns>
        public static SqlGeometry PopulateGeometryMeasures(SqlGeometry geometry, double? startMeasure, double? endMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // check for point type
            if (geometry.CheckGeomPoint())
            {
                // if anyone measure is null; then point measure is 0 else point measure is end measure
                var pointMeasure = startMeasure == null || endMeasure == null ? 0 : (double)endMeasure;
                return Ext.GetPointWithUpdatedM(geometry, pointMeasure);
            }

            // if anyone is null then assign null to other
            if (startMeasure == null || endMeasure == null)
                startMeasure = endMeasure = null;

            // segment length
            var segmentLength = geometry.STLength().Value;

            // As per requirement; 
            // the default value of start point is 0 when null is specified
            // the default value of end point is cartographic length of the segment when null is specified
            var localStartMeasure = startMeasure ?? 0;
            var localEndMeasure = endMeasure ?? segmentLength;

            var geomSink = new PopulateGeometryMeasuresSink(localStartMeasure, localEndMeasure, segmentLength);
            geometry.Populate(geomSink);
            return geomSink.GetConstructedGeom();
        }
        /// <summary>
        /// Resets Geometry Measure values.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ResetMeasure(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new ResetMGeometrySink(geomBuilder);
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Reverse Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <returns></returns>
        public static SqlGeometry ReverseLinearGeometry(SqlGeometry geometry)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if point its no-op
            if (geometry.IsPoint())
                return geometry;

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ReverseLinearGeometrySink(geometryBuilder);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Reverse and translate Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="translateMeasure"></param>
        /// <returns></returns>
        public static SqlGeometry ReverseAndTranslateGeometry(SqlGeometry geometry, double translateMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // if point then return new point with updated measure
            if (geometry.IsPoint())
                return Ext.GetPointWithUpdatedM(geometry, geometry.M.Value + translateMeasure);

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ReverseAndTranslateGeometrySink(geometryBuilder, translateMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Scale the measure values of the Linear Geometry
        /// Works only for POINT, LINESTRING, MULTILINESTRING Geometry.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="startMeasure">Start Measure</param>
        /// <param name="endMeasure">End Measure</param>
        /// <param name="shiftMeasure">Measure to be Multiplied</param>
        /// <returns></returns>
        public static SqlGeometry ScaleGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double shiftMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);
            Ext.ThrowIfMeasureNotLinear(geometry);

            var geometryBuilder = new SqlGeometryBuilder();
            var geomSink = new ScaleGeometrySink(geometryBuilder, geometry.GetStartPointMeasure(), geometry.GetEndPointMeasure(), startMeasure, endMeasure, shiftMeasure);
            geometry.Populate(geomSink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Split a geometry into geometry segments based on split measure. 
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="splitMeasure"></param>
        /// <param name="geometry1">First Geometry Segment</param>
        /// <param name="geometry2">Second Geometry Segment</param>
        public static void SplitGeometrySegment(SqlGeometry geometry, double splitMeasure, out SqlGeometry geometry1, out SqlGeometry geometry2)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            // assign default null values to out param.
            geometry1 = null;
            geometry2 = null;

            // for point validation.
            if (geometry.CheckGeomPoint())
            {
                var pointMeasure = geometry.HasM ? geometry.M.Value : 0;
                if (pointMeasure.NotEqualsTo(splitMeasure))
                    Ext.ThrowLRSError(LRSErrorCodes.InvalidLRSMeasure);
                return;
            }

            var startPointM = geometry.GetStartPointMeasure();
            var endPointM = geometry.GetEndPointMeasure();
            var ifNotLinear = !geometry.STHasLinearMeasure();

            // if start point and end point is equal and it is equal to split measure then return null
            if (geometry.STHasEqualStartAndEndMeasure() && startPointM.EqualsTo(splitMeasure))
                return;

            // if start measure is split measure then segment 1 is null and segment 2 is input geom
            if (startPointM.EqualsTo(splitMeasure))
            {
                geometry2 = geometry;
                return;
            }

            // if end measure is split measure then segment 2 is null and segment 1 is input geom
            if (endPointM.EqualsTo(splitMeasure))
            {
                geometry1 = geometry;
                return;
            }

            // if geom is multiline and the segment doesn't has linear measure;
            // reassign the split measure to start or end measure as per progress
            if (geometry.IsMultiLineString() && ifNotLinear)
            {
                var maxMeasure = startPointM > endPointM ? startPointM : endPointM;
                if (splitMeasure > maxMeasure)
                    splitMeasure = maxMeasure;
            }

            // measure range validation is handled inside LocatePoint
            var splitPoint = LocatePointAlongGeom(geometry, splitMeasure);

            var geomSink = new SplitGeometrySegmentSink(splitPoint);
            geometry.Populate(geomSink);
            geometry1 = geomSink.Segment1;
            geometry2 = geomSink.Segment2;
        }

        /// <summary>
        /// Translates the measure values of Input Geometry
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="translateMeasure">The translate measure.</param>
        /// <returns>SqlGeometry with translated measure.</returns>
        public static SqlGeometry TranslateMeasure(SqlGeometry geometry, double translateMeasure)
        {
            Ext.ThrowIfNotLRSType(geometry);
            Ext.ValidateLRSDimensions(ref geometry);

            var geomBuilder = new SqlGeometryBuilder();
            var geomSink = new TranslateMeasureGeometrySink(geomBuilder, translateMeasure);
            geometry.Populate(geomSink);
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Validates the LRS geometry.
        /// </summary>
        /// <param name="geometry">The input SqlGeometry.</param>
        /// <returns>TRUE if Valid; 13331 - if Invalid; 13333 - if Invalid Measure</returns>
        public static string ValidateLRSGeometry(SqlGeometry geometry)
        {
            // throw if type apart from POINT, LINESTRING, MULTILINESTRING is given as input.
            Ext.ThrowIfNotLRSType(geometry);

            // check for dimension
            if (geometry.STGetDimension() == DimensionalInfo.Dim2D)
            {
                // If there is no measure value; return invalid.
                return LRSErrorCodes.InvalidLRS.Value();
            }

            // convert to valid 3 point LRS co-ordinate.
            Ext.ValidateLRSDimensions(ref geometry);

            // return invalid if empty or is of geometry collection
            if (geometry.IsNullOrEmpty() || !geometry.STIsValid() || geometry.IsGeometryCollection())
                return LRSErrorCodes.InvalidLRS.Value();

            // return invalid if geometry doesn't or have null values or checks if the measures are in linear range.
            return geometry.STHasLinearMeasure() ? LRSErrorCodes.ValidLRS.Value() : LRSErrorCodes.InvalidLRSMeasure.Value();
        }
    }
}
