-- Copyright (c) Microsoft Corporation.  All rights reserved.

-- Insert your database
use []
go


-- Enable CLR
sp_configure 'clr enabled', 1
reconfigure
go


-- Insert the path to the SQLSpatialTools assesmbly here
create assembly SQLSpatialTools from '' 
go


-- Create types
create type Projection external name SQLSpatialTools.[SQLSpatialTools.SqlProjection]
go

create type AffineTransform external name SQLSpatialTools.[SQLSpatialTools.AffineTransform]
go


-- Register the functions...
create function ShiftGeometry(@g geometry, @xShift float, @yShift float) returns geometry
as external name SQLSpatialTools.[SQLSpatialTools.Functions].ShiftGeometry
go

create function LocateAlongGeog(@g geography, @distance float) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].LocateAlongGeog
go

create function LocateAlongGeom(@g geometry, @distance float) returns geometry
as external name SQLSpatialTools.[SQLSpatialTools.Functions].LocateAlongGeom
go

create function InterpolateBetweenGeog(@start geography, @end geography, @distance float) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].InterpolateBetweenGeog
go

create function InterpolateBetweenGeom(@start geometry, @end geometry, @distance float) returns geometry
as external name SQLSpatialTools.[SQLSpatialTools.Functions].InterpolateBetweenGeom
go

create function VacuousGeometryToGeography(@toConvert geometry, @targetSrid int) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].VacuousGeometryToGeography
go

create function VacuousGeographyToGeometry(@toConvert geography, @targetSrid int) returns geometry
as external name SQLSpatialTools.[SQLSpatialTools.Functions].VacuousGeographyToGeometry
go


-- Create aggregates.

create aggregate GeometryEnvelopeAggregate(@geom geometry) returns geometry
external name SQLSpatialTools.[SQLSpatialTools.GeometryEnvelopeAggregate]
go

create aggregate GeographyUnionAggregate(@geog geography, @bufferDistance float) returns geography
external name SQLSpatialTools.[SQLSpatialTools.GeographyUnionAggregate]
go
