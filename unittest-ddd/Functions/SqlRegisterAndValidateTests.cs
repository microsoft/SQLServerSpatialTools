//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class SqlRegisterAndValidateTests
    {
        private static string _connectionString;
        private static string _targetDir;
        private static SqlConnection _dbConnection;
        private static Server _dbServer;

        private static string _registerScriptFilePath;
        private static string _unRegisterScriptFilePath;
        private static string _lrsExampleScriptFilePath;
        private static string _utilExampleScriptFilePath;

        private const string RegisterScriptFileName = "Register.sql";
        private const string UnregisterScriptFileName = "Unregister.sql";
        private const string LRSExampleScriptFileName = "lrs_geometry_example.sql";
        private const string UtilExampleScriptFileName = "util_geometry_example.sql";

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            _connectionString = ConfigurationManager.AppSettings.Get("sql_connection");
            _dbConnection = new SqlConnection(_connectionString);
            _dbServer = new Server(new ServerConnection(_dbConnection));

            _targetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_targetDir != null)
            {
                _targetDir = _targetDir.Substring(0, _targetDir.LastIndexOf('\\'));
                _targetDir = Path.Combine(_targetDir, "lib\\SQL Scripts");
                if (!Directory.Exists(_targetDir))
                    throw new Exception("Target Directory not found : " + _targetDir);

                // register script
                _registerScriptFilePath = Path.Combine(_targetDir, RegisterScriptFileName);
                if (!File.Exists(_registerScriptFilePath))
                    throw new Exception("Register Script file not found : " + _registerScriptFilePath);

                // unregister script
                _unRegisterScriptFilePath = Path.Combine(_targetDir, UnregisterScriptFileName);
                if (!File.Exists(_unRegisterScriptFilePath))
                    throw new Exception("Unregister Script file not found : " + _unRegisterScriptFilePath);

                // LRS examples script
                _lrsExampleScriptFilePath = Path.Combine(_targetDir, LRSExampleScriptFileName);
                if (!File.Exists(_lrsExampleScriptFilePath))
                    throw new Exception("LRS examples Script file not found : " + _lrsExampleScriptFilePath);

                // Util examples script
                _utilExampleScriptFilePath = Path.Combine(_targetDir, UtilExampleScriptFileName);
                if (!File.Exists(_utilExampleScriptFilePath))
                    throw new Exception("Util examples Script file not found : " + _utilExampleScriptFilePath);
            }

            // call unregister script as part of initialize
            Unregister();
        }

        private static void Unregister()
        {
            var scriptContent = File.ReadAllText(_unRegisterScriptFilePath);
            _dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [TestMethod]
        [Priority(1)]
        public void UnregisterOSSLibraryTest()
        {
            Unregister();
        }

        [TestMethod]
        [Priority(2)]
        public void RegisterOSSLibraryTest()
        {
            UnregisterOSSLibraryTest();
            var scriptContent = File.ReadAllText(_registerScriptFilePath);
            _dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [TestMethod]
        [Priority(3)]
        public void RunLRSExamplesTest()
        {
            RegisterOSSLibraryTest();
            var scriptContent = File.ReadAllText(_lrsExampleScriptFilePath);
            _dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [TestMethod]
        [Priority(4)]
        public void RunUtilExamplesTest()
        {
            RegisterOSSLibraryTest();
            var scriptContent = File.ReadAllText(_utilExampleScriptFilePath);
            _dbServer.ConnectionContext.ExecuteNonQuery(scriptContent);
        }

        [ClassCleanup()]
        public static void Cleanup()
        {
            _dbConnection?.Close();
        }
    }
}
