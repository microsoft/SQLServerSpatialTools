--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!
-- Copyright (c) Microsoft Corporation.  All rights reserved.
-- Drop the SQLSpatialTools assembly and all its functions from the current database
-- Drop the aggregates...
IF OBJECT_ID('GeometryEnvelopeAggregate') IS NOT NULL
    DROP AGGREGATE GeometryEnvelopeAggregate

IF OBJECT_ID('GeographyCollectionAggregate') IS NOT NULL
    DROP AGGREGATE GeographyCollectionAggregate

IF OBJECT_ID('GeographyUnionAggregate') IS NOT NULL
    DROP AGGREGATE GeographyUnionAggregate

-- Drop the functions...
-- General Geometry
IF OBJECT_ID('FilterArtifactsGeometry') IS NOT NULL
    DROP FUNCTION FilterArtifactsGeometry

IF OBJECT_ID('GeomFromXYMText') IS NOT NULL
    DROP FUNCTION GeomFromXYMText

IF OBJECT_ID('InterpolateBetweenGeom') IS NOT NULL
    DROP FUNCTION InterpolateBetweenGeom

IF OBJECT_ID('LocateAlongGeom') IS NOT NULL
    DROP FUNCTION LocateAlongGeom

IF OBJECT_ID('MakeValidForGeography') IS NOT NULL
    DROP FUNCTION MakeValidForGeography

IF OBJECT_ID('ReverseLinestring') IS NOT NULL
    DROP FUNCTION ReverseLinestring

IF OBJECT_ID('ShiftGeometry') IS NOT NULL
    DROP FUNCTION ShiftGeometry

IF OBJECT_ID('VacuousGeometryToGeography') IS NOT NULL
    DROP FUNCTION VacuousGeometryToGeography

-- General Geography
IF OBJECT_ID('ConvexHullGeographyFromText') IS NOT NULL
    DROP FUNCTION ConvexHullGeographyFromText

IF OBJECT_ID('ConvexHullGeography') IS NOT NULL
    DROP FUNCTION ConvexHullGeography

IF OBJECT_ID('DensifyGeography') IS NOT NULL
    DROP FUNCTION DensifyGeography

IF OBJECT_ID('FilterArtifactsGeography') IS NOT NULL
    DROP FUNCTION FilterArtifactsGeography

IF OBJECT_ID('InterpolateBetweenGeog') IS NOT NULL
    DROP FUNCTION InterpolateBetweenGeog

IF OBJECT_ID('IsValidGeographyFromGeometry') IS NOT NULL
    DROP FUNCTION IsValidGeographyFromGeometry

IF OBJECT_ID('IsValidGeographyFromText') IS NOT NULL
    DROP FUNCTION IsValidGeographyFromText

IF OBJECT_ID('LocateAlongGeog') IS NOT NULL
    DROP FUNCTION LocateAlongGeog

IF OBJECT_ID('MakeValidGeographyFromGeometry') IS NOT NULL
    DROP FUNCTION MakeValidGeographyFromGeometry

IF OBJECT_ID('MakeValidGeographyFromText') IS NOT NULL
    DROP FUNCTION MakeValidGeographyFromText

IF OBJECT_ID('VacuousGeographyToGeometry') IS NOT NULL
    DROP FUNCTION VacuousGeographyToGeometry

-- LRS Geometry
IF OBJECT_ID('LRS_ClipGeometrySegment') IS NOT NULL
    DROP FUNCTION LRS_ClipGeometrySegment

IF OBJECT_ID('LRS_ConvertToLrsGeom') IS NOT NULL
DROP FUNCTION LRS_ConvertToLrsGeom

IF OBJECT_ID('LRS_GetEndMeasure') IS NOT NULL
    DROP FUNCTION LRS_GetEndMeasure    

IF OBJECT_ID('LRS_GetMergePosition') IS NOT NULL
    DROP FUNCTION LRS_GetMergePosition

IF OBJECT_ID('LRS_GetStartMeasure') IS NOT NULL
    DROP FUNCTION LRS_GetStartMeasure

IF OBJECT_ID('LRS_InterpolateBetweenGeom') IS NOT NULL
    DROP FUNCTION LRS_InterpolateBetweenGeom

IF OBJECT_ID('LRS_IsConnected') IS NOT NULL
    DROP FUNCTION LRS_IsConnected

IF OBJECT_ID('LRS_IsValidPoint') IS NOT NULL
    DROP FUNCTION LRS_IsValidPoint

IF OBJECT_ID('LRS_LocatePointAlongGeom') IS NOT NULL
    DROP FUNCTION LRS_LocatePointAlongGeom

IF OBJECT_ID('LRS_MergeGeometrySegments') IS NOT NULL
    DROP FUNCTION LRS_MergeGeometrySegments

IF OBJECT_ID('LRS_MergeAndResetGeometrySegments') IS NOT NULL
	DROP FUNCTION LRS_MergeAndResetGeometrySegments

IF OBJECT_ID('LRS_OffsetGeometrySegments') IS NOT NULL
    DROP FUNCTION LRS_OffsetGeometrySegments

IF OBJECT_ID('LRS_PopulateGeometryMeasures') IS NOT NULL
    DROP FUNCTION LRS_PopulateGeometryMeasures

IF OBJECT_ID('LRS_ResetMeasure') IS NOT NULL
    DROP FUNCTION LRS_ResetMeasure

IF OBJECT_ID('LRS_ReverseLinearGeometry') IS NOT NULL
    DROP FUNCTION LRS_ReverseLinearGeometry

IF OBJECT_ID('LRS_ScaleGeometrySegment') IS NOT NULL
    DROP FUNCTION LRS_ScaleGeometrySegment

IF OBJECT_ID('LRS_SplitGeometrySegment') IS NOT NULL
    DROP PROCEDURE LRS_SplitGeometrySegment

IF OBJECT_ID('LRS_TranslateMeasure') IS NOT NULL
    DROP FUNCTION LRS_TranslateMeasure

IF OBJECT_ID('LRS_ValidateLRSGeometry') IS NOT NULL
    DROP FUNCTION LRS_ValidateLRSGeometry

-- Utility Functions
IF OBJECT_ID('Util_PolygonToLine') IS NOT NULL
    DROP FUNCTION Util_PolygonToLine

IF OBJECT_ID('Util_RemoveDuplicateVertices') IS NOT NULL
    DROP FUNCTION Util_RemoveDuplicateVertices
	
IF OBJECT_ID('Util_Extract') IS NOT NULL
    DROP FUNCTION Util_Extract

-- Drop the types...
IF TYPE_ID('Projection') IS NOT NULL
    DROP TYPE Projection

IF TYPE_ID('AffineTransform') IS NOT NULL
    DROP TYPE AffineTransform

-- Drop the assembly...
DROP ASSEMBLY IF EXISTS SQLSpatialTools
