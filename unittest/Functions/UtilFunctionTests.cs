//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.Util;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Functions
{
    [TestClass]
    public class UtilFunctionTests
    {
        [TestMethod]
        public void ExtractPointTest()
        {
            var geom = "POINT(1 1 1)".GetGeom();
            var expected = "POINT(1 1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 4, 2);
                Assert.Fail("Should through exception : Invalid index for element to be extracted.");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : Invalid index for sub-element to be extracted.");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            geom = "MULTIPOINT((1 1 1), (2 2 2), (3 3 3), (4 4 4))".GetGeom();
            expected = geom;
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            geom = "MULTIPOINT((1 1 1), (2 2 2), (3 3 3), (4 4 4))".GetGeom();
            expected = "POINT(1 1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            expected = "POINT(3 3 3)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 3)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 5);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractLineStringTest()
        {
            // LINESTRING
            var geom = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            var expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // CIRCULARSTRING
            geom = "CIRCULARSTRING(1 1, 2 0, -1 1)".GetGeom();
            expected = "CIRCULARSTRING(1 1, 2 0, -1 1)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // MULTILINESTRING
            geom = "MULTILINESTRING((1 1 1, 2 2 2), (3 3 3, 4 4 4))".GetGeom();
            expected = "LINESTRING(1 1 1, 2 2 2)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            expected = "LINESTRING(3 3 3, 4 4 4)".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 3, 2);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractPolygonTest()
        {
            // Single Polygon - Sub index is to extract the inner rings
            var geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            var expected = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            expected = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))".GetGeom();
            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
            // here for the interior ring; the polygon is rotated; so checking the points
            SqlAssert.IsTrue(expected.STPointN(2).STEquals(obtainedGeom.STPointN(2)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            // Multi Polygon
            geom = "MULTIPOLYGON(((1 1, 1 -1, -1 -1, -1 1, 1 1)),((1 1, 3 1, 3 3, 1 1)))".GetGeom();
            expected = "POLYGON((1 1, 1 -1, -1 -1, -1 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            expected = "POLYGON((1 1, 3 1, 3 3, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 0)));

            try
            {
                Geometry.ExtractGeometry(geom, 3);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            geom = "MULTIPOLYGON(((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), ((9 9, 9 10, 10 9, 9 9)))".GetGeom();
            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 0)));

            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 1)));

            expected = "POLYGON((1 1, 1 2, 2 1, 1 1))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 1, 2)));

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            expected = "POLYGON((9 9, 9 10, 10 9, 9 9))".GetGeom();
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 0)));
            SqlAssert.IsTrue(expected.STEquals(Geometry.ExtractGeometry(geom, 2, 1)));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }

        [TestMethod]
        public void ExtractCurvePolygonTest()
        {
            var geom = "CURVEPOLYGON (CIRCULARSTRING (3 3, 4 9, 2 3, 0 0, 3 3), (1 1, 2 2, 2 1, 1 1))".GetGeom();
            var expected = "CURVEPOLYGON (CIRCULARSTRING (3 3, 4 9, 2 3, 0 0, 3 3), (1 1, 2 2, 2 1, 1 1))".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "CURVEPOLYGON (CIRCULARSTRING (3 3, 4 9, 2 3, 0 0, 3 3))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON ((1 1, 2 1, 2 2, 1 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
            // here for the interior ring; the curve polygon is rotated; so checking the points
            SqlAssert.IsTrue(expected.STPointN(2).STEquals(obtainedGeom.STPointN(2)));

            geom = "CURVEPOLYGON ((0 1, 0.5 0.5, 1 0, 0.8 0.8, 0 1), CIRCULARSTRING(0.8 0.4, 0.6 0.6, 0.2 0.9, 0.7 0.7, 0.8 0.4))".GetGeom();
            expected = "POLYGON ((0 1, 0.5 0.5, 1 0, 0.8 0.8, 0 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
            // here for the exterior ring; the curve polygon shouldn't be rotated; so checking the points
            SqlAssert.IsTrue(expected.STPointN(2).STEquals(obtainedGeom.STPointN(2)));

            expected = "CURVEPOLYGON(CIRCULARSTRING(0.8 0.4, 0.7 0.7, 0.2 0.9, 0.6 0.6, 0.8 0.4))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "CURVEPOLYGON ((0 1, 0.5 0.5, 1 0, 0.8 0.8, 0 1), (0.8 0.4, 0.6 0.6, 0.2 0.9, 0.7 0.7, 0.8 0.4))".GetGeom();
            expected = "POLYGON ((0 1, 0.5 0.5, 1 0, 0.8 0.8, 0 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
            // here for the exterior ring; the curve polygon shouldn't be rotated; so checking the points
            SqlAssert.IsTrue(expected.STPointN(2).STEquals(obtainedGeom.STPointN(2)));

            expected = "POLYGON ((0.8 0.4, 0.7 0.7, 0.2 0.9, 0.6 0.6, 0.8 0.4))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 0);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            geom = "CURVEPOLYGON(COMPOUNDCURVE(CIRCULARSTRING (1 0, 0.7 0.7, 0 1), (0 1, 1 0)))".GetGeom();
            expected = "CURVEPOLYGON(COMPOUNDCURVE(CIRCULARSTRING (1 0, 0.7 0.7, 0 1), (0 1, 1 0)))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "COMPOUNDCURVE(CIRCULARSTRING (1 0, 0.7 0.7, 0 1), (0 1, 1 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 2, 0);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            geom = "CURVEPOLYGON(COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3)), COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 2, 5 6, 6 3, 1 3)))".GetGeom();
            expected = "CURVEPOLYGON(COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3)), COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 2, 5 6, 6 3, 1 3)))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "COMPOUNDCURVE(CIRCULARSTRING(1 3, 3 2, 5 6, 6 3, 1 3))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
        }

        [TestMethod]
        public void ExtractCompoundCurveTest()
        {
            var geom = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            var expected = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 0, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }
        }


        [TestMethod]
        public void ExtractGeometryCollectionTest()
        {
            var geom = "GEOMETRYCOLLECTION(LINESTRING(1 1, 2 2), COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0)))".GetGeom();
            var expected = "LINESTRING(1 1, 2 2)".GetGeom();
            var obtainedGeom = Geometry.ExtractGeometry(geom, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 0);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            try
            {
                Geometry.ExtractGeometry(geom, 0, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 3, 1);
                Assert.Fail("Should through exception : invalid index for element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 2);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }

            try
            {
                Geometry.ExtractGeometry(geom, 1, 3);
                Assert.Fail("Should through exception : invalid index for sub-element to be extracted");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Invalid index for sub-element to be extracted.", ex.Message);
            }


            geom = "GEOMETRYCOLLECTION(MULTILINESTRING((1 1, 2 2), (4 4, 5 5, 7 7), (8 8, 9 9)), POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)))".GetGeom();
            expected = "LINESTRING(4 4, 5 5, 7 7)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "LINESTRING(8 8, 9 9)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 1, 3);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON((0 0, 0 3, 3 3, 3 0, 0 0))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 1);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POLYGON((1 1, 1 2, 2 1, 1 1))".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 2, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            geom = "GEOMETRYCOLLECTION(MULTILINESTRING((1 1, 2 2), (4 4, 5 5, 7 7), (8 8, 9 9)), POLYGON((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), MULTIPOINT((1 1), (2 2), (4 4)))".GetGeom();

            expected = "POINT(2 2)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 3, 2);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));

            expected = "POINT(4 4)".GetGeom();
            obtainedGeom = Geometry.ExtractGeometry(geom, 3, 3);
            SqlAssert.IsTrue(expected.STEquals(obtainedGeom));
        }

        [TestMethod]
        public void RemoveDuplicateVerticesTest()
        {
            var geometry = "POINT (1 2 3)".GetGeom();
            var expected = "POINT (1 2)".GetGeom();
            var tolerance = 0.5;
            var resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "MULTIPOINT ((1 2 NULL 3), (5 5 NULL 6))".GetGeom();
            expected = "MULTIPOINT ((1 2), (5 5))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "MULTILINESTRING((1 1, 3 2, 3.2 2.2, 3 8), (6 6, 10 10))".GetGeom();
            expected = "MULTILINESTRING ((1 1, 3.2 2.2, 3 8), (6 6, 10 10))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "POLYGON ((1 1, 1 5, 8 2, 7.8 1.8, 1 1, 1 1))".GetGeom();
            expected = "POLYGON ((1 1, 1 5, 8 2, 1 1))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "MULTIPOLYGON (((1 1, 1 -1, -1 -1, -1 1, 1 1)), ((1 1, 3 1, 3.1 3.2, 3.3 3.5 1 3, 1 1)))".GetGeom();
            expected = "MULTIPOLYGON (((1 1, 1 -1, -1 -1, -1 1, 1 1)), ((1 1, 3 1, 3.3 3.5, 1 1)))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "CURVEPOLYGON(CIRCULARSTRING(1 3, 3 5, 4 7, 4.2 7.3, 4.5 7.5, 7 3, 1 3))".GetGeom();
            expected = "CURVEPOLYGON (COMPOUNDCURVE (CIRCULARSTRING (1 3, 3 5, 4 7), (4 7, 4.5 7.5), CIRCULARSTRING (4.5 7.5, 7 3, 1 3)))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            geometry = "GEOMETRYCOLLECTION(LINESTRING(1 1,3 5, 3.2 5.1), POINT (2 3 NULL 1), MULTIPOLYGON(((5 5, 5 10, 10 15, 15 15, 15.4 15.4, 15 10, 5 5))))".GetGeom();
            expected = "GEOMETRYCOLLECTION (LINESTRING (1 1, 3.2 5.1), POINT (2 3), POLYGON ((5 5, 5 10, 10 15, 15.4 15.4, 15 10, 5 5)))".GetGeom();
            tolerance = 0.5;
            resultantGeom = Geometry.RemoveDuplicateVertices(geometry, tolerance);
            SqlAssert.IsTrue(expected.STEquals(resultantGeom));

            //negative cases
            try
            {
                geometry = "LINESTRING (1 1, 6 6, 3 3, 2 2)".GetGeom(); // linestring overlaps
                tolerance = 0.5;
                Geometry.RemoveDuplicateVertices(geometry, tolerance);
                Assert.Fail("Should throw exception for the invalid overlapping geometry");
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ErrorMessage.InvalidGeometry, ex.Message);
            }

            try
            {
                geometry = "MULTILINESTRING((1 1, 3 3, 12 12), (5 5, 5 5))".GetGeom();
                tolerance = 0.5;
                Geometry.RemoveDuplicateVertices(geometry, tolerance);
                Assert.Fail("Should throw exception for invalid geometry");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ErrorMessage.InvalidGeometry, ex.Message);
            }
        }
    }
}
