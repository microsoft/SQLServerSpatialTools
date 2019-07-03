-- Copyright (c) Microsoft Corporation.  All rights reserved.

-- Install the SQLSpatialTools assembly and all its functions into the current database

-- Enable CLR
sp_configure 'clr enabled', 1
reconfigure
go

-- !!! Insert the path to the SQLSpatialTools assembly here !!!
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

create function ConvexHullGeography(@geog geography) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].ConvexHullGeography
go

create function ConvexHullGeographyFromText(@inputWKT nvarchar(max), @srid int) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].ConvexHullGeographyFromText
go

create function IsValidGeographyFromGeometry(@inputGeometry geometry) returns bit
as external name SQLSpatialTools.[SQLSpatialTools.Functions].IsValidGeographyFromGeometry
go

create function IsValidGeographyFromText(@inputWKT nvarchar(max), @srid int) returns bit
as external name SQLSpatialTools.[SQLSpatialTools.Functions].IsValidGeographyFromText
go

create function MakeValidGeographyFromGeometry(@inputGeometry geometry) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].MakeValidGeographyFromGeometry
go

create function MakeValidGeographyFromText(@inputWKT nvarchar(max), @srid int) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].MakeValidGeographyFromText
go

create function FilterArtifactsGeometry(@g geometry, @filterEmptyShapes bit, @filterPoints bit, @lineStringTolerance float(53), @ringTolerance float(53)) returns geometry
as external name SQLSpatialTools.[SQLSpatialTools.Functions].FilterArtifactsGeometry
go

create function FilterArtifactsGeography(@g geography, @filterEmptyShapes bit, @filterPoints bit, @lineStringTolerance float(53), @ringTolerance float(53)) returns geography
as external name SQLSpatialTools.[SQLSpatialTools.Functions].FilterArtifactsGeography
go

-- Create aggregates.

create aggregate GeometryEnvelopeAggregate(@geom geometry) returns geometry
external name SQLSpatialTools.[SQLSpatialTools.GeometryEnvelopeAggregate]
go

create aggregate GeographyCollectionAggregate(@geog geography) returns geography
external name SQLSpatialTools.[SQLSpatialTools.GeographyCollectionAggregate]
go

create aggregate GeographyUnionAggregate(@geog geography) returns geography
external name SQLSpatialTools.[SQLSpatialTools.GeographyUnionAggregate]
go