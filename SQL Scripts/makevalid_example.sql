-- Simple bow-tie polygon
declare @geog varchar(max) = 'POLYGON ((-10 -10, 10 10, -10 10, 10 -10, -10 -10))';

select geometry::Parse('POLYGON ((-10 -10, 10 10, -10 10, 10 -10, -10 -10))')

select dbo.IsValidGeography(@geog, 4326);
-- returns 0 without throwing an exception

select dbo.MakeValidGeography(@geog, 4326);
-- returns multipolygon of two triangles