//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.LRS;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class LRSFunctionTests : BaseDDDFunctionTest 
    {
        [TestMethod]
        public void ClipGeometrySegmentTest()
        {
            Logger.LogLine("Clip Geometry Segments Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.ClipGeometrySegmentData>(LRSDataSet.ClipGeometrySegmentData.SelectQuery);

            var clipGeometrySegmentData = dataSet.ToList();
            if (!clipGeometrySegmentData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in clipGeometrySegmentData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Start Measure : {0}", test.StartMeasure);
                    Logger.Log("End Measure : {0}", test.EndMeasure);
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.ClipGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure, test.Tolerance)?.ToString().TrimNullValue();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoClipGeometrySegment(test);
                OracleTimer.Stop();

                #endregion

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.ClipGeometrySegmentData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ConvertToLrsGeomTest()
        {
            Logger.LogLine("ConvertedLrsGeomTest() Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.ConvertToLrsGeomData>(LRSDataSet.ConvertToLrsGeomData.SelectQuery);

            var ConvertToLrsGeomData = dataSet.ToList();
            if (!ConvertToLrsGeomData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in ConvertToLrsGeomData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();
                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    if (Geometry.ConvertToLrsGeom(geom, test.StartMeasure, test.EndMeasure) != null)
                        test.SqlObtainedResult1 = Geometry.ConvertToLrsGeom(geom, test.StartMeasure, test.EndMeasure).ToString();
                    else
                        test.SqlObtainedResult1 = null;

                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoConvertToLrsGeom(test);
                OracleTimer.Stop();
                Logger.Log("Oracle Result: {0}", test.OracleResult1);

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.ConvertToLrsGeomData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void GetEndMeasureTest()
        {
            Logger.LogLine("Get End Measure Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.GetEndMeasureData>(LRSDataSet.GetEndMeasureData.SelectQuery);

            var getEndMeasureData = dataSet.ToList();
            if (!getEndMeasureData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in getEndMeasureData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.GetEndMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoGetEndMeasure(test);
                OracleTimer.Stop();

                #endregion


                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.GetEndMeasureData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void GetStartMeasureTest()
        {
            Logger.LogLine("Get Start Measure Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.GetStartMeasureData>(LRSDataSet.GetStartMeasureData.SelectQuery);

            var getStartMeasureData = dataSet.ToList();
            if (!getStartMeasureData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in getStartMeasureData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.GetStartMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoGetStartMeasure(test);
                OracleTimer.Stop();

                #endregion


                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.GetStartMeasureData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void InterpolateBetweenGeomTest()
        {
            Logger.LogLine("Interpolate Between Points Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.InterpolateBetweenGeomData>(LRSDataSet.InterpolateBetweenGeomData.SelectQuery);

            var interpolateBetweenGeomData = dataSet.ToList();
            if (!interpolateBetweenGeomData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in interpolateBetweenGeomData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment1 = test.InputGeom1.GetGeom();
                    var inputGeomSegment2 = test.InputGeom2.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString());
                    Logger.Log("Interpolate with a distance of {0}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString(), test.Measure);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.InterpolateBetweenGeom(inputGeomSegment1, inputGeomSegment2, test.Measure).ToString();
                    MSSQLTimer.Stop();
                    test.SetElapsedTime(MSSQLTimer.Elapsed);
                    Logger.Log("Obtained Point: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                // Update results to database
                UpdateSqlServerTestResults(test, LRSDataSet.InterpolateBetweenGeomData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void IsConnectedTest()
        {
            Logger.LogLine("IsConnected Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.IsConnectedData>(LRSDataSet.IsConnectedData.SelectQuery);

            var isConnectedData = dataSet.ToList();
            if (!isConnectedData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in isConnectedData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment1 = test.InputGeom1.GetGeom();
                    var inputGeomSegment2 = test.InputGeom2.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.LogLine("Input geom 1:{0} geom 2:{1}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString());
                    Logger.Log("IsConnected with a tolerance of {0}", inputGeomSegment1.ToString(), inputGeomSegment2.ToString(), test.Tolerance);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.IsConnected(inputGeomSegment1, inputGeomSegment2, test.Tolerance).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Point: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoIsConnectedGeomSegmentTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.IsConnectedData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void LocatePointAlongGeomTest()
        {
            Logger.LogLine("LocatePointAlongGeom Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.LocatePointAlongGeomData>(LRSDataSet.LocatePointAlongGeomData.SelectQuery);

            var locatePointAlongGeomData = dataSet.ToList();
            if (!locatePointAlongGeomData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in locatePointAlongGeomData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment1 = test.InputGeom.GetGeom();
                    Logger.LogLine("Input geom :{0}", inputGeomSegment1.ToString());
                    Logger.Log("Location point along Geom at a measure of {0}", test.Measure);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.LocatePointAlongGeom(inputGeomSegment1, test.Measure).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoLocatePointAlongGeomTest(test);
                Logger.Log("Oracle Result: {0}", test.OracleResult1);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.LocatePointAlongGeomData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void MergeGeometrySegmentsTest()
        {
            Logger.LogLine("Merge Geometry Segments Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.MergeGeometrySegmentsData>(LRSDataSet.MergeGeometrySegmentsData.SelectQuery);

            var mergeGeometrySegmentsData = dataSet.ToList();
            if (!mergeGeometrySegmentsData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in mergeGeometrySegmentsData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();
                    var expectedGeom = test.ExpectedResult1.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom1);
                    Logger.Log("Input geom 2:{0}", geom2);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);

                    MSSQLTimer.Restart();
                    // OSS Function Execution

                    test.SqlObtainedResult1 = Geometry.MergeGeometrySegments(geom1, geom2, test.Tolerance).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result : {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoMergeGeomTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.MergeGeometrySegmentsData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void MergeAndResetGeometrySegments()
        {
            Logger.LogLine("Merge and Reset Geometry Segments Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.MergeAndResetGeometrySegmentsData>(LRSDataSet.MergeAndResetGeometrySegmentsData.SelectQuery);
            var mergeAndResetGeometrySegmentsData = dataSet.ToList();
            if (!mergeAndResetGeometrySegmentsData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1;
            foreach (var test in mergeAndResetGeometrySegmentsData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom1 = test.InputGeom1.GetGeom();
                    var geom2 = test.InputGeom2.GetGeom();
                    var expectedGeom = test.ExpectedResult1.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom1);
                    Logger.Log("Input geom 2:{0}", geom2);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);

                    MSSQLTimer.Restart();
                    // OSS Function Execution

                    test.SqlObtainedResult1 = Geometry.MergeAndResetGeometrySegments(geom1, geom2, test.Tolerance).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result : {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoMergeAndResetGeomTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.MergeAndResetGeometrySegmentsData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void OffsetGeometrySegmentTest()
        {
            Logger.LogLine("Offset Geometry Segments Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.OffsetGeometrySegmentData>(LRSDataSet.OffsetGeometrySegmentData.SelectQuery);

            var offsetGeometrySegmentData = dataSet.ToList();
            if (!offsetGeometrySegmentData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in offsetGeometrySegmentData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Start Measure : {0}", test.StartMeasure);
                    Logger.Log("End Measure : {0}", test.EndMeasure);
                    Logger.Log("Offset : {0}", test.Offset);
                    Logger.Log("Tolerance : {0}", test.Tolerance);
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.OffsetGeometrySegment(inputGeomSegment, test.StartMeasure, test.EndMeasure, test.Offset, test.Tolerance)?.ToString().TrimNullValue();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoOffsetGeometrySegment(test);
                OracleTimer.Stop();

                #endregion

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.OffsetGeometrySegmentData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void PopulateGeometryMeasuresTest()
        {
            Logger.LogLine("PopulateGeometryMeasures Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.PopulateGeometryMeasuresData>(LRSDataSet.PopulateGeometryMeasuresData.SelectQuery);

            var populateGeometryMeasuresData = dataSet.ToList();
            if (!populateGeometryMeasuresData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in populateGeometryMeasuresData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();
                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.PopulateGeometryMeasures(geom, test.StartMeasure, test.EndMeasure).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoPopulateMeasuresTest(test);
                OracleTimer.Stop();
                Logger.Log("Oracle Result: {0}", test.OracleResult1);

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.PopulateGeometryMeasuresData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ResetMeasureTest()
        {
            Logger.LogLine("Reset Measure Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.ResetMeasureData>(LRSDataSet.ResetMeasureData.SelectQuery);

            var resetMeasureData = dataSet.ToList();
            if (!resetMeasureData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in resetMeasureData)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    var expectedGeomSegment = test.ExpectedResult1.GetGeom();
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", expectedGeomSegment.ToString());

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.ResetMeasure(inputGeomSegment).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                // Update results to database
                UpdateSqlServerTestResults(test, LRSDataSet.ResetMeasureData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ReverseLinearGeometryTest()
        {
            Logger.LogLine("ReverseLinearGeometry Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.ReverseLinearGeometryData>(LRSDataSet.ReverseLinearGeometryData.SelectQuery);

            var reverseLinearGeometryData = dataSet.ToList();
            if (!reverseLinearGeometryData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in reverseLinearGeometryData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();

                    Logger.LogLine("Input geom: {0}", geom);
                    Logger.LogLine("Expected Result: {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.ReverseLinearGeometry(geom).ToString();
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoReverseLinearGeomTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.ReverseLinearGeometryData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void SplitGeometrySegmentTest()
        {
            Logger.LogLine("SplitGeometrySegment Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.SplitGeometrySegmentData>(LRSDataSet.SplitGeometrySegmentData.SelectQuery);

            var splitGeometrySegmentData = dataSet.ToList();
            if (!splitGeometrySegmentData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in splitGeometrySegmentData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();

                    Logger.LogLine("Splitting Input geom: {0} at a measure of : {1}", geom, test.Measure);
                    Logger.Log("Expected Split Geom Segment 1: {0}", test.ExpectedResult1);
                    Logger.Log("Expected Split Geom Segment 2: {0}", test.ExpectedResult2);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    Geometry.SplitGeometrySegment(geom, test.Measure, out var obtainedGeom1, out var obtainedGeom2);
                    MSSQLTimer.Stop();

                    test.SqlObtainedResult1 = obtainedGeom1?.ToString();
                    test.SqlObtainedResult2 = obtainedGeom2?.ToString();

                    Logger.LogLine("Obtained Result1: {0}", test.SqlObtainedResult1);
                    Logger.Log("Obtained Result2: {0}", test.SqlObtainedResult2);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.DoSplitGeometrySegmentTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, LRSDataSet.SplitGeometrySegmentData.TableName, testIterator, true);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void ValidateLRSGeometryTest()
        {
            Logger.LogLine("Validate LRS Geometry Segments Tests");
            var dataSet = DBConnectionObj.Query<LRSDataSet.ValidateLRSGeometryData>(LRSDataSet.ValidateLRSGeometryData.SelectQuery);
            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in dataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var inputGeomSegment = test.InputGeom.GetGeom();
                    test.ExpectedResult1 = test.ExpectedResult1;
                    Logger.Log("Input Geom : {0}", inputGeomSegment.ToString());
                    Logger.Log("Expected Geom : {0}", test.ExpectedResult1);

                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.ValidateLRSGeometry(inputGeomSegment).ToString(CultureInfo.CurrentCulture);
                    MSSQLTimer.Stop();
                    Logger.Log("Obtained Result: {0}", test.SqlObtainedResult1);
                }
                catch (Exception ex)
                {
                    test.Result = "Failed";
                    test.SqlError = ex.Message;
                    Logger.LogError(ex);
                }

                #endregion

                #region Run against Oracle

                OracleTimer.Restart();
                // Oracle Function Execution
                OracleConnectorObj.ValidateLRSGeometry(test);
                OracleTimer.Stop();

                #endregion

                // Update test results in DB
                UpdateTestResults(test, LRSDataSet.ValidateLRSGeometryData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
            if (testIterator == 1)
                Logger.Log("No test cases found");
        }        
    }
}
