//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.UnitTests.Extension;

namespace SQLSpatialTools.UnitTests.DDD
{
    [TestClass]
    public class OracleConnectorTest : BaseUnitTest
    {
        [TestMethod]
        public void ConvertTo3DCoordinatesTest()
        {
            var obj = OracleConnector.GetInstance();
            var segment = "MULTILINE ((1 1 1, 2 2 2), (1 1 1, 2 2 2))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "MULTILINE ((1 1 1 5, 2 2 2 10), (1 1 1 15, 2 2 2 20))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "MULTILINE ((1 1 NULL 5, 2 2 2 10), (1 1 NULL 15, 2 2 NULL 20))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1 1, 2 2 2, 3 3 3, 4 4 4)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1 1 NULL, 2 2 2, 3 3 3, 4 4 NULL NULL)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "LINESTRING (1 1, 2 2, 3 3 NULL 3, 4 4 10 NULL)";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "Polygon ((1 1, 2 2, 3 3, 1 1))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "CURVEPOLYGON(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3),CIRCULARSTRING(11 13, 13 15, 14 17, 17 13, 11 13))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "CURVEPOLYGON(COMPOUNDCURVE(CIRCULARSTRING(0 4, 4 0, 8 4), (8 4, 0 4)), COMPOUNDCURVE(CIRCULARSTRING(10 14, 14 10, 18 14), (18 14, 10 14)))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "GEOMETRYCOLLECTION(LINESTRING(1 1, 3 5),POLYGON((-1 -1, -1 -5, -5 -5, -5 -1, -1 -1)))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));

            segment = "CURVEPOLYGON((0 0, 0 0 , 0 0 , 0 0 ), (0 0, 0 0 , 0 0 , 0 0 ))";
            Logger.LogLine("Input    : {0}", segment);
            Logger.Log("Output   : {0}", obj.ConvertTo3DCoordinates(segment));
        }

        [TestMethod]
        public void TrimDecimalPointsTest()
        {
            var input = "1335.0 45)";
            var expected = "1335 45)";
            var result= input.TrimDecimalPoints();
            Assert.AreEqual(expected, result);
        }

    }
}
