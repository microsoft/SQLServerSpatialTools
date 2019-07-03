-- Simple bow-tie polygon
declare @geog varchar(max) = 'POLYGON ((0 0, 10 2, 10 0, 0 2, 0 0))';

select dbo.IsValidGeographyFromText(@geog2, 4326);
-- returns 0 without throwing an exception

select dbo.MakeValidGeographyFromText(@geog2, 4326);
-- returns multipolygon of two triangles