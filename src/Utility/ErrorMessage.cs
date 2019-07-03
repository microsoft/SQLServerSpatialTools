//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.Utility
{
    public static class ErrorMessage
    {
        public const string LineStringCompatible = "LINESTRING is currently the only spatial type supported.";
        public const string LRSCompatible = "POINT, LINESTRING or MULTILINE STRING is currently the only spatial type supported.";
        public const string LineOrMultiLineStringCompatible = "LINESTRING or MULTILINE STRING is currently the only spatial type supported.";
        public const string LineOrPointCompatible = "LINESTRING or POINT is currently the only spatial type supported.";
        public const string MultiLineStringCompatible = "MULTILINE STRING is currently the only spatial type supported.";
        public const string PointCompatible = "Start and End geometry must be a point.";
        public const string SRIDCompatible = @"SRID's of geography\geometry objects doesn't match.";
        public const string MeasureRange = "Measure not within range.";
        public const string LinearMeasureRange = "The given measure for linear referencing was out of range.";
        public const string WKT3DOnly = "Input WKT should only have three dimensions!";
        public const string LinearGeometryMeasureMustBeInRange = "{0} is not within the measure range {1} : {2} of the linear geometry."; 
        public const string DistanceMustBeBetweenTwoPoints = "The distance value provided exceeds the distance between the two points.";
        public const string DistanceMustBePositive = "The distance must be positive.";
        public const string TwoDimensionalCoordinates = "Cannot operate on 2 Dimensional co-ordinates without measure values.";
        public const string InvalidElementIndex = "Invalid index for element to be extracted.";
        public const string InvalidSubElementIndex = "Invalid index for sub-element to be extracted.";
        public const string InvalidGeometry = "Invalid geometry";
    }
}
