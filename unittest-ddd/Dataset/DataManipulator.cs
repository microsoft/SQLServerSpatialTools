//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using SQLSpatialTools.UnitTests.Extension;

namespace SQLSpatialTools.UnitTests.DDD
{
    internal class DataManipulator
    {
        private const string DataFileComment = "##";
        private readonly string _connectionString;
        private readonly string[] _dataParamSeparator = {"||"};
        private readonly SqlCeConnection _dbConnection;
        private readonly TestLogger _logger;
        private readonly string _schemaFile;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataManipulator" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="schemaFile">The schema file.</param>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="logger">The logger.</param>
        public DataManipulator(string connectionString, string schemaFile, SqlCeConnection dbConnection,
            TestLogger logger)
        {
            _connectionString = connectionString;
            _schemaFile = schemaFile;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        /// <summary>
        ///     Creates the database.
        /// </summary>
        public void CreateDB()
        {
            var sqlCeEngine = new SqlCeEngine(_connectionString);
            sqlCeEngine.CreateDatabase();
        }

        /// <summary>
        ///     Loads the data set.
        /// </summary>
        public void LoadDataSet()
        {
            if (!CreateSchema())
                return;

            ExecuteQuery(ParseDataSet(LRSDataSet.ClipGeometrySegmentData.DataFile,
                LRSDataSet.ClipGeometrySegmentData.ParamCount, LRSDataSet.ClipGeometrySegmentData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ConvertToLrsGeomData.DataFile,
                LRSDataSet.ConvertToLrsGeomData.ParamCount,
                LRSDataSet.ConvertToLrsGeomData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.GetEndMeasureData.DataFile, LRSDataSet.GetEndMeasureData.ParamCount,
                LRSDataSet.GetEndMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.GetStartMeasureData.DataFile,
                LRSDataSet.GetStartMeasureData.ParamCount, LRSDataSet.GetStartMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.InterpolateBetweenGeomData.DataFile,
                LRSDataSet.InterpolateBetweenGeomData.ParamCount, LRSDataSet.InterpolateBetweenGeomData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.IsConnectedData.DataFile, LRSDataSet.IsConnectedData.ParamCount,
                LRSDataSet.IsConnectedData.InsertQuery));

            ExecuteQuery(ParseDataSet(LRSDataSet.LocatePointAlongGeomData.DataFile,
                LRSDataSet.LocatePointAlongGeomData.ParamCount, LRSDataSet.LocatePointAlongGeomData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.MergeGeometrySegmentsData.DataFile,
                LRSDataSet.MergeGeometrySegmentsData.ParamCount, LRSDataSet.MergeGeometrySegmentsData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.MergeAndResetGeometrySegmentsData.DataFile,
                LRSDataSet.MergeAndResetGeometrySegmentsData.ParamCount,
                LRSDataSet.MergeAndResetGeometrySegmentsData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.PopulateGeometryMeasuresData.DataFile,
                LRSDataSet.PopulateGeometryMeasuresData.ParamCount,
                LRSDataSet.PopulateGeometryMeasuresData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ResetMeasureData.DataFile, LRSDataSet.ResetMeasureData.ParamCount,
                LRSDataSet.ResetMeasureData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ReverseLinearGeometryData.DataFile,
                LRSDataSet.ReverseLinearGeometryData.ParamCount, LRSDataSet.ReverseLinearGeometryData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.SplitGeometrySegmentData.DataFile,
                LRSDataSet.SplitGeometrySegmentData.ParamCount, LRSDataSet.SplitGeometrySegmentData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.ValidateLRSGeometryData.DataFile,
                LRSDataSet.ValidateLRSGeometryData.ParamCount, LRSDataSet.ValidateLRSGeometryData.InsertQuery));
            ExecuteQuery(ParseDataSet(LRSDataSet.OffsetGeometrySegmentData.DataFile,
                LRSDataSet.OffsetGeometrySegmentData.ParamCount, LRSDataSet.OffsetGeometrySegmentData.InsertQuery));

            // Utility functions data set
            ExecuteQuery(ParseDataSet(UtilDataSet.PolygonToLineData.DataFile,
                UtilDataSet.PolygonToLineData.ParamCount, UtilDataSet.PolygonToLineData.InsertQuery));
            ExecuteQuery(ParseDataSet(UtilDataSet.ExtractData.DataFile,
               UtilDataSet.ExtractData.ParamCount, UtilDataSet.ExtractData.InsertQuery));
            ExecuteQuery(ParseDataSet(UtilDataSet.RemoveDuplicateVerticesData.DataFile,
               UtilDataSet.RemoveDuplicateVerticesData.ParamCount, UtilDataSet.RemoveDuplicateVerticesData.InsertQuery));
        }

        /// <summary>
        ///     Creates the schema.
        /// </summary>
        /// <returns></returns>
        private bool CreateSchema()
        {
            if (!File.Exists(_schemaFile)) return false;
            var splitQueries = File.ReadAllText(_schemaFile).Trim()
                .Split(new[] {"GO"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var query in splitQueries)
                if (!string.IsNullOrWhiteSpace(query))
                    ExecuteQuery(query);
            return true;
        }

        /// <summary>
        ///     Parses the data set.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="paramCount">The parameter count.</param>
        /// <param name="queryFormat">The query format.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<string> ParseDataSet(string fileName, int paramCount, string queryFormat)
        {
            var queryList = new List<string>();
            try
            {
                if (!File.Exists(fileName))
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, "Data file :{0} not exists.",
                        fileName));

                var dataSet = File.ReadLines(fileName);
                var lineCounter = 0;
                foreach (var line in dataSet)
                {
                    ++lineCounter;
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    if (line.TrimStart().StartsWith(DataFileComment))
                        continue;

                    var subDataSets = line.Split(_dataParamSeparator, StringSplitOptions.None);
                    if (subDataSets.Length != paramCount)
                    {
                        _logger.LogError(new Exception("Data Format Exception"),
                            "Error in input data format:{0};\nLine:{1} Argument count mismatch, expected: {2}, obtained: {3}",
                            fileName, lineCounter, paramCount, subDataSets.Length);
                        continue;
                    }

                    var queryContent = queryFormat;
                    for (var param = 0; param < paramCount; param++)
                    {
                        var paramValue = subDataSets[param].Trim();
                        var dataContent = string.IsNullOrWhiteSpace(paramValue) ? "NULL" : paramValue;

                        queryContent = queryContent.Replace(string.Format(CultureInfo.CurrentCulture, "[{0}]", param),
                            dataContent);
                    }

                    queryList.Add(queryContent);
                }

                return queryList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in parsing data content for {0}", fileName);
            }

            return queryList;
        }

        /// <summary>
        ///     Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        public void ExecuteQuery(string commandText)
        {
            try
            {
                var sqlCommand = new SqlCeCommand(commandText, _dbConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in query execution; Query: {0}", commandText);
            }
        }

        /// <summary>
        ///     Executes the query.
        /// </summary>
        /// <param name="commandTexts">The command texts.</param>
        public void ExecuteQuery(List<string> commandTexts)
        {
            foreach (var query in commandTexts) ExecuteQuery(query);
        }
    }
}