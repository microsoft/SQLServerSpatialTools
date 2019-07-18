-- Script that shows examples on exposed Util Functions in SQLSpatialTools Library
-- Util Geometric Functions
DECLARE @geom geometry;
DECLARE @geom1 geometry
DECLARE @geom2 geometry
DECLARE @srid INT = 4326;

-- 1. Utility Function - Polygon To Line
SET @geom = GEOMETRY::STGeomFromText('POLYGON((-5 -5, -5 5, 5 5, 5 -5, -5 -5), (0 0, 3 0, 3 3, 0 3, 0 0))', @srid);
SET @geom1 = GEOMETRY::STGeomFromText('CURVEPOLYGON((-122.3 47, 122.3 -47, 125.7 -49, 121 -38, -122.3 47)) ', @srid);
SET @geom2 = GEOMETRY::STGeomFromText('MULTIPOLYGON(((1 1, 1 1, 1 1, 1 1)),((1 1, 3 1, 3 3, 1 3, 1 1)))', @srid);

SELECT 'Polygon To Line' AS 'FunctionInfo'
    ,@geom.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_PolygonToLine](@geom).ToString() AS 'Converted Line'
UNION ALL
SELECT 'Polygon To Line' AS 'FunctionInfo'
    ,@geom1.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_PolygonToLine](@geom1).ToString() AS 'Converted Line'
UNION ALL
SELECT 'Polygon To Line' AS 'FunctionInfo'
    ,@geom2.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_PolygonToLine](@geom2).ToString() AS 'Converted Line';

-- 2. Utility Function - Extract
SELECT 'Extract' AS 'FunctionInfo'
    ,@geom.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_Extract](@geom, 1, 2).ToString() AS 'Extracted Geom'
    UNION ALL
SELECT 'Extract' AS 'FunctionInfo'
    ,@geom1.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_Extract](@geom, 1, 1).ToString() AS 'Extracted Geom'
UNION ALL
SELECT 'Extract' AS 'FunctionInfo'
    ,@geom2.ToString() AS 'Input Geom Segment'
    ,[dbo].[Util_Extract](@geom, 1, 2).ToString() AS 'Extracted Geom';