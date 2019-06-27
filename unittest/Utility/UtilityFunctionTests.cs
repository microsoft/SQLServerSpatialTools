//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Types;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.Utility
{
    [TestClass]
    public class UtilityFunctionTests : BaseUnitTest
    {
        [TestMethod]
        public void DimensionalInfoTest()
        {
            var geom = "POINT EMPTY".GetGeom();
            Assert.AreEqual(DimensionalInfo.None, geom.STGetDimension());

            geom = "POINT (1 1)".GetGeom();
            Assert.AreEqual(DimensionalInfo.Dim2D, geom.STGetDimension());

            geom = "POINT (1 1 null 1)".GetGeom();
            Assert.AreEqual(DimensionalInfo.Dim2DWithMeasure, geom.STGetDimension());

            geom = "POINT (1 1 1)".GetGeom();
            Assert.AreEqual(DimensionalInfo.Dim3D, geom.STGetDimension());

            geom = "POINT (1 1 1 1)".GetGeom();
            Assert.AreEqual(DimensionalInfo.Dim3DWithMeasure, geom.STGetDimension());

            Assert.AreEqual(DimensionalInfo.Dim2D.GetString(), "2 Dimensional point, with x and y");
        }

        [TestMethod]
        public void IsWithinRangeTest()
        {
            var geom = "LineString (1 1 NULL -4, 4 4 NULL 4)".GetGeom();
            double measure = -1;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -3;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 2;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = -15;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = 5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            geom = "LineString (1 1 NULL 4, -4 -4 NULL -4)".GetGeom();
            measure = -1;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -3;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 2;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = 4;
            Assert.IsTrue(measure.IsWithinRange(geom));

            measure = -5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = -15;
            Assert.IsFalse(measure.IsWithinRange(geom));

            measure = 5;
            Assert.IsFalse(measure.IsWithinRange(geom));

            double? m = 10.0;
            double? mStart = 11.0;

            Assert.IsFalse(m.IsWithinRange(mStart, null));
        }

        [TestMethod]
        public void CheckLRSType()
        {
            var geom = "GEOMETRYCOLLECTION(POINT(3 3 1), POLYGON((0 0 2, 1 10 3, 1 0 4, 0 0 2)))".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom =
                "GEOMETRYCOLLECTION(LINESTRING(1 1, 3 5), MULTILINESTRING((-1 -1, 1 -5, -5 5), (-5 -1, -1 -1)), POINT(5 6))"
                    .GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "LINESTRING(1 1, 3 5)".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "MULTILINESTRING((-1 -1, 1 -5, -5 5), (-5 -1, -1 -1))".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "POINT(5 6)".GetGeom();
            Assert.IsTrue(geom.IsLRSType());

            geom = "POLYGON((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1))".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom = "CIRCULARSTRING(1 1, 2 0, 2 0, 2 0, 1 1)".GetGeom();
            Assert.IsFalse(geom.IsLRSType());

            geom = "POINT(5 6)".GetGeom();
            Assert.IsTrue(geom.IsOfSupportedTypes(OpenGisGeometryType.Point));
        }

        [TestMethod]
        public void LogErrorTest()
        {
            Logger.LogError(new Exception("TestException"));
            Logger.LogError(new Exception("TestException"), "Test");
            Logger.LogError(new Exception("TestException"), "Test : {0}", 1);
            Logger.LogError(new Exception("TestException"), "Test : {0}, {1}", "Value", 1);
            var innerException = new MyException("InnerException", "Stack trace");
            Logger.LogError(new Exception("TestException", innerException), "Test : {0}, {1}", "Value", 1);
        }

        [TestMethod]
        public void TrimDecimalPointsTest()
        {
            const string inputWKT = "";
            var result = inputWKT.TrimDecimalPoints();
            Assert.AreEqual(inputWKT, result);
        }

        [TestMethod]
        public void LRSMultiLineToSqlTest()
        {
            // empty LRS Point check
            var point1 = new LRSPoint("LINESTRING EMPTY".GetGeom());
            var hashCode = point1.GetHashCode();
            Assert.IsTrue(hashCode > 0);
            Assert.AreEqual(point1.ToString(), "POINT (0 0 )");

            // LRS Point Equality check
            point1 = new LRSPoint("POINT(1 1 0 1)".GetGeom());
            var point2 = new LRSPoint("POINT(1 1 0 1)".GetGeom());
            Assert.IsTrue(point1 == point2);

            // range check
            Assert.IsTrue(point1.IsXYWithinTolerance(point2, 0.5));

            point2 = new LRSPoint("POINT(2 2 0 2)".GetGeom());
            var point3 = new LRSPoint("POINT(3 3 0 3)".GetGeom());
            var point4 = new LRSPoint("POINT(4 4 0 4)".GetGeom());
            var lrsPoints = new List<LRSPoint> {point1, point2, point3};

            // next and previous point
            Assert.AreEqual(LRSPoint.GetNextPoint(ref lrsPoints, point2), point3);
            Assert.AreEqual(LRSPoint.GetNextPoint(ref lrsPoints, point4), null);
            Assert.AreEqual(LRSPoint.GetPreviousPoint(ref lrsPoints, point2), point1);
            Assert.AreEqual(LRSPoint.GetPreviousPoint(ref lrsPoints, point4), null);

            // Multiline
            var lrs = new LRSMultiLine(4326);
            SqlGeometryBuilder geomBuilder = null;
            lrs.ToSqlGeometry(ref geomBuilder);
            lrs.BuildSqlGeometry(ref geomBuilder);

            geomBuilder = new SqlGeometryBuilder();
            lrs.ToSqlGeometry(ref geomBuilder);
            lrs.BuildSqlGeometry(ref geomBuilder);

            var lrsLine = new LRSLine(4326);
            lrsLine.AddPoint(new LRSPoint(1, 1, 0, 1, 4326));
            lrs.AddLine(lrsLine);

            geomBuilder = new SqlGeometryBuilder();
            lrs.ToSqlGeometry(ref geomBuilder);

            var lrsLine1 = new LRSLine(4326);
            var wkt = lrs.ToString();
            Assert.AreEqual("MULTILINESTRING EMPTY", wkt);
            Assert.AreEqual(lrs.GetPointAtM(10), null);

            var pt = new LRSPoint(1, 1, 0, 1, 4326);
            lrsLine1.AddPoint(pt);
            Assert.AreEqual(lrsLine1.ToString(), "POINT (1 1 1)");

            pt = new LRSPoint(2, 2, 0, 2, 4326);
            lrsLine1.AddPoint(pt);

            lrs = new LRSMultiLine(4326);
            lrs.AddLine(lrsLine1);

            wkt = lrs.ToString();
            Assert.AreEqual("LINESTRING (1 1 1, 2 2 2)", wkt);

            var lrsLine2 = new LRSLine(4326);

            pt = new LRSPoint(3, 3, 0, 3, 4326);
            Assert.AreEqual(pt.ToString(), "POINT (3 3 3)");
            lrsLine2.AddPoint(pt);
            pt = new LRSPoint(4, 4, 0, 4, 4326);
            lrsLine2.AddPoint(pt);

            lrs = new LRSMultiLine(4326);
            lrs.AddLine(lrsLine1);
            lrs.AddLine(lrsLine2);
            geomBuilder = null;
            lrs.BuildSqlGeometry(ref geomBuilder);

            Assert.AreEqual(lrs.GetPointAtM(10), null);
            Assert.AreEqual(lrs.GetPointAtM(2), new LRSPoint(2, 2, 0, 2, 4326));
            wkt = lrs.ToString();
            lrs.RemoveFirst();
            Assert.AreEqual(lrs.Count, 1);
            lrs.RemoveLast();
            Assert.AreEqual(lrs.Count, 0);
            lrs.RemoveFirst();
            lrs.RemoveLast();
            Assert.AreEqual("MULTILINESTRING ((1 1 1, 2 2 2), (3 3 3, 4 4 4))", wkt);
        }

        private class MyException : Exception
        {
            public MyException(string message, string stackTrace) : base(message)
            {
                StackTrace = stackTrace;
            }

            public override string StackTrace { get; }
        }

        [TestMethod]
        public void ThrowIfMeasureIsNotInRangeTest()
        {
            const int measure = 10;
            var startPoint = "POINT(1 2 0)".GetGeom();
            var endPoint = "POINT(2 2 5)".GetGeom();
            try
            {
                SpatialExtensions.ThrowIfMeasureIsNotInRange(measure, startPoint, endPoint);
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        public void VectorTest()
        {
            var vec1 = new Vector3(1, 1, 0);
            var vec2 = new Vector3(3, 3, 0);
            var angle = vec1.AngleInDegrees(vec2);
            Assert.AreEqual(angle, 38.942441268981391);
        }

        [TestMethod]
        public void LRSLineStringTest()
        {
            var lrsLine = new LRSLine(4326);
            var wkt = lrsLine.ToString();
            Assert.AreEqual(wkt, "LINESTRING EMPTY");
            lrsLine.AddPoint(new LRSPoint(1, 1, 0, 1, 4326));
            lrsLine.AddPoint(new LRSPoint(2, 2, null, null, 4326));
            lrsLine.LocatePoint(3);
            lrsLine.AddPoint(new LRSPoint(3, 3, 0, 3, 4326), new LRSPoint(4, 4, 0, 4, 4326));

            var enumerator = lrsLine.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();
            var currentPoint = enumerator.Current;
            Assert.AreEqual(currentPoint, new LRSPoint(4, 4, 0, 4, 4326));
            try
            {
                enumerator.MoveNext();
                // ReSharper disable once RedundantAssignment
                currentPoint = enumerator.Current;
            }
            catch (InvalidOperationException)
            {
                enumerator.Reset();
            }
        }

        [TestMethod]
        public void STLinearMeasureProgressTest()
        {
            var geom = "LINESTRING EMPTY".GetGeom();
            Assert.AreEqual(geom.STLinearMeasureProgress(), LinearMeasureProgress.None);

            geom = "LINESTRING (1 1 0 1, 2 2 0 5, 3 3 0 3, 4 4 0 2, 5 5 null null)".GetGeom();
            Assert.AreEqual(geom.STLinearMeasureProgress(), LinearMeasureProgress.None);

            geom = "POINT (1 1 1 1)".GetGeom();
            Assert.AreEqual(geom.STLinearMeasureProgress(), LinearMeasureProgress.Increasing);

            geom = "LINESTRING (1 1 0 0, 2 2 0 5, 3 3 0 10)".GetGeom();
            Assert.AreEqual(geom.STLinearMeasureProgress(), LinearMeasureProgress.Increasing);

            geom = "LINESTRING (1 1 0 10, 2 2 0 5, 3 3 0 0)".GetGeom();
            Assert.AreEqual(geom.STLinearMeasureProgress(), LinearMeasureProgress.Decreasing);

            Assert.AreEqual(LinearMeasureProgress.Increasing.Value(), 1);
            Assert.AreEqual(LinearMeasureProgress.Decreasing.Value(), 2);
        }

        [TestMethod]
        public void GetMergeTypeTest()
        {
            var geom1 = "LINESTRING (1 1 1 1, 2 2 2 2)".GetGeom();
            var geom2 = "POINT (1 1 1 1)".GetGeom();

            var mergeType = geom1.GetMergeType(geom2);
            Assert.AreEqual(MergeInputType.None, mergeType);
        }

        [TestMethod]
        public void IsNullOrEmptyTest()
        {
            SqlGeometry geom = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual(geom.IsNullOrEmpty(), true);

            geom = "LINESTRING EMPTY".GetGeom();
            Assert.AreEqual(geom.IsNullOrEmpty(), true);
        }

        [TestMethod]
        public void CompareGeomStringTest()
        {
            var geom = "POINT EMPTY".GetGeom();
            Assert.IsFalse(geom.STGeometryType().Compare(string.Empty));
        }

        [TestMethod]
        public void STHasLinearMeasureTest()
        {
            var geom = "POINT EMPTY".GetGeom();
            Assert.IsFalse(geom.STHasLinearMeasure());

            geom = "LINESTRING (1 1 0 1, 2 2 0 1)".GetGeom();
            Assert.IsTrue(geom.STHasLinearMeasure());

            geom = "LINESTRING (1 1, 2 2)".GetGeom();
            Assert.IsFalse(geom.STHasLinearMeasure());
        }

        [TestMethod]
        public void ThrowIfNotOfTypeTest()
        {
            var geom = "POINT EMPTY".GetGeom();
            try
            {
                SpatialExtensions.ThrowIfNotLineOrMultiLine(geom);
            }
            catch (ArgumentException)
            {

            }

            try
            {
                SpatialExtensions.ThrowIfNotLine(geom);
            }
            catch (ArgumentException)
            {

            }

            geom = "LINESTRING EMPTY".GetGeom();
            try
            {
                SpatialExtensions.ThrowIfNotPoint(geom);
            }
            catch (ArgumentException)
            {

            }

            try
            {
                SpatialExtensions.ThrowException("Test : {0}", "Test Message");
            }
            catch (ArgumentException)
            {

            }
        }

        [TestMethod]
        public void IsTolerableTest()
        {
            const double distance = 0.7;
            Assert.IsFalse(distance.IsTolerable(0.05));
            Assert.IsTrue(distance.IsTolerable(0.9));
        }

        [TestMethod]
        public void EqualsToTest()
        {
            const double distance = 0.7;
            Assert.IsFalse(distance.EqualsTo(0.05));

            double? d1 = 0.5;
            Assert.IsTrue(d1.EqualsTo(0.5));

            double? d2 = 0.5;
            Assert.IsFalse(distance.EqualsTo(d1));

            Assert.IsTrue(d1.EqualsTo(d2));
        }


        [TestMethod]
        public void GISTypesTest()
        {
            var geom = "MULTIPOLYGON(((1 1, 1 -1, -1 -1, -1 1, 1 1)),((1 1, 3 1, 3 3, 1 3, 1 1)))".GetGeom();
            Assert.IsTrue(geom.IsMultiPolygon());

            geom = "MULTIPOINT((2 3), (7 8 9.5))".GetGeom();
            Assert.IsTrue(geom.IsMultiPoint());

            geom = "CURVEPOLYGON(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3))".GetGeom();
            Assert.IsTrue(geom.IsCurvePolygon());

            geom = "COMPOUNDCURVE(CIRCULARSTRING(1 1, 1 1, 1 1), (1 1, 3 5, 5 4))".GetGeom();
            Assert.IsTrue(geom.IsCompoundCurve());

            geom = "CIRCULARSTRING(1 1, 2 0, -1 1)".GetGeom();
            Assert.IsTrue(geom.IsCircularString());

            geom = "POLYGON((1 1, 3 3, 3 1, 1 1))".GetGeom();
            Assert.IsTrue(geom.IsPolygon());
        }
    }
}
