//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLSpatialTools.UnitTests.Extension
{
    public class BaseUnitTest
    {
        protected TestLogger Logger;

        [TestInitialize]
        public void Initialize()
        {
            Logger = new TestLogger(TestContext);
        }

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
    }
}