//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using SQLSpatialTools.Utility;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.UnitTests.DDD
{
    internal class OracleConnector
    {
        private static OracleConnector _oracleConnectorObj;
        private readonly OracleConnection _oracleConnection;
        private readonly Regex _dimensionParseRegex;
        private readonly Regex _dimensionGroupRegex;
        private readonly Regex _dimensionSplitRegex;

        /// <summary>
        /// Obtain the Oracle Connector instance.
        /// </summary>
        /// <returns>Oracle Connector Object</returns>
        public static OracleConnector GetInstance()
        {
            return _oracleConnectorObj ?? (_oracleConnectorObj = new OracleConnector());
        }

        #region Oracle DB Manipulation

        /// <summary>
        /// Initializes Oracle Connector Object.
        /// Also Checks the Oracle connection defined in Configuration.
        /// </summary>
        private OracleConnector()
        {
            var connStr = ConfigurationManager.AppSettings.Get("oracle_connection");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new ArgumentNullException(message: "Oracle Connection string is empty", paramName: connStr);

            _dimensionParseRegex = new Regex(OracleLRSQuery.DimensionParse, RegexOptions.Compiled);
            _dimensionGroupRegex = new Regex(OracleLRSQuery.DimensionGroup, RegexOptions.Compiled);
            _dimensionSplitRegex = new Regex(OracleLRSQuery.DimensionMatch, RegexOptions.Compiled);

            _oracleConnection = new OracleConnection { ConnectionString = connStr };

            Open();
            Close();

            //drop and create temp table to capture intermediate results.
            var error = string.Empty;

            ExecuteNonQuery(OracleLRSQuery.DropTempTableQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentNullException(error);

            ExecuteNonQuery(OracleLRSQuery.CreateTempTableQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTableIndexQuery, ref error);
            ExecuteNonQuery(OracleLRSQuery.CreateTempTablePkQuery, ref error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentNullException(error);
        }

        /// <summary>
        /// Opens Oracle Connection Object.
        /// </summary>
        private void Open()
        {
            try
            {
                if (_oracleConnection.State != ConnectionState.Open)
                    _oracleConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, "Error in connecting Oracle DB:{0}", ex.Message));
            }
        }

        /// <summary>
        /// Close Oracle Connection Object.
        /// </summary>
        private void Close()
        {
            if (_oracleConnection.State == ConnectionState.Open)
            {
                _oracleConnection.Close();
                //oracleConnection.Dispose();
            }
        }

        /// <summary>
        /// Executes Scalar query against Oracle
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private T ExecuteScalar<T>(string query, out string error)
        {
            var result = default(T);
            error = string.Empty;
            try
            {
                Open();
                result = _oracleConnection.QuerySingle<T>(query);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                Close();
            }
            return result;
        }

        /// <summary>
        /// Executes non query against Oracle.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="error"></param>
        private void ExecuteNonQuery(string query, ref string error)
        {
            var oracleCommand = new OracleCommand
            {
                Connection = _oracleConnection,
                CommandText = query
            };

            try
            {
                Open();
                oracleCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                Close();
            }
        }
        #endregion

        #region LRS Test Functions

        /// <summary>
        /// Test MergeGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoMergeGeomTest(LRSDataSet.MergeGeometrySegmentsData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.MergeGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }
        /// <summary>
        /// Test MergeAndReset against oracle
        /// </summary>
        /// <param name="testObj"></param>
        internal void DoMergeAndResetGeomTest(LRSDataSet.MergeAndResetGeometrySegmentsData testObj)
        {
            var errorInfo = string.Empty;
            var query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.MergeAndResetGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            // first execute to store the result in temp table.
            ExecuteNonQuery(query1, ref errorInfo);

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetOneResultFromTempTable);
                var result = ExecuteScalar<BaseDataSet.OralceTwoResult>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result.Output1;
            }
            testObj.OracleError = errorInfo;

        }
        /// <summary>
        /// Test ClipGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoClipGeometrySegment(LRSDataSet.ClipGeometrySegmentData testObj)
        {
            var inputGeom = testObj.InputGeom.GetGeom();
            var query = inputGeom.IsPoint()
                ? string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ClipGeomSegmentPointQuery, GetOracleOrdinatePoint(inputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Tolerance)
                : string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ClipGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test ConvertedLrs Geom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoConvertToLrsGeom(LRSDataSet.ConvertToLrsGeomData testObj)
        {
            var inputGeom = testObj.InputGeom.GetGeom();
            var optionBuilder = new StringBuilder();

            if (testObj.StartMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.StartMeasure);

            if (testObj.EndMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.EndMeasure);

            var errorInfo = string.Empty;
            string query1;
            if (inputGeom.CheckGeomPoint())
            {
                var pointInOracle =
                    $"{inputGeom.STX}, {inputGeom.STY}"; //Two dimensional line string 
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetConvertToLrsGeomPoint, pointInOracle, optionBuilder.ToString());
            }
            else
            {
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetConvertToLrsGeom, testObj.InputGeom, optionBuilder.ToString());
            }

            // Here oracle query execution  happens for specific function
            if (string.IsNullOrEmpty(errorInfo))
            {
                var result = ExecuteScalar<string>(query1, out  errorInfo);
                testObj.OracleQuery = query1;
                testObj.OracleResult1 = result;
            }
            testObj.OracleError = errorInfo;
        }

        /// <summary>
        /// Test GetEndMeasure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoGetEndMeasure(LRSDataSet.GetEndMeasureData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetEndMeasureQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test GetStartMeasure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoGetStartMeasure(LRSDataSet.GetStartMeasureData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetStartMeasureQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test Is Spatially Connected Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoIsConnectedGeomSegmentTest(LRSDataSet.IsConnectedData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetIsConnectedGeomSegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom1), ConvertTo3DCoordinates(testObj.InputGeom2), testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }
        
        /// <summary>
        /// Test LocatePoint Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoLocatePointAlongGeomTest(LRSDataSet.LocatePointAlongGeomData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetLocatePointAlongGeomQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.Measure);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test OffsetGeom Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoOffsetGeometrySegment(LRSDataSet.OffsetGeometrySegmentData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.OffsetGeometryQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.StartMeasure, testObj.EndMeasure, testObj.Offset, testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test Populate Measure Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoPopulateMeasuresTest(LRSDataSet.PopulateGeometryMeasuresData testObj)
        {
            var inputGeom = testObj.InputGeom.GetGeom();
            var optionBuilder = new StringBuilder();

            if (testObj.StartMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.StartMeasure);

            if (testObj.EndMeasure != null)
                optionBuilder.AppendFormat(CultureInfo.CurrentCulture, ", {0}", testObj.EndMeasure);

            var errorInfo = string.Empty;
            string query1;
            if (inputGeom.CheckGeomPoint())
            {
                var pointInOracle =
                    $"{inputGeom.STX}, {inputGeom.STY}, {(inputGeom.HasM ? inputGeom.M.Value : inputGeom.Z.Value)}";
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetPopulateMeasurePoint, pointInOracle, optionBuilder.ToString());
                ExecuteNonQuery(query1, ref errorInfo);
            }
            else
            {
                query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetPopulateMeasureNonQuery, ConvertTo3DCoordinates(testObj.InputGeom), optionBuilder.ToString());
                // first execute to stores the result in temp table.
                ExecuteNonQuery(query1, ref errorInfo);
            }

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetOneResultFromTempTable);
                var result = ExecuteScalar<string>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result;
            }
            testObj.OracleError = errorInfo;
        }

        /// <summary>
        /// Test Reverse Linear Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoReverseLinearGeomTest(LRSDataSet.ReverseLinearGeometryData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetReverseLinearGeomQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }


        /// <summary>
        /// Test Split Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoSplitGeometrySegmentTest(LRSDataSet.SplitGeometrySegmentData testObj)
        {
            var errorInfo = string.Empty;
            var query1 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetSplitGeometrySegmentQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.Measure);
            // first execute to store the result in temp table.
            ExecuteNonQuery(query1, ref errorInfo);

            // retrieve the result from temp table.
            // if there is an error in the previous query; don't run the result from temp table.
            if (string.IsNullOrEmpty(errorInfo))
            {
                var query2 = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetTwoResultFromTempTable);
                var result = ExecuteScalar<BaseDataSet.OralceTwoResult>(query2, out errorInfo);
                testObj.OracleQuery = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", query1, query2);
                testObj.OracleResult1 = result.Output1;
                testObj.OracleResult2 = result.Output2;
            }

            testObj.OracleError = errorInfo;
        }


        /// <summary>
        /// Test Validate LRS Geometry Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void ValidateLRSGeometry(LRSDataSet.ValidateLRSGeometryData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.ValidateLRSGeometryQuery, testObj.InputGeom);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        #endregion LRS Test Functions

        #region Util Functions

        /// <summary>
        /// Test PolygonToLine Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoPolygonToLineTest(UtilDataSet.PolygonToLineData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetPolygonToLineQuery, ConvertTo3DCoordinates(testObj.InputGeom));
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test RemoveDuplicate vertices Function against Oracle.
        /// </summary>
        /// <param name="testObj"></param>
        internal void DoRemoveDuplicateVertices(UtilDataSet.RemoveDuplicateVerticesData testObj)
        {
            var query = string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetRemoveDuplicateVerticesQuery, testObj.InputGeom, testObj.Tolerance);
            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        /// <summary>
        /// Test Extract Function against Oracle.
        /// </summary>
        /// <param name="testObj">The test object.</param>
        internal void DoExtractTest(UtilDataSet.ExtractData testObj)
        {
            var query = testObj.InputGeom.IsPoint()
                ? string.Format(CultureInfo.CurrentCulture,
                (testObj.InputGeom.GetDimension() == DimensionalInfo.Dim2D ? OracleLRSQuery.GetExtractPoint2DQuery : OracleLRSQuery.GetExtractPoint3DQuery), 
                GetOracleOrdinatePoint(testObj.InputGeom.GetGeom()), testObj.ElementIndex, testObj.ElementSubIndex)
                : string.Format(CultureInfo.CurrentCulture, OracleLRSQuery.GetExtractQuery, ConvertTo3DCoordinates(testObj.InputGeom), testObj.ElementIndex, testObj.ElementSubIndex);

            var result = ExecuteScalar<string>(query, out var errorInfo);
            testObj.OracleError = errorInfo;
            testObj.OracleQuery = query;
            testObj.OracleResult1 = result;
        }

        #endregion Util Functions

        #region Helper Functions

        /// <summary>
        /// Gets the oracle ordinate point.
        /// </summary>
        /// <param name="inputGeom">The input geom.</param>
        /// <returns></returns>
        internal string GetOracleOrdinatePoint(SqlGeometry inputGeom)
        {
            var thirdDContent = string.Empty;
            var thirdDimension = inputGeom.HasM ? inputGeom.M.Value : inputGeom.HasZ ? inputGeom.Z.Value : 0;

            if (thirdDimension.NotEqualsTo(0))
                thirdDContent = $", {thirdDimension}";

            return $"{inputGeom.STX}, {inputGeom.STY}{thirdDContent}";
        }

        /// <summary>
        /// Converts the input WKT in 4d(x,y,z,m), 3d(x,y,m) 2d(x,y) to P(x,y,m) values.
        /// </summary>
        /// <param name="geomText">Input Geom text</param>
        /// <returns>3D WKT text</returns>
        internal string ConvertTo3DCoordinates(string geomText)
        {
            var inputText = geomText;
            var matches = _dimensionParseRegex.Matches(geomText);

            // evaluate first point if its a 2D point.
            var is2DPoint = false;

            var iterator = 1;
            foreach (Match match in matches)
            {
                var dimContent = _dimensionGroupRegex.Match(match.Value);
                var suffix = dimContent.Groups["suffix"].Value;

                if (iterator ==1)
                {
                    var countMatches = Regex.Matches(dimContent.Value, @"\d+");
                    is2DPoint = countMatches.Count == 2;
                }
                inputText = _dimensionParseRegex.Replace(inputText, $"[{iterator}]{suffix}", 1);
                iterator++;
            }

            iterator = 1;
            foreach (Match match in matches)
            {
                var subRegex = new Regex($"\\[{iterator}\\]");
                inputText = subRegex.Replace(inputText, EvalutateMatch(match, is2DPoint), 1);
                iterator++;
            }

            return inputText;
        }

        /// <summary>
        /// Evaluates the match to find the 3 dimensional replace text for the match.
        /// </summary>
        /// <param name="match">Match</param>
        /// <param name="is2DPoint">Is 2 Dimensional point</param>
        /// <returns></returns>
        public string EvalutateMatch(Match match, bool is2DPoint)
        {
            var dimContent = _dimensionGroupRegex.Match(match.Value.Trim());
            var textToParse = dimContent.Groups["content"].Value;
            if (string.IsNullOrWhiteSpace(textToParse))
                textToParse = match.Value.Trim();

            var dimension = _dimensionSplitRegex.Match(textToParse);
            var x = dimension.Groups["x"];
            var y = dimension.Groups["y"];

            // if 2D point don't check for z and m values.
            if(is2DPoint)
                return $"{x.Value} {y.Value}".Trim();

            var z = dimension.Groups["z"];
            var m = dimension.Groups["m"];

            z = z != null
                ? string.IsNullOrWhiteSpace(z.Value)
                  || z.Value.ToLower(CultureInfo.CurrentCulture).Trim()
                      .Equals("null", StringComparison.CurrentCulture) ? null : z
                : null;

            m = m != null
                ? string.IsNullOrWhiteSpace(m.Value)
                  || m.Value.ToLower(CultureInfo.CurrentCulture).Trim()
                      .Equals("null", StringComparison.CurrentCulture) ? null : m
                : null;

            var thirdDim = m == null ? (z != null ? z.Value : "0") : m.Value;

            return $"{x.Value} {y.Value} {thirdDim}".Trim();
        }

        #endregion Helper Functions
    }
}
