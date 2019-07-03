//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.UnitTests.Extension;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public abstract class BaseDDDFunctionTest
    {
        internal static SqlCeConnection DBConnectionObj;
        internal static OracleConnector OracleConnectorObj;
        protected static TestLogger Logger;
        protected static Stopwatch MSSQLTimer, OracleTimer;

        private static DataManipulator _dataManipulator;
        private static bool _onStart = true;
        private static string _connectionString;

        private const string DatabaseFile = "SpatialTestData.sdf";
        private const string SchemaFile = @"TestData\CreateDBSchema.sql";

        public int PassCount, FailCount;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Logger = new TestLogger(TestContext);
            if (_onStart)
            {
                if (TestContext.TestName != "OverallResultTest")
                {
                    if (File.Exists(DatabaseFile))
                        File.Delete(DatabaseFile);
                }

                _connectionString = string.Format(CultureInfo.CurrentCulture, "Data Source=|DataDirectory|\\{0}", DatabaseFile);
                DBConnectionObj = new SqlCeConnection(_connectionString);

                if (TestContext.TestName != "OverallResultTest")
                {
                    _dataManipulator = new DataManipulator(_connectionString, SchemaFile, DBConnectionObj, Logger);
                    _dataManipulator.CreateDB();
                    DBConnectionObj.Open();
                    _dataManipulator.LoadDataSet();
                }
                OracleConnectorObj = OracleConnector.GetInstance();
                _onStart = false;
            }
            else
            {
                DBConnectionObj.Open();
            }

            MSSQLTimer = new Stopwatch();
            OracleTimer = new Stopwatch();
        }

        [TestCleanup]
        public void Cleanup()
        {
            DBConnectionObj?.Close();
        }

        /// <summary>
        /// Update test results specific to SQL Server alone
        /// </summary>
        /// <param name="test"></param>
        /// <param name="tableName"></param>
        /// <param name="count"></param>
        internal void UpdateSqlServerTestResults(BaseDataSet test, string tableName, int count)
        {
            test.SetElapsedTime(MSSQLTimer.Elapsed);

            test.SqlObtainedResult1 = test.SqlObtainedResult1.TrimNullValue();
            test.ExpectedResult1 = test.ExpectedResult1.TrimNullValue();

            test.Result = test.SqlObtainedResult1.Compare(test.ExpectedResult1).GetResult();
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlObtainedResult1), test.SqlObtainedResult1));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Result), test.Result));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlElapsedTime), test.SqlElapsedTime));
            if (!string.IsNullOrWhiteSpace(test.SqlError))
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlError), test.SqlError));

            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExecutionTime), test.ExecutionTime));

            if (test.Result.Equals("Passed", StringComparison.CurrentCulture))
                PassCount++;
            else
                FailCount++;

            if (count == 1)
                _dataManipulator.ExecuteQuery(test.InsertOverallStatusQuery(tableName));
            _dataManipulator.ExecuteQuery(test.UpdateOverallStatusCountQuery(tableName, count, PassCount, FailCount));
        }

        /// <summary>
        /// Update Test Results.
        /// </summary>
        /// <param name="test">Test Obj.</param>
        /// <param name="tableName">Table name.</param>
        /// <param name="count"></param>
        /// <param name="isMultiResult">Is result singular</param>
        internal void UpdateTestResults(BaseDataSet test, string tableName, int count, bool isMultiResult = false)
        {
            test.SetElapsedTime(MSSQLTimer.Elapsed);
            test.SetOracleElapsedTime(OracleTimer.Elapsed);

            test.SqlObtainedResult1 = test.SqlObtainedResult1?.TrimNullValue();
            test.ExpectedResult1 = test.ExpectedResult1?.TrimNullValue();
            test.OracleResult1 = test.OracleResult1?.TrimDecimalPoints()?.TrimNullValue();

            test.OutputComparison1 = test.SqlObtainedResult1.Compare(test.OracleResult1);
            test.Result = test.SqlObtainedResult1.Compare(test.ExpectedResult1).GetResult();

            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlObtainedResult1), test.SqlObtainedResult1));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExpectedResult1), test.ExpectedResult1));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleResult1), test.OracleResult1));

            if (isMultiResult)
            {
                test.SqlObtainedResult2 = test.SqlObtainedResult2?.TrimNullValue();
                test.ExpectedResult2 = test.ExpectedResult2?.TrimNullValue();
                test.OracleResult2 = test.OracleResult2?.TrimDecimalPoints()?.TrimNullValue();

                test.OutputComparison2 = test.SqlObtainedResult2.Compare(test.OracleResult2);
                test.Result = (test.Result.Equals("Passed", StringComparison.CurrentCulture) && test.SqlObtainedResult2.Compare(test.ExpectedResult2)).GetResult();

                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlObtainedResult2), test.SqlObtainedResult2));
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExpectedResult2), test.ExpectedResult2));
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleResult2), test.OracleResult2));
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OutputComparison2), test.OutputComparison2));
            }

            // comparison of result with expected against obtained results from MSSQL OSS extension functions
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.Result), test.Result));
            // comparison of obtained results from MSSQL OSS extension functions against results from competitive Oracle functions.
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OutputComparison1), test.OutputComparison1));

            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlElapsedTime), test.SqlElapsedTime));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleElapsedTime), test.OracleElapsedTime));
            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleQuery), test.OracleQuery.EscapeQueryString()));

            if (!string.IsNullOrWhiteSpace(test.SqlError))
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.SqlError), test.SqlError));

            if (!string.IsNullOrWhiteSpace(test.OracleError))
                _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.OracleError), test.OracleError));

            _dataManipulator.ExecuteQuery(test.GetTargetUpdateQuery(tableName, nameof(test.ExecutionTime), test.ExecutionTime));

            //update test results
            if (isMultiResult)
            {
                if (test.OutputComparison1 && test.OutputComparison2)
                    PassCount++;
                else
                    FailCount++;
            }
            else
            {
                if (test.OutputComparison1)
                    PassCount++;
                else
                    FailCount++;
            }

            if (count == 1)
                _dataManipulator.ExecuteQuery(test.InsertOverallStatusQuery(tableName));
            _dataManipulator.ExecuteQuery(test.UpdateOverallStatusCountQuery(tableName, count, PassCount, FailCount));
        }
    }
}
