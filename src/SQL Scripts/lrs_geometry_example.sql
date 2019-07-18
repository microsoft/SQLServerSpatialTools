-- Script that shows examples on exposed LRS Functions in SQLSpatialTools Library
-- LRS Geometric Functions
DECLARE @geom geometry;
DECLARE @geom1 geometry
DECLARE @geom2 geometry
DECLARE @srid INT = 4326;
DECLARE @distance FLOAT;
DECLARE @measure FLOAT;
DECLARE @offset FLOAT = 2;
DECLARE @tolerance FLOAT = 0.5;
DECLARE @startMeasure FLOAT = 15.0;
DECLARE @endMeasure FLOAT = 20.0;

SET @geom = GEOMETRY::STGeomFromText('LINESTRING (20 1 NULL 10, 25 1 NULL 25 )', @srid);

-- 1. ClipGeometrySegement Function
SELECT 'Clipped Segment' AS 'FunctionInfo'
    ,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure, @tolerance) AS 'Geometry'
    ,[dbo].[LRS_ClipGeometrySegment](@geom, @startMeasure, @endMeasure, @tolerance).ToString() AS 'Geometry in String'

-- 2. Get Start Measure
SELECT 'Start Measure' AS 'FunctionInfo'
    ,[dbo].[LRS_GetStartMeasure](@geom) AS 'Measure'

-- 3. Get Merge Position
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING(5 5 0 0, 2 2 0 0)', @srid);
SET @tolerance = 0.5;

SELECT 'Merge Position' AS 'FunctionInfo'
    ,[dbo].LRS_GetMergePosition(@geom1, @geom2, @tolerance) AS 'IsConnected';

-- 4. Get End Measure
SELECT 'End Measure' AS 'FunctionInfo'
    ,[dbo].[LRS_GetEndMeasure](@geom) AS 'Measure'

-- 5. Interpolate points between Geom
SET @geom1 = GEOMETRY::STGeomFromText('POINT(0 0 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('POINT(10 0 0 10)', @srid);

SET @measure = 5;

SELECT 'Interpolate Points' AS 'FunctionInfo'
    ,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure) AS 'Geometry'
    ,[dbo].[LRS_InterpolateBetweenGeom](@geom1, @geom2, @measure).ToString() AS 'Geometry in String';

-- 6. Is Spatially Connected
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING(0 0 0 0, 1 1 0 0, 3 4 0 0, 5.5 5 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING(5 5 0 0, 2 2 0 0)', @srid);
SET @tolerance = 0.5;

SELECT 'Is Spatially Connected' AS 'FunctionInfo'
    ,[dbo].[LRS_IsConnected](@geom1, @geom2, @tolerance) AS 'IsConnected';

-- 7. IsValid LRS Point
SET @geom = GEOMETRY::STGeomFromText('POINT(0 0 0)', @srid);

SELECT 'Is Valid LRS Point' AS 'FunctionInfo'
    ,[dbo].[LRS_IsValidPoint](@geom) AS 'IsValidLRSPoint';

-- 8. Locate Point Along the Geometry Segment
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (0 0 0 0, 10 0 0 10)', @srid);
SET @measure = 5.0;

SELECT 'Point to Locate' AS 'FunctionInfo'
    ,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure) AS 'Geometry'
    ,[dbo].[LRS_LocatePointAlongGeom](@geom, @measure).ToString() AS 'Geometry in String';

-- 9. Merge two Geometry Segments to one Geometry Segment.
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING (30 1 NULL 30, 40 1 NULL 40 )', @srid);

SELECT 'Merge Geometry Segments' AS 'FunctionInfo'
    ,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2, @tolerance) AS 'Geometry'
    ,[dbo].[LRS_MergeGeometrySegments](@geom1, @geom2, @tolerance).ToString() AS 'Geometry in String';

-- 10. Merge two Geometry Segments to one Geometry Segment and reset there measures
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING (10 15 9, 10 14 12, 10 10 20)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING (9.5 14.5 100, 10 18 10, 10 10 5)', @srid);

SELECT 'Merge and Reset Geometry Segments' AS 'FunctionInfo'
    ,[dbo].[LRS_MergeAndResetGeometrySegments](@geom1, @geom2, @tolerance) AS 'Geometry'
    ,[dbo].[LRS_MergeAndResetGeometrySegments](@geom1, @geom2, @tolerance).ToString() AS 'Geometry in String';

-- 11. OffsetGeometrySegment Function
SET @geom = GEOMETRY::STGeomFromText('LINESTRING(5 10 0, 20 5 30.628, 35 10 61.257, 55 10 100)', @srid);
SET @startMeasure = 0;
SET @endMeasure = 61.5;
SET @offset = 2;
SELECT 'Offset Segment' AS 'FunctionInfo'
    ,[dbo].[LRS_OffsetGeometrySegments](@geom, @startMeasure, @endMeasure, @offset, @tolerance) AS 'Geometry'
    ,[dbo].[LRS_OffsetGeometrySegments](@geom, @startMeasure, @endMeasure, @offset, @tolerance).ToString() AS 'Geometry in String'

-- 12. Populate geometry measures.
SET @startMeasure = 10;
SET @endMeasure = 40;
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (10 1 10 100, 15 1 10 NULL, 20 1 10 NULL, 25 1 10 250 )', @srid);

SELECT 'Populate Geometric Measures' AS 'FunctionInfo'
    ,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure) AS 'Geometry'
    ,[dbo].[LRS_PopulateGeometryMeasures](@geom, @startMeasure, @endMeasure).ToString() AS 'Geometry in String';
    
-- 13. Convert To  Lrs Geometry
SET @startMeasure = 10;
SET @endMeasure = 40;
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (10 1 , 15 1, 20 1, 25 1)', @srid);
SELECT 'Convert Lrs geometry' AS 'FunctionInfo'
    ,[dbo].[LRS_ConvertToLrsGeom](@geom, @startMeasure, @endMeasure) AS 'Geometry'
    ,[dbo].[LRS_ConvertToLrsGeom](@geom, @startMeasure, @endMeasure).ToString() AS 'Geometry in String';

-- 14. Reset Measure
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 10, 5 5 0 25)', @srid);

SELECT 'Reset Measure' AS 'FunctionInfo'
    ,[dbo].[LRS_ResetMeasure](@geom) AS 'Geometry'
    ,@geom.ToString() AS 'Input Line'
    ,[dbo].[LRS_ResetMeasure](@geom).ToString() AS 'Geometry in String'

-- 15. Reverse Line String
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 0, 5 5 0 0)', @srid);

SELECT 'Reverse Linear Geometry' AS 'FunctionInfo'
    ,[dbo].[LRS_ReverseLinearGeometry](@geom) AS 'Geometry'
    ,@geom.ToString() AS 'Input Line'
    ,[dbo].[LRS_ReverseLinearGeometry](@geom).ToString() AS 'Geometry in String'

-- 16. Scale Geometry Measures
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (2 2 0 6, 2 4 0 7, 8 4 0 8)', @srid);
SET @startMeasure = 10;
SET @endMeasure = 40;
SET @measure = 5;

SELECT 'Scale Geometry Measures' AS 'FunctionInfo'
    ,[dbo].[LRS_ScaleGeometrySegment](@geom, @startMeasure, @endMeasure, @measure) AS 'Geometry'
    ,@geom.ToString() AS 'Input Line'
    ,[dbo].[LRS_ScaleGeometrySegment](@geom, @startMeasure, @endMeasure, @measure).ToString() AS 'Geometry in String'

-- 17. Split Geometry Segment
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (10 1 NULL 10, 25 1 NULL 25)', @srid);
SET @measure = 15;
EXECUTE [dbo].[LRS_SplitGeometrySegment] 
   @geom
  ,@measure
  ,@geom1 OUTPUT
  ,@geom2 OUTPUT

SELECT 'Split Line Segment' AS 'FunctionInfo'
    ,@geom.ToString() AS 'Input Line'
    ,@geom1.ToString() AS 'Line Segment 1'
    ,@geom2.ToString() AS 'Line Segment 2'

-- 18. Translate Linear Geometry Measure
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 10, 5 5 0 20)', @srid);
SET @measure = 5;

SELECT 'Translate Measures' AS 'FunctionInfo'
    ,[dbo].[LRS_TranslateMeasure](@geom, @measure) AS 'Geometry'
    ,@geom.ToString() AS 'Input Line'
    ,[dbo].[LRS_TranslateMeasure](@geom, @measure).ToString() AS 'Geometry in String'

-- 19. Validate LRS Segment
SET @geom1 = GEOMETRY::STGeomFromText('LINESTRING (1 1 0 0, 5 5 0 0)', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('LINESTRING (2 2 0, 2 4 2, 8 4 8, 12 4 12, 12 10 29, 8 10 22, 5 14 27)', @srid);
SET @geom = GEOMETRY::STGeomFromText('LINESTRING (2 2, 2 4, 8 4)', @srid);

SELECT 'Validate LRS Segment' AS 'FunctionInfo'
    ,@geom1.ToString() AS 'Input Geom Segment'
    ,[dbo].[LRS_ValidateLRSGeometry](@geom1) AS 'Valid State'
UNION
SELECT 'Validate LRS Segment' AS 'FunctionInfo'
    ,@geom2.ToString() AS 'Input Geom Segment'
    ,[dbo].[LRS_ValidateLRSGeometry](@geom2) AS 'Valid State'
UNION
SELECT 'Validate LRS Segment' AS 'FunctionInfo'
    ,@geom.ToString() AS 'Input Geom Segment'
    ,[dbo].[LRS_ValidateLRSGeometry](@geom) AS 'Valid State';
