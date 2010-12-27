-- Copyright (c) Microsoft Corporation.  All rights reserved.

-- Drop the SQLSpatialTools assembly and all its functions from the current database

-- Drop the aggregates...
drop aggregate GeometryEnvelopeAggregate
drop aggregate GeographyCollectionAggregate
drop aggregate GeographyUnionAggregate

-- Drop the functions...
drop function ShiftGeometry
drop function LocateAlongGeog
drop function LocateAlongGeom
drop function InterpolateBetweenGeog
drop function InterpolateBetweenGeom
drop function VacuousGeometryToGeography
drop function VacuousGeographyToGeometry

drop function ConvexHullGeography
drop function ConvexHullGeographyFromText
drop function IsValidGeographyFromGeometry
drop function IsValidGeographyFromText
drop function MakeValidGeographyFromGeometry
drop function MakeValidGeographyFromText

drop function FilterArtifactsGeometry
drop function FilterArtifactsGeography

-- Drop the types...
drop type Projection
drop type AffineTransform

-- Drop the assembly...
drop assembly SQLSpatialTools


