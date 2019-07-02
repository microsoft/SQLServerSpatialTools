//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.UnitTests.DDD
{
    public class LRSDataSet
    {
        public class ClipGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 6;
            public const string TableName = "LRS_ClipGeometrySegmentData";
            public const string DataFile = @"TestData\LRS\ClipGeometrySegment.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [Tolerance], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [StartMeasure], [EndMeasure], [Tolerance], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], [3], N'[4]' ,N'[5]');";

            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public double Tolerance { get; set; }
        }

        public class ConvertToLrsGeomData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_ConvertToLrsGeomData";
            public const string DataFile = @"TestData\LRS\ConvertToLrsGeomData.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], N'[3]',N'[4]');";

            public string InputGeom { get; set; }
            public double? StartMeasure { get; set; }
            public double? EndMeasure { get; set; }
        }

        public class GetEndMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_GetEndMeasureData";
            public const string DataFile = @"TestData\LRS\GetEndMeasure.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], N'[2]');";

            public string InputGeom { get; set; }
        }

        public class GetStartMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_GetStartMeasureData";
            public const string DataFile = @"TestData\LRS\GetStartMeasure.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], N'[2]');";

            public string InputGeom { get; set; }
        }

        public class InterpolateBetweenGeomData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_InterpolateBetweenGeomData";
            public const string DataFile = @"TestData\LRS\InterpolateBetweenGeom.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom1], [InputGeom2], [Measure], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom1], [InputGeom2], [Measure], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', [2], N'[3]', N'[4]');";

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Measure { get; set; }
        }

        public class IsConnectedData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_IsConnectedData";
            public const string DataFile = @"TestData\LRS\IsConnected.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom1], [InputGeom2], [Tolerance], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom1], [InputGeom2], [Tolerance], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', [2], [3], N'[4]');";

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
        }

        public class LocatePointAlongGeomData : BaseDataSet
        {
            public const short ParamCount = 4;
            public const string TableName = "LRS_LocatePointAlongGeomData";
            public const string DataFile = @"TestData\LRS\LocatePointAlongGeom.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [Measure], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [Measure], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', N'[3]');";

            public string InputGeom { get; set; }
            public double Measure { get; set; }
        }

        public class MergeGeometrySegmentsData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_MergeGeometrySegmentsData";
            public const string DataFile = @"TestData\LRS\MergeGeometrySegments.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', [3], N'[4]');";

            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
        }

        public class MergeAndResetGeometrySegmentsData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_MergeAndResetGeometrySegmentsData";
            public const string DataFile = @"TestData\LRS\MergeAndResetGeometrySegments.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom1], [InputGeom2], [ExpectedResult1], [Tolerance], [Comments]) VALUES (N'[0]', N'[1]', N'[2]', [3], N'[4]');";
            public string InputGeom1 { get; set; }
            public string InputGeom2 { get; set; }
            public double Tolerance { get; set; }
        }

        public class OffsetGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 7;
            public const string TableName = "LRS_OffsetGeometrySegmentData";
            public const string DataFile = @"TestData\LRS\OffsetGeometrySegment.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [Offset], [Tolerance], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [StartMeasure], [EndMeasure], [Offset], [Tolerance], [ExpectedResult1],[Comments]) VALUES (N'[0]', [1], [2], [3], [4], N'[5]', N'[6]');";

            public string InputGeom { get; set; }
            public double StartMeasure { get; set; }
            public double EndMeasure { get; set; }
            public double Offset { get; set; }
            public double Tolerance { get; set; }
        }

        public class PopulateGeometryMeasuresData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_PopulateGeometryMeasuresData";
            public const string DataFile = @"TestData\LRS\PopulateGeometryMeasures.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [StartMeasure], [EndMeasure], [ExpectedResult1], [Comments]) VALUES (N'[0]', [1], [2], N'[3]',N'[4]');";

            public string InputGeom { get; set; }
            public double? StartMeasure { get; set; }
            public double? EndMeasure { get; set; }
        }

        public class ResetMeasureData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ResetMeasureData";
            public const string DataFile = @"TestData\LRS\ResetMeasure.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');";

            public string InputGeom { get; set; }
        }


        public class ReverseLinearGeometryData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ReverseLinearGeometryData";
            public const string DataFile = @"TestData\LRS\ReverseLinearGeometry.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');";

            public string InputGeom { get; set; }
        }

        public class SplitGeometrySegmentData : BaseDataSet
        {
            public const short ParamCount = 5;
            public const string TableName = "LRS_SplitGeometrySegmentData";
            public const string DataFile = @"TestData\LRS\SplitGeometrySegment.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [Measure], [ExpectedResult1], [ExpectedResult2], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [Measure], [ExpectedResult1], [ExpectedResult2], [Comments]) VALUES (N'[0]', [1], N'[2]', N'[3]', N'[4]');";

            public string InputGeom { get; set; }
            public double Measure { get; set; }
        }

        public class ValidateLRSGeometryData : BaseDataSet
        {
            public const short ParamCount = 3;
            public const string TableName = "LRS_ValidateLRSGeometryData";
            public const string DataFile = @"TestData\LRS\ValidateLRSGeometryTest.data";
            public static readonly string SelectQuery =
                $"SELECT [Id], [InputGeom], [ExpectedResult1], [Comments] FROM [{TableName}];";
            public static readonly string InsertQuery =
                $"INSERT INTO [{TableName}] ([InputGeom], [ExpectedResult1], [Comments]) VALUES (N'[0]', N'[1]', N'[2]');";

            public string InputGeom { get; set; }
        }
    }
}
