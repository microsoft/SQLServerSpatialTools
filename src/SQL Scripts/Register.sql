-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Install the SQLSpatialTools assembly and all its functions into the current database

-- Enabling CLR prior to registering assembly and its related functions.
EXEC sp_configure 'show advanced option', '1'; 
RECONFIGURE;
Go

sp_configure 'clr enabled', 1  ;
GO  
RECONFIGURE  ;
GO

-- SQL Server 2017+: choose a trust option before registering the assembly.
-- These examples are commented out; this script does not change CLR strict security.
-- Run the selected setup separately as a server administrator, then return to
-- the target database to run this registration script.
--
-- OPTION 1 (recommended): keep CLR strict security enabled and establish trust.
-- EXEC sys.sp_configure 'clr strict security', 1;
-- RECONFIGURE;
-- GO
--
-- For a signed DLL, trust its signing key once in master:
-- USE master;
-- GO
-- CREATE ASYMMETRIC KEY [SQLSpatialToolsSigningKey]
-- FROM EXECUTABLE FILE = N'DLLPath';
-- CREATE LOGIN [SQLSpatialToolsSigningLogin]
-- FROM ASYMMETRIC KEY [SQLSpatialToolsSigningKey];
-- GRANT UNSAFE ASSEMBLY TO [SQLSpatialToolsSigningLogin];
-- GO
-- Later builds signed with the same key reuse this trust. This authorizes other
-- assemblies signed with that key too: protect the private key and never trust
-- a shared/development key in production. Do not copy the private key to SQL Server.
-- A signed build with SqlClrGenerateTrustScript=true generates this setup as
-- TrustAssembly.sql. Dependencies require their own applicable trust.
--
-- Alternatively, trust only this exact DLL (signed or unsigned) by SHA-512 hash:
-- USE master;
-- GO
-- DECLARE @assemblyHash varbinary(64);
-- SELECT @assemblyHash = HASHBYTES('SHA2_512', BulkColumn)
-- FROM OPENROWSET(BULK 'DLLPath', SINGLE_BLOB) AS assemblyFile;
-- EXEC sys.sp_add_trusted_assembly @hash = @assemblyHash,
--     @description = N'SQLSpatialTools - administrator-approved build';
-- GO
-- Review the DLL before approving it. Update hash-based trust whenever it changes.
-- In both examples, DLLPath must be accessible to the SQL Server service account.
--
-- OPTION 2 (less secure; isolated development/test servers only):
-- Disable CLR strict security instead of configuring the trust above.
-- WARNING: this weakens protection SERVER-WIDE, not just for SQLSpatialTools.
-- SAFE/EXTERNAL_ACCESS permissions are not a reliable security boundary.
-- Do not use this option in production.
-- EXEC sys.sp_configure 'clr strict security', 0;
-- RECONFIGURE;
-- GO
-- To restore protection, configure assembly trust and set the option back to 1
-- as shown in option 1. Untrusted assemblies may stop working when it is re-enabled.

-- !!! DLL Path will be replace based upon system environment !!!
CREATE assembly SQLSpatialTools
FROM 'DLLPath'
GO

-- Create User Defined SQL Types
CREATE TYPE Projection EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Types.SQL.SqlProjection]
GO

CREATE TYPE AffineTransform EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Types.SQL.AffineTransform]
GO

-- Register the functions...

--#region General Geometry Functions
CREATE FUNCTION FilterArtifactsGeometry (
    @geometry GEOMETRY
    ,@filterEmptyShapes BIT
    ,@filterPoints BIT
    ,@lineStringTolerance FLOAT(53)
    ,@ringTolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].FilterArtifactsGeometry
GO

CREATE FUNCTION GeomFromXYMText (
    @geometry NVARCHAR(MAX)
    ,@targetSrid INT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].GeomFromXYMText
GO

CREATE FUNCTION InterpolateBetweenGeom (
    @startPoint GEOMETRY
    ,@endPoint GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LocateAlongGeom (
    @geometry GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION MakeValidForGeography (@geometry GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].MakeValidForGeography
GO

CREATE FUNCTION ReverseLinestring (@geometry GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ReverseLinestring
GO

CREATE FUNCTION ShiftGeometry (
    @geometry GEOMETRY
    ,@xShift FLOAT
    ,@yShift FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].ShiftGeometry
GO

CREATE FUNCTION VacuousGeometryToGeography (
    @geometryToConvert GEOMETRY
    ,@targetSrid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geometry].VacuousGeometryToGeography
GO

--#endregion

--#region General Geography Functions

CREATE FUNCTION ConvexHullGeographyFromText (
    @inputWKT NVARCHAR(max)
    ,@srid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeographyFromText
GO

CREATE FUNCTION ConvexHullGeography (@geography GEOGRAPHY)
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].ConvexHullGeography
GO

CREATE FUNCTION DensifyGeography (
    @geography GEOGRAPHY
    ,@maxAngle FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].DensifyGeography
GO

CREATE FUNCTION FilterArtifactsGeography (
    @geography GEOGRAPHY
    ,@filterEmptyShapes BIT
    ,@filterPoints BIT
    ,@lineStringTolerance FLOAT(53)
    ,@ringTolerance FLOAT(53)
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].FilterArtifactsGeography
GO

CREATE FUNCTION InterpolateBetweenGeog (
    @startPoint GEOGRAPHY
    ,@endPoint GEOGRAPHY
    ,@distance FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].InterpolateBetweenGeog
GO

CREATE FUNCTION IsValidGeographyFromGeometry (@geometry GEOMETRY)
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromGeometry
GO

CREATE FUNCTION IsValidGeographyFromText (
    @inputWKT NVARCHAR(MAX)
    ,@srid INT
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].IsValidGeographyFromText
GO

CREATE FUNCTION LocateAlongGeog (
    @geography GEOGRAPHY
    ,@distance FLOAT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].LocatePointAlongGeog
GO

CREATE FUNCTION MakeValidGeographyFromGeometry (@geometry GEOMETRY)
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromGeometry
GO

CREATE FUNCTION MakeValidGeographyFromText (
    @inputWKT NVARCHAR(MAX)
    ,@srid INT
    )
RETURNS GEOGRAPHY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].MakeValidGeographyFromText
GO

CREATE FUNCTION VacuousGeographyToGeometry (
    @geographyToConvert GEOGRAPHY
    ,@targetSrid INT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.General.Geography].VacuousGeographyToGeometry
GO

--#endregion

--#region LRS Geometric Functions
CREATE FUNCTION LRS_ClipGeometrySegment (
    @geometry GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    ,@tolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ClipGeometrySegment
GO

CREATE FUNCTION LRS_ConvertToLrsGeom (
    @geometry GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ConvertToLrsGeom
GO

CREATE FUNCTION LRS_GetEndMeasure (@geometry GEOMETRY)
RETURNS FLOAT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetEndMeasure
GO

CREATE FUNCTION LRS_GetMergePosition (
    @geometry1 GEOMETRY
    ,@geometry2 GEOMETRY
    ,@tolerance FLOAT(53)
    )
RETURNS NVARCHAR(20)
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetMergePosition
GO

CREATE FUNCTION LRS_GetStartMeasure (@geometry GEOMETRY)
RETURNS FLOAT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].GetStartMeasure
GO

CREATE FUNCTION LRS_InterpolateBetweenGeom (
    @startPoint GEOMETRY
    ,@endPoint GEOMETRY
    ,@measure FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].InterpolateBetweenGeom
GO

CREATE FUNCTION LRS_IsConnected (
    @geometry1 GEOMETRY
    ,@geometry2 GEOMETRY
    ,@tolerance FLOAT(53)
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsConnected
GO

CREATE FUNCTION LRS_IsValidPoint (
    @geometry GEOMETRY
    )
RETURNS BIT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].IsValidPoint
GO

CREATE FUNCTION LRS_LocatePointAlongGeom (
    @geometry GEOMETRY
    ,@distance FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].LocatePointAlongGeom
GO

CREATE FUNCTION LRS_MergeGeometrySegments (
    @geometry1 GEOMETRY
    ,@geometry2 GEOMETRY
    ,@tolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].MergeGeometrySegments
GO

CREATE FUNCTION LRS_MergeAndResetGeometrySegments (
    @geometry1 GEOMETRY
    ,@geometry2 GEOMETRY
    ,@tolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].MergeAndResetGeometrySegments
GO

CREATE FUNCTION LRS_OffsetGeometrySegments (
    @geometry GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    ,@offset FLOAT
    ,@tolerance FLOAT(53)
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].OffsetGeometrySegment
GO

CREATE FUNCTION LRS_PopulateGeometryMeasures (
    @geometry GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    )
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].PopulateGeometryMeasures
GO

CREATE FUNCTION LRS_ResetMeasure(@geometry GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ResetMeasure
GO

CREATE FUNCTION LRS_ReverseLinearGeometry (@geometry GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ReverseLinearGeometry
GO

CREATE FUNCTION LRS_ScaleGeometrySegment (@geometry GEOMETRY
    ,@startMeasure FLOAT
    ,@endMeasure FLOAT
    ,@shiftMeasure FLOAT)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ScaleGeometrySegment
GO

CREATE PROCEDURE LRS_SplitGeometrySegment @geometry GEOMETRY
    ,@splitMeasure FLOAT
    ,@geometry1 GEOMETRY OUTPUT
    ,@geometry2 GEOMETRY OUTPUT
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].SplitGeometrySegment
GO

CREATE FUNCTION LRS_TranslateMeasure (@geometry GEOMETRY
    ,@translateMeasure FLOAT)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].TranslateMeasure
GO

CREATE FUNCTION LRS_ValidateLRSGeometry (@geometry GEOMETRY)
RETURNS NVARCHAR(10)
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.LRS.Geometry].ValidateLRSGeometry
GO

CREATE FUNCTION Util_PolygonToLine (@geometry GEOMETRY)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.Util.Geometry].PolygonToLine
GO

CREATE FUNCTION Util_Extract (@geometry GEOMETRY
    ,@elementIndex INTEGER
    ,@ringIndex INTEGER)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.Util.Geometry].ExtractGeometry
GO

CREATE FUNCTION Util_RemoveDuplicateVertices (@geometry GEOMETRY, @tolerance FLOAT)
RETURNS GEOMETRY
AS
EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Functions.Util.Geometry].RemoveDuplicateVertices
GO
--#endregion

-- Create aggregates.
CREATE AGGREGATE GEOMETRYEnvelopeAggregate (@geometry GEOMETRY)
RETURNS GEOMETRY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeometryEnvelopeAggregate]
GO

CREATE AGGREGATE GeographyCollectionAggregate (@geography GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeographyCollectionAggregate]
GO

CREATE AGGREGATE GeographyUnionAggregate (@geography GEOGRAPHY)
RETURNS GEOGRAPHY EXTERNAL NAME SQLSpatialTools.[SQLSpatialTools.Aggregates.GeographyUnionAggregate]
GO