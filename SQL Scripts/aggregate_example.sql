-- Script assumes that current database contains zipcodes table with columns: code, state, shape_geom, shape_geog

select state, dbo.GeometryEnvelopeAggregate(shape_geom) from zipcodes group by state;

-- For every state return its polygon by performing an union of all zip area belonging to that state.
-- Also buffer each zip area by 0.5 meter to avoid tiny cracks along the borders of zip areas.
select state, dbo.GeographyUnionAggregate(shape_geog, 0.5) from zipcodes group by state;