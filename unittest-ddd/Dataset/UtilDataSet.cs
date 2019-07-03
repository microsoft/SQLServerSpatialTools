//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.UnitTests.DDD
{
    public class UtilDataSet
    {
        public class PolygonToLineData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "Util_PolygonToLineData";
            public const string DataFile = @"TestData\Util\PolygonToLine.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');";

            public string InputGeom { get; set; }
        }

        public class ExtractData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "Util_ExtractData";
            public const string DataFile = @"TestData\Util\Extract.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ElementIndex], [ElementSubIndex], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ElementIndex], [ElementSubIndex], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], N'[3]', N'[4]');";

            public string InputGeom { get; set; }
            public int ElementIndex { get; set; }
            public int ElementSubIndex { get; set; }
        }

        public class RemoveDuplicateVerticesData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "Util_RemoveDuplicateVerticesData";
            public const string DataFile = @"TestData\Util\RemoveDuplicateVertices.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [Tolerance], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [Tolerance], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', N'[3]');";
            public string InputGeom { get; set; }
            public double Tolerance { get; set; }
        }
    }
}
