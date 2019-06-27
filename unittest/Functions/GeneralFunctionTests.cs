//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.General;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Functions
{
    public static class GeneralFunctionTests
    {
        [TestClass]
        public class GeometryTests : BaseUnitTest
        {
            [TestMethod]
            public void FilterArtifactsGeometryTest()
            {
                var geom = "GEOMETRYCOLLECTION(LINESTRING EMPTY, LINESTRING (1 1, 3 5), POINT (1 1), POLYGON ((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1)))".GetGeom();

                // Empty line and point should be removed
                // short line should be removed - tolerance length
                var shortLineTolerance = 5;
                // Polygon inner ring with area < tolerance * polygon length
                var polygonAreaTolerance = 1.5;

                Logger.LogLine("Input Geometry: {0}", geom);
                Logger.Log("Filtering input geometry; removing empty line string");
                Logger.Log("points, short line of tolerance: {0}, Polygon with inner ring area tolerance: {1}", shortLineTolerance, polygonAreaTolerance);
                var expectedGeom = "GEOMETRYCOLLECTION EMPTY".GetGeom();
                var filteredGeom = Geometry.FilterArtifactsGeometry(geom, true, true, shortLineTolerance, polygonAreaTolerance);
                Logger.Log("Expected converted geom: {0}", expectedGeom);
                Logger.Log("Obtained converted geom: {0}", filteredGeom);
                SqlAssert.IsTrue(filteredGeom.STEquals(expectedGeom));
            }

            [TestMethod]
            public void GeomFromXYMTextTest()
            {
                var geomWKT = "LINESTRING (0 0 3 4, 10 0 3 4)";
                Logger.LogLine("Converting input Geom with 3 dimension and measure : {0}", geomWKT);
                try
                {
                    Geometry.GeomFromXYMText(geomWKT, Constants.DefaultSRID);
                }
                catch (ArgumentException e)
                {
                    Assert.AreEqual(e.Message, ErrorMessage.WKT3DOnly);
                    TestContext.WriteLine(ErrorMessage.WKT3DOnly);
                }

                geomWKT = "LINESTRING (0 0 3, 10 0 4)";
                Logger.LogLine("Converting input Geom with 3 dimension and measure : {0}", geomWKT);
                var expectedGeom = "LINESTRING(0 0 NULL 3, 10 0 NULL 4)".GetGeom();
                var convertedGeom = Geometry.GeomFromXYMText(geomWKT, Constants.DefaultSRID);
                Logger.Log("Expected converted geom: {0}", expectedGeom);
                Logger.Log("Obtained converted geom: {0}", convertedGeom);
                SqlAssert.IsTrue(convertedGeom.STEquals(expectedGeom));
            }

            [TestMethod]
            public void InterpolateBetweenGeomTest()
            {
                var geom1 = "POINT(0 0 0 0)".GetGeom();
                var geom2 = "POINT(10 0 0 10)".GetGeom();
                var returnPoint = "POINT (5 0 NULL 5)".GetGeom();
                const int distance = 5;
                Logger.LogLine("Input Point 1:{0} Point 2:{1}", geom1, geom2);
                Logger.Log("Interpolating at a distance of {0}", geom1, geom2, distance);
                Logger.LogLine("Expected Point: {0}", returnPoint);
                var sqlGeometry = Geometry.InterpolateBetweenGeom(geom1, geom2, distance);
                Logger.Log("Obtained Point: {0}", sqlGeometry.ToString());
                SqlAssert.IsTrue(sqlGeometry.STEquals(returnPoint));

                try
                {
                    geom1 = "LINESTRING(0 0 0 0, 1 1 1 1)".GetGeom();
                    Geometry.InterpolateBetweenGeom(geom1, geom2, distance);
                }
                catch (ArgumentException)
                {
                }

                try
                {
                    geom1 = "POINT(0 0 0 0)".GetGeom();
                    geom2 = "POINT(0 0 0 0)".GetGeom(0);
                    Geometry.InterpolateBetweenGeom(geom1, geom2, distance);
                }
                catch (ArgumentException)
                {
                }

                try
                {
                    geom1 = "POINT(0 0 0 0)".GetGeom();
                    geom2 = "POINT(0 0 0 1)".GetGeom();
                    Geometry.InterpolateBetweenGeom(geom1, geom2, 10);
                }
                catch (ArgumentException)
                {
                }

                try
                {
                    geom1 = "POINT(0 0 0 0)".GetGeom();
                    geom2 = "POINT(0 0 0 1)".GetGeom();
                    Geometry.InterpolateBetweenGeom(geom1, geom2, -5);
                }
                catch (ArgumentException)
                {
                }
            }

            [TestMethod]
            public void LocatePointAlongGeomTest()
            {
                var geom = "LINESTRING (0 0, 10 0)".GetGeom();
                Logger.Log("Input Geom : {0}", geom.ToString());
                var returnPoint = "POINT (5 0)".GetGeom();
                var distance = 5;

                Logger.LogLine("Locating a point at distance of {0} Measure", distance);
                var locatedPoint = Geometry.LocatePointAlongGeom(geom, distance);
                Logger.Log("Expected point: {0}", returnPoint);
                Logger.Log("Located  point: {0} at distance of {1} Measure", locatedPoint, distance);
                SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));

                geom = "LINESTRING (0 0 0 5, 10 0 0 10)".GetGeom();
                returnPoint = "POINT (0 0 0 5)".GetGeom();
                locatedPoint = Geometry.LocatePointAlongGeom(geom, 0);
                SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));

                try
                {
                    Geometry.LocatePointAlongGeom(geom, 15);
                }
                catch (ArgumentException) { }

                try
                {
                    geom = "POINT (0 0 0 0)".GetGeom();
                    Geometry.LocatePointAlongGeom(geom, 15);
                }
                catch (ArgumentException) { }
            }

            [TestMethod]
            public void MakeValidForGeographyTest()
            {
                var geometry = "CURVEPOLYGON (CIRCULARSTRING (0 -4, 4 0, 0 4, -4 0, 0 -4))".GetGeom();
                var retGeom = Geometry.MakeValidForGeography(geometry);
                Logger.LogLine("Executing Make Valid: {0}", geometry);
                SqlAssert.IsTrue(retGeom.STEquals(retGeom));

                geometry = "LINESTRING(0 2, 1 1, 1 0, 1 1, 2 2)".GetGeom();
                Logger.LogLine("Executing Make Valid: {0}", geometry);
                var expectedGeom = "MULTILINESTRING ((7.1054273576010019E-15 2, 1 1, 2 2), (1 1, 1 7.1054273576010019E-15))".GetGeom();
                retGeom = Geometry.MakeValidForGeography(geometry);
                Logger.Log("Expected Geom: {0}", expectedGeom);
                Logger.Log("Obtained Geom: {0}", retGeom);
                SqlAssert.IsTrue(retGeom.STEquals(expectedGeom));
            }

            [TestMethod]
            public void ReverseLinestringTest()
            {
                var geom = "LINESTRING (1 1, 5 5)".GetGeom();
                Logger.Log("Input Geom : {0}", geom.ToString());

                var endPoint = "POINT (5 5 0 0)".GetGeom();
                var reversedLineSegment = Geometry.ReverseLinestring(geom);
                Logger.Log("Reversed Line string : {0}", reversedLineSegment.ToString());
                SqlAssert.IsTrue(reversedLineSegment.STStartPoint().STEquals(endPoint));

                try
                {
                    geom = "POINT (1 1)".GetGeom();
                    Geometry.ReverseLinestring(geom);
                }
                catch (ArgumentException)
                {
                }
            }

            [TestMethod]
            public void ShiftGeometryTest()
            {
                // Point
                var geom = "POINT(0 1)".GetGeom();
                var shiftPoint = "POINT (4 5)".GetGeom();
                double xShift = 4, yShift = 4;
                Logger.LogLine("Input Point: {0}", geom);
                Logger.Log("Expected Point: {0}", shiftPoint);
                var shiftedGeom = Geometry.ShiftGeometry(geom, xShift, yShift);
                Logger.Log("Obtained Point: {0}", shiftedGeom);
                SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

                // Simple Line String
                geom = "LINESTRING (1 1, 4 4)".GetGeom();
                shiftPoint = "LINESTRING (11 11, 14 14)".GetGeom();
                xShift = 10;
                yShift = 10;
                Logger.LogLine("Input Geom: {0}", geom);
                Logger.Log("Expected Geom: {0}", shiftPoint);
                shiftedGeom = Geometry.ShiftGeometry(geom, xShift, yShift);
                Logger.Log("Obtained Point: {0}", shiftedGeom);
                SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

                // Line String with multiple points
                geom = "LINESTRING (1 1, 2 3, -1 -3, 4 -3, -2 1)".GetGeom();
                shiftPoint = "LINESTRING (11 11, 12 13, 9 7, 14 7, 8 11)".GetGeom();
                Logger.LogLine("Input Geom: {0}", geom);
                Logger.Log("Expected Geom: {0}", shiftPoint);
                shiftedGeom = Geometry.ShiftGeometry(geom, xShift, yShift);
                Logger.Log("Obtained Point: {0}", shiftedGeom);
                SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

                // Multi Line String
                geom = "MULTILINESTRING ((1 1, 2 3), (-1 -3, 4 -3, -2 1))".GetGeom();
                shiftPoint = "MULTILINESTRING ((11 11, 12 13), (9 7, 14 7, 8 11))".GetGeom();
                Logger.LogLine("Input Geom: {0}", geom);
                Logger.Log("Expected Geom: {0}", shiftPoint);
                shiftedGeom = Geometry.ShiftGeometry(geom, xShift, yShift);
                Logger.Log("Obtained Point: {0}", shiftedGeom);
                SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));

                // Polygon
                geom = "POLYGON((1 1, 3 3, 3 1, 1 1))".GetGeom();
                shiftPoint = "POLYGON ((11 11, 13 13, 13 11, 11 11))".GetGeom();
                Logger.LogLine("Input Geom: {0}", geom);
                Logger.Log("Expected Geom: {0}", shiftPoint);
                shiftedGeom = Geometry.ShiftGeometry(geom, xShift, yShift);
                Logger.Log("Obtained Point: {0}", shiftedGeom);
                SqlAssert.IsTrue(shiftedGeom.STEquals(shiftPoint));
            }

            [TestMethod]
            public void VacuousGeometryToGeographyTest()
            {
                var geom = "LINESTRING (0 0 1 1, 2 2 1 1)".GetGeom();
                var expectedGeog = "LINESTRING (0 0, 2 2)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geom);
                Logger.Log("Expected Geography: {0}", expectedGeog);
                var obtainedGeog = Geometry.VacuousGeometryToGeography(geom, Constants.DefaultSRID);
                Logger.Log("Obtained Geography: {0}", obtainedGeog);
                SqlAssert.IsTrue(obtainedGeog.STEquals(expectedGeog));
            }
        }

        [TestClass]
        public class GeographyTests : BaseUnitTest
        {
            [TestMethod]
            public void ConvexHullGeographyFromTextTest()
            {
                var geomText = "LINESTRING(-122.360 47.656, -122.343 47.656)";
                var expectedGeog = "LINESTRING (-122.343 47.655999999999992, -122.36 47.655999999999992)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geomText);
                var result = Geography.ConvexHullGeographyFromText(geomText, Constants.DefaultSRID);
                Logger.LogLine("Expected result: {0}", expectedGeog);
                Logger.LogLine("Obtained result: {0}", result);
                SqlAssert.IsTrue(result.STEquals(expectedGeog));
            }

            [TestMethod]
            public void ConvexHullGeographyTest()
            {
                var geog = "LINESTRING(-122.360 47.656, -122.343 47.656)".GetGeog();
                var expectedGeog = "LINESTRING (-122.343 47.655999999999992, -122.36 47.655999999999992)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geog);
                var result = Geography.ConvexHullGeography(geog);
                Logger.LogLine("Expected result: {0}", expectedGeog);
                Logger.LogLine("Obtained result: {0}", result);
                SqlAssert.IsTrue(result.STEquals(expectedGeog));
            }

            [TestMethod]
            public void DensifyGeographyTest()
            {
                var geog = "LINESTRING(-5 0, 5 0)".GetGeog();
                var expectedGeog = "LINESTRING (-5 0, -3 0, -1.0000000000000004 0, 0.99999999999999956 0, 2.9999999999999978 0, 5 0)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geog);
                var result = Geography.DensifyGeography(geog, 2.0);
                Logger.LogLine("Expected result: {0}", expectedGeog);
                Logger.LogLine("Obtained result: {0}", result);
                SqlAssert.IsTrue(result.STEquals(expectedGeog));
            }

            [TestMethod]
            public void FilterArtifactsGeographyTest()
            {
                var geog = "GEOMETRYCOLLECTION(LINESTRING EMPTY, LINESTRING (1 1, 3 5), POINT (1 1), POLYGON ((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1)))".GetGeog();

                // Empty line and point should be removed
                // short line should be removed - tolerance length
                const double shortLineTolerance = 500000.0F;
                // Polygon inner ring with area < tolerance * polygon length
                const double polygonAreaTolerance = 150000.0F;

                Logger.LogLine("Input Geography: {0}", geog);
                Logger.Log("Filtering input geometry; removing empty line string");
                Logger.Log("points, short line of tolerance: {0}, Polygon with inner ring area tolerance: {1}", shortLineTolerance, polygonAreaTolerance);
                var expectedGeog = "GEOMETRYCOLLECTION EMPTY".GetGeog();
                var filteredGeog = Geography.FilterArtifactsGeography(geog, true, true, shortLineTolerance, polygonAreaTolerance);
                Logger.Log("Expected converted geog: {0}", expectedGeog);
                Logger.Log("Obtained converted geog: {0}", filteredGeog);
                SqlAssert.IsTrue(filteredGeog.STEquals(expectedGeog));
            }

            [TestMethod]
            public void InterpolateBetweenGeogTest()
            {
                var geog1 = "POINT(0 0 0 0)".GetGeog();
                var geog2 = "POINT(10 0 0 10)".GetGeog();
                var returnPoint = "POINT (4.7441999536520428E-05 0)".GetGeog();
                const int distance = 5;
                Logger.LogLine("Input Point 1:{0} Point 2:{1}", geog1, geog2);
                Logger.Log("Interpolating at a distance of {0}", geog1, geog2, distance);
                Logger.LogLine("Expected Point: {0}", returnPoint);
                var sqlGeography = Geography.InterpolateBetweenGeog(geog1, geog2, distance);
                Logger.Log("Obtained Point: {0}", sqlGeography.ToString());
                SqlAssert.IsTrue(sqlGeography.STEquals(returnPoint));
            }

            [TestMethod]
            public void IsValidGeographyFromGeometryTest()
            {
                var geom = "LINESTRING (0 0 1 1, 2 2 1 1)".GetGeom();
                Logger.LogLine("Input Geography: {0}", geom);
                var result = Geography.IsValidGeographyFromGeometry(geom);
                Logger.LogLine("Expected result: true, Obtained result: {0}", result);
                SqlAssert.IsTrue(result);
            }

            [TestMethod]
            public void IsValidGeographyFromTextTest()
            {
                var geogText = "CURVEPOLYGON (CIRCULARSTRING (0 -4, 4 0, 0 4, -4 0, 0 -4)";
                Logger.LogLine("Input Geography: {0}", geogText);
                var result = Geography.IsValidGeographyFromText(geogText, Constants.DefaultSRID);
                Logger.LogLine("Expected result: false, Obtained result: {0}", result);
                SqlAssert.IsFalse(result);

                geogText = "CURVEPOLYGON (CIRCULARSTRING (0 -4, 4 0, 0 4, -4 0, 0 -4))";
                Logger.LogLine("Input Geography: {0}", geogText);
                result = Geography.IsValidGeographyFromText(geogText, Constants.DefaultSRID);
                Logger.LogLine("Expected result: false, Obtained result: {0}", result);
                SqlAssert.IsTrue(result);
            }

            [TestMethod]
            public void LocatePointAlongGeogTest()
            {
                var geog = "LINESTRING (0 0, 10 0)".GetGeog();
                Logger.Log("Input Geom : {0}", geog.ToString());
                var returnPoint = "POINT (4.7441999536520428E-05 0)".GetGeog();
                var distance = 5;

                Logger.LogLine("Locating a point at distance of {0} Measure", distance);
                var locatedPoint = Geography.LocatePointAlongGeog(geog, distance);
                Logger.Log("Expected point: {0}", returnPoint);
                Logger.Log("Located  point: {0} at distance of {1} Measure", locatedPoint, distance);
                SqlAssert.IsTrue(locatedPoint.STEquals(returnPoint));
            }

            [TestMethod]
            public void MakeValidGeographyFromGeometryTest()
            {
                var geom = "LINESTRING(-122.360 47.656, -122.343 47.656)".GetGeom();
                var expectedGeog = "LINESTRING (-122.343 47.655999999999992, -122.36 47.655999999999992)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geom);
                var result = Geography.MakeValidGeographyFromGeometry(geom);
                Logger.LogLine("Expected result: {0}", expectedGeog);
                Logger.LogLine("Obtained result: {0}", result);
                SqlAssert.IsTrue(result.STEquals(expectedGeog));
            }

            [TestMethod]
            public void MakeValidGeographyFromTextTest()
            {
                var geomText = "LINESTRING(-122.360 47.656, -122.343 47.656)";
                var expectedGeog = "LINESTRING (-122.343 47.655999999999992, -122.36 47.655999999999992)".GetGeog();
                Logger.LogLine("Input Geometry: {0}", geomText);
                var result = Geography.MakeValidGeographyFromText(geomText, Constants.DefaultSRID);
                Logger.LogLine("Expected result: {0}", expectedGeog);
                Logger.LogLine("Obtained result: {0}", result);
                SqlAssert.IsTrue(result.STEquals(expectedGeog));
            }

            [TestMethod]
            public void VacuousGeographyToGeometry()
            {
                var geog = "LINESTRING (0 0 1 1, 2 2 1 1)".GetGeog();
                var expectedGeom = "LINESTRING (0 0, 2 2)".GetGeom();
                Logger.LogLine("Input Geometry: {0}", geog);
                Logger.Log("Expected Geography: {0}", expectedGeom);
                var obtainedGeom = Geography.VacuousGeographyToGeometry(geog, Constants.DefaultSRID);
                Logger.Log("Obtained Geography: {0}", obtainedGeom);
                SqlAssert.IsTrue(obtainedGeom.STEquals(expectedGeom));
            }

        }
    }
}