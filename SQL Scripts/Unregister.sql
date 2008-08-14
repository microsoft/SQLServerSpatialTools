-- Copyright (c) Microsoft Corporation.  All rights reserved.

-- Insert your database
use []
go

-- Drop the aggregates...
drop aggregate GeometryEnvelopeAggregate
drop aggregate GeographyUnionAggregate

-- Drop the functions...
drop function ShiftGeometry
drop function LocateAlongGeog
drop function LocateAlongGeom
drop function InterpolateBetweenGeog
drop function InterpolateBetweenGeom
drop function VacuousGeometryToGeography
drop function VacuousGeographyToGeometry

-- Drop the types...
drop type Projection
drop type AffineTransform

-- Drop the assembly...
drop assembly SQLSpatialTools


