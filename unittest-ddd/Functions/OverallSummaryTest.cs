//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class OverallSummaryTest : BaseDDDFunctionTest
    {
        [TestMethod]
        public void OverallResultTest()
        {
            Logger.LogLine("Overall Result Tests");
            Logger.LogLine("This should be run in sequence after all the tests as MS test doesn't support test priority order.");
            var dataSet = DBConnectionObj.Query<BaseDataSet.OverallResult>(BaseDataSet.OverallResult.SelectQuery);
            var testIterator = 1;

            var failedCases = new List<string>();

            if(!dataSet.Any())
            {
                Logger.LogLine("This shouldn't be run separately. Should be executed in parallel with other tests. Please ensure other tests are run before this.");
            }

            foreach (var test in dataSet)
            {
                Logger.LogLine("{0}. Function : {1}", testIterator, test.FunctionName);
                Logger.Log("Total : {0}", test.TotalCount);
                Logger.Log("Passed : {0} / {1}", test.PassCount, test.TotalCount);
                Logger.Log("Failed : {0} / {1}", test.FailCount, test.TotalCount);

                if (test.FailCount > 0)
                    failedCases.Add($"In {test.FunctionName.PadRight(35)} : {test.FailCount.ToString().PadLeft(4)} failed out of / {test.TotalCount.ToString().PadLeft(4)}");
                testIterator++;
            }

            if (failedCases.Any())
                throw new Exception($"\n{string.Join("\n", failedCases.ToArray())}");
        }
    }
}
