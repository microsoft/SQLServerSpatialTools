//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.Functions.Util;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class UtilFunctionTests : BaseDDDFunctionTest
    {
        [TestMethod]
        public void PolygonToLineTest()
        {
            var dataSet = DBConnectionObj.Query<UtilDataSet.PolygonToLineData>(UtilDataSet.PolygonToLineData.SelectQuery);

            var polygonToLineData = dataSet.ToList();
            if (!polygonToLineData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in polygonToLineData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom = test.ExpectedResult1.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);

                    MSSQLTimer.Restart();
                    // OSS Function Execution

                    test.SqlObtainedResult1 = Geometry.PolygonToLine(geom).ToString();
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
                OracleConnectorObj.DoPolygonToLineTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, UtilDataSet.PolygonToLineData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }

        [TestMethod]
        public void RemoveDuplicateVertices()
        {
            var dataSet = DBConnectionObj.Query<UtilDataSet.RemoveDuplicateVerticesData>(UtilDataSet.RemoveDuplicateVerticesData.SelectQuery);
            var removeVerticesDataSet = dataSet.ToList();
            if (!removeVerticesDataSet.Any())
                Logger.Log("No test cases found");
            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in removeVerticesDataSet)
            {
                Logger.LogLine("Executing test {0}", testIterator);
                #region Run against OSS
                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var tol = test.Tolerance;
                    var expectedGeom = test.ExpectedResult1.GetGeom();
                    Logger.LogLine("Input geom 1:{0}", geom);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);
                    MSSQLTimer.Restart();
                    // OSS Function Execution
                    test.SqlObtainedResult1 = Geometry.RemoveDuplicateVertices(geom, tol).ToString();
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
                OracleConnectorObj.DoRemoveDuplicateVertices(test);
                OracleTimer.Stop();
                #endregion
                // Update results to database
                UpdateTestResults(test, UtilDataSet.RemoveDuplicateVerticesData.TableName, testIterator);
                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }


        [TestMethod]
        public void ExtractTest()
        {
            var dataSet = DBConnectionObj.Query<UtilDataSet.ExtractData>(UtilDataSet.ExtractData.SelectQuery);

            var extractData = dataSet.ToList();
            if (!extractData.Any())
                Logger.Log("No test cases found");

            var testIterator = 1; PassCount = 0; FailCount = 0;
            foreach (var test in extractData)
            {
                Logger.LogLine("Executing test {0}", testIterator);

                #region Run against OSS

                try
                {
                    var geom = test.InputGeom.GetGeom();
                    var expectedGeom = test.ExpectedResult1.GetGeom();

                    Logger.LogLine("Input geom 1:{0}", geom);
                    Logger.LogLine("Expected Result: {0}", expectedGeom);

                    MSSQLTimer.Restart();
                    // OSS Function Execution

                    test.SqlObtainedResult1 = Geometry.ExtractGeometry(geom, test.ElementIndex, test.ElementSubIndex).ToString();
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
                OracleConnectorObj.DoExtractTest(test);
                OracleTimer.Stop();

                #endregion

                // Update results to database
                UpdateTestResults(test, UtilDataSet.ExtractData.TableName, testIterator);

                Logger.Log("Test Result : {0}", test.Result);
                testIterator++;
            }
        }
    }
}
