-- LRS Functions Test Data Table

-- Overall Results Table
CREATE TABLE [_LRS_OverallResult] (
  [Id] int IDENTITY (1,1) NOT NULL
, [FunctionName] nvarchar(1000) NOT NULL
, [TotalCount] int
, [PassCount] int
, [FailCount] int
);
GO
ALTER TABLE [_LRS_OverallResult] ADD CONSTRAINT [PK_LRS_OverallResult] PRIMARY KEY ([Id]);
GO

-- ClipGeometrySegment
CREATE TABLE [LRS_ClipGeometrySegmentData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [StartMeasure] float NOT NULL
, [EndMeasure] float NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ClipGeometrySegmentData] ADD CONSTRAINT [PK_ClipGeometrySegmentData] PRIMARY KEY ([Id]);
GO

-- ConvertToLrsGeom
CREATE TABLE [LRS_ConvertToLrsGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [StartMeasure] float
, [EndMeasure] float
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(1000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ConvertToLrsGeomData] ADD CONSTRAINT [PK_ConvertToLrsGeomData] PRIMARY KEY ([Id]);
GO

-- Get End Measure
CREATE TABLE [LRS_GetEndMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_GetEndMeasureData] ADD CONSTRAINT [PK_GetEndMeasureData] PRIMARY KEY ([Id]);
GO


-- GetStartMeasure
CREATE TABLE [LRS_GetStartMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [ExecutionTime] datetime
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_GetStartMeasureData] ADD CONSTRAINT [PK_GetStartMeasureData] PRIMARY KEY ([Id]);
GO


-- InterpolateBetweenGeom
CREATE TABLE [LRS_InterpolateBetweenGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Measure] float NOT NULL
, [SqlObtainedResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [SqlElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_InterpolateBetweenGeomData] ADD CONSTRAINT [PK_InterpolateBetweenGeomData] PRIMARY KEY ([Id]);
GO


-- IsConnected
CREATE TABLE [LRS_IsConnectedData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] bit
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] bit NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_IsConnectedData] ADD CONSTRAINT [PK_IsConnectedData] PRIMARY KEY ([Id]);
GO


-- LocatePointAlongGeom
CREATE TABLE [LRS_LocatePointAlongGeomData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [Measure] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_LocatePointAlongGeomData] ADD CONSTRAINT [PK_LocatePointAlongGeomData] PRIMARY KEY ([Id]);
GO


-- MergeGeometrySegments
CREATE TABLE [LRS_MergeGeometrySegmentsData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_MergeGeometrySegmentsData] ADD CONSTRAINT [PK_MergeGeometrySegmentsData] PRIMARY KEY ([Id]);
GO

-- MergeGeometrySegments
CREATE TABLE [LRS_MergeAndResetGeometrySegmentsData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom1] nvarchar(1000) NOT NULL
, [InputGeom2] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_MergeAndResetGeometrySegmentsData] ADD CONSTRAINT [PK_MergeAndResetGeometrySegmentsData] PRIMARY KEY ([Id]);
GO


-- OffsetGeometrySegment
CREATE TABLE [LRS_OffsetGeometrySegmentData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [StartMeasure] float NOT NULL
, [EndMeasure] float NOT NULL
, [Offset] float NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_OffsetGeometrySegmentData] ADD CONSTRAINT [PK_OffsetGeometrySegmentData] PRIMARY KEY ([Id]);
GO


-- PopulateGeometryMeasures
CREATE TABLE [LRS_PopulateGeometryMeasuresData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [StartMeasure] float
, [EndMeasure] float
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_PopulateGeometryMeasuresData] ADD CONSTRAINT [PK_PopulateGeometryMeasuresData] PRIMARY KEY ([Id]);
GO

-- ResetMeasureTest
CREATE TABLE [LRS_ResetMeasureData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [SqlObtainedResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [SqlElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ResetMeasureData] ADD CONSTRAINT [PK_ResetMeasureData] PRIMARY KEY ([Id]);
GO


-- ReverseLinearGeometry
CREATE TABLE [LRS_ReverseLinearGeometryData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ReverseLinearGeometryData] ADD CONSTRAINT [PK_ReverseLinearGeometryData] PRIMARY KEY ([Id]);
GO


-- SplitGeometrySegment
CREATE TABLE [LRS_SplitGeometrySegmentData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [Measure] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OutputComparison2] bit
, [SqlObtainedResult2] nvarchar(4000)
, [OracleResult2] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [ExpectedResult2] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_SplitGeometrySegmentData] ADD CONSTRAINT [PK_SplitGeometrySegmentData] PRIMARY KEY ([Id]);
GO

-- Validate LRS Geometry
CREATE TABLE [LRS_ValidateLRSGeometryData] (
  [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [SqlError] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [LRS_ValidateLRSGeometryData] ADD CONSTRAINT [PK_ValidateLRSGeometryData] PRIMARY KEY ([Id]);
GO

-- PolygonToLine
CREATE TABLE [Util_PolygonToLineData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [Util_PolygonToLineData] ADD CONSTRAINT [PK_PolygonToLineData] PRIMARY KEY ([Id]);
GO

-- Extract
CREATE TABLE [Util_ExtractData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(4000) NOT NULL
, [ElementIndex] integer NOT NULL
, [ElementSubIndex] integer NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(4000)
, [OracleResult1] nvarchar(4000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(4000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(4000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [Util_ExtractData] ADD CONSTRAINT [PK_ExtractData] PRIMARY KEY ([Id]);
GO

-- RemoveDuplicateVertices
CREATE TABLE [Util_RemoveDuplicateVerticesData] (
 [Id] int IDENTITY (1,1) NOT NULL
, [InputGeom] nvarchar(1000) NOT NULL
, [Tolerance] float NOT NULL
, [OutputComparison1] bit
, [SqlObtainedResult1] nvarchar(1000)
, [OracleResult1] nvarchar(1000)
, [OracleError] nvarchar(1000)
, [SqlError] nvarchar(1000)
, [OracleQuery] nvarchar(1000)
, [SqlElapsedTime] nvarchar(100)
, [OracleElapsedTime] nvarchar(100)
, [ExecutionTime] datetime
, [ExpectedResult1] nvarchar(1000) NOT NULL
, [Result] nvarchar(50)
, [Comments] nvarchar(1000)
);
GO
ALTER TABLE [Util_RemoveDuplicateVerticesData] ADD CONSTRAINT [PK_RemoveDuplicateVerticesData] PRIMARY KEY ([Id]);