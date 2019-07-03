//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace SQLSpatialTools.UnitTests.DDD
{
    public class BaseDataSet
    {
        public const string UpdateTargetQuery = "UPDATE [{0}] Set [{1}] = {2} WHERE [ID] = {3};";

        public string ExpectedResult1 { get; set; }
        public string ExpectedResult2 { get; set; }
        public string SqlObtainedResult1 { get; set; }
        public string SqlObtainedResult2 { get; set; }

        public int Id { get; set; }
        public string Result { get; set; }
        public string SqlError { get; set; }
        public string SqlElapsedTime { get; private set; }
        public string OracleResult1 { get; set; }
        public bool OutputComparison1 { get; set; }
        public bool OutputComparison2 { get; set; }
        public string OracleResult2 { get; set; }
        public string OracleElapsedTime { get; private set; }
        public string OracleError { get; set; }

        public string ExecutionTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        public string OracleQuery { get; set; }
        internal void SetElapsedTime(TimeSpan elapsed)
        {
            SqlElapsedTime = elapsed.ToString();
        }

        internal void SetOracleElapsedTime(TimeSpan elapsed)
        {
            OracleElapsedTime = elapsed.ToString();
        }

        internal string GetTargetUpdateQuery(string tableName, string fieldName, object fieldValue)
        {
            return string.Format(CultureInfo.CurrentCulture, UpdateTargetQuery, tableName, fieldName, GetFieldValue(fieldValue), Id);
        }

        internal string UpdateOverallStatusCountQuery(string tableName, int count, int passCount, int failCount)
        {
            return string.Format(CultureInfo.CurrentCulture, OverallResult.UpdateQuery, count, passCount, failCount, tableName);
        }

        internal string InsertOverallStatusQuery(string tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, OverallResult.InsertQuery, tableName);
        }

        private static string GetFieldValue(object fieldValue)
        {
            if (fieldValue == null)
                return $"N''";
            var type = fieldValue.GetType();

            if (type == typeof(int) || type == typeof(float) || type == typeof(double))
                return fieldValue.ToString();
            else if (type == typeof(bool))
                return (bool)fieldValue ? "1" : "0";
            return $"N'{fieldValue}'";
        }

        public class OverallResult
        {
            public const string TableName = "_LRS_OverallResult";
            public static readonly string SelectQuery =
                $"SELECT [Id], [FunctionName], [TotalCount], [PassCount], [FailCount] FROM [{TableName}];";
            private static readonly string InsertPart1 = $"INSERT INTO[{TableName}] ([FunctionName]) ";
            private const string InsertPart2 = "VALUES (N'{0}');";
            public static readonly string InsertQuery = string.Concat(InsertPart1, InsertPart2);

            private static readonly string UpdatePart1 = $"UPDATE [{TableName}] SET ";
            private const string UpdatePart2 = "[TotalCount] = {0}, [PassCount] = {1}, [FailCount] = {2} WHERE [FunctionName] = '{3}';";
            public static readonly string UpdateQuery = string.Concat(UpdatePart1, UpdatePart2);

            public string FunctionName { get; set; }
            public int TotalCount { get; set; }
            public int PassCount { get; set; }
            public int FailCount { get; set; }
        }

        public class OralceTwoResult
        {
            public string Output1 { get; set; }
            public string Output2 { get; set; }
        }

        public class OralceOneResult
        {
            public string Output1 { get; set; }
        }
    }   
}