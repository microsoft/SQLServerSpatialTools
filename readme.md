# SQL Server Spatial Tools

## Contents

- [Introduction](#introduction)
- [Build and signing](#build-and-signing)
- [Installation](#installation)
- [Features](#features)

## Introduction

This project is a collection of tools for use with the spatial types
in SQL Server. This project does not provide an
end-user application, but rather a set of reusable functions which
applications can make use of.

Source and development are hosted on
[GitHub](https://github.com/microsoft/SQLServerSpatialTools).

This package is licensed under the Microsoft Public License. See
[License.txt](License.txt) for details.

## Build and signing

### Target frameworks

The library, regular unit tests, and test-extension project target both
`net472` and `net10.0`. A normal solution build produces both versions from the
same source, with outputs in separate framework directories:

```text
output\lib\net472\SQLSpatialTools.dll
output\lib\net10.0\SQLSpatialTools.dll
```

Only the `net472` version can be registered inside SQL Server CLR. The `net10.0`
version is for ordinary .NET applications. NuGet selects the `net472` asset of
`Microsoft.SqlServer.Types` for the Framework build and its `netstandard2.0` asset
for the modern build. Its native spatial DLLs require Windows x86, x64, or
ARM64; targeting modern .NET does not make spatial execution fully cross-platform.

### Tests

The regular tests also compile and run once for each framework:

```powershell
dotnet test unittest\SQLSpatialTools.UnitTests.csproj
dotnet test unittest\SQLSpatialTools.UnitTests.csproj --framework net472
dotnet test unittest\SQLSpatialTools.UnitTests.csproj --framework net10.0
```

Project references select the matching library and test-extension build.
The LRS DDD test project remains `net472`-only. It is not
included in these regular test commands, and solution-wide `dotnet test` runs
report its tests as skipped by default, before any database setup occurs:

```powershell
dotnet test SQLSpatialTools.sln
```

To explicitly enable the DDD integration tests in a configured environment:

```powershell
dotnet test unittest-ddd\SQLSpatialTools.UnitTests.DDD.csproj -p:RunDDDTests=true
```

> **Warning:** These tests require Windows, SQL Compact native binaries, and
> configured Oracle/SQL Server connections. They create local test data and can
> modify the configured databases, including unregistering SQL CLR assemblies.
> Use isolated test databases, never production databases.

`RunDDDTests=true` selects [`unittest-ddd\DDD.runsettings`](unittest-ddd/DDD.runsettings)
unless custom runsettings are supplied. Custom runsettings must explicitly
include a `TestRunParameters` parameter named `RunDDDTests` with value `true`.
Visual Studio or direct DLL test runs also skip the suite unless that parameter
is supplied; selecting
`DDD.runsettings` opts in. Merely building with `RunDDDTests=true` does not enable
future runs without those runsettings.

### Signing

SDK-style projects build unsigned by default, without a certificate or key:

```powershell
dotnet build SQLSpatialTools.sln
```

SQL CLR strong-name signing is opt-in and applies only to .NET Framework
targets. Modern .NET targets remain unsigned and do not generate or read keys.
All Framework projects, including the tests, use the same selected key.
Friend-assembly declarations are generated with that key's public key, or
without a public key for unsigned builds.

`SqlClrSigningMode` has three values:

| Mode | Behavior |
| --- | --- |
| `None` (default) | Unsigned; no signing key is needed. |
| `Development` | Generate `.signing\development.snk` once, then reuse it. |
| `Production` | Require `.signing\production.snk`; never generate a missing key. |

The `.signing` directory at the repository root is ignored by Git. Keys survive
builds, rebuilds, and clean. Do not commit private keys or distribute them with
the DLL.

For signed local development, including an isolated local SQL CLR test server:

```powershell
dotnet build SQLSpatialTools.sln -p:SqlClrSigningMode=Development
```

For customer or official production builds, provision your own private `.snk`
key in `.signing\production.snk`, or supply its path:

```powershell
dotnet build SQLSpatialTools.sln -c Release -p:SqlClrSigningMode=Production
dotnet build SQLSpatialTools.sln -c Release -p:SqlClrSigningMode=Production -p:SqlClrSigningKeyFile="C:\BuildKeys\CustomerSpatial.snk"
```

The same properties work with Visual Studio MSBuild:

```powershell
msbuild SQLSpatialTools.sln -restore -p:SqlClrSigningMode=Development
```

`SqlClrSigningDirectory` overrides the default key directory, and
`SqlClrSigningKeyFile` overrides the selected key file. Relative key paths are
resolved from the repository root. Use native paths on Linux/macOS, for example
`-p:SqlClrSigningKeyFile=/home/build/keys/customer-spatial.snk`.

Key generation and script preparation run inside MSBuild, with no user-run
setup scripts, `sn.exe`, PowerShell, or global tool installation. Use the .NET 10
SDK or newer, or a Visual Studio MSBuild version supporting .NET 10 on Windows.
Framework builds reference Microsoft's build-only
`Microsoft.NETFramework.ReferenceAssemblies` package, so they can compile without
a locally installed Windows targeting pack.
Building on another platform does not make SQL Server CLR or the Windows-native
spatial and SQL Compact test dependencies portable.

Only fully signed private `.snk` keys are supported, not PFX, public-only keys,
`PublicSign`, or `DelaySign`. Development generates a 2048-bit RSA key, restricts
new Unix key files to their owner, and never replaces an existing key, even
when multiple projects build concurrently. An invalid key fails the build.
Visual Studio design-time builds do not generate keys; run a signed `Development`
build once before using that mode for design-time analysis.
Production key provisioning and protection are the responsibility of the
customer or release pipeline. Never silently replace a missing production key:
changing a key changes the assembly identity and its SQL Server trust setup.

Build-signing regression checks can be run through either build host:

```powershell
dotnet msbuild build\tests\Signing.Tests.proj -t:Test -m
msbuild build\tests\Signing.Tests.proj -t:Test -m
```

## Installation

Scripts for registering and unregistering the functionality in
SQL Server are included in the [SQL Scripts directory](src/SQL%20Scripts).

### Configure assembly trust

SQL Server 2017 and later enable CLR strict security by default. `Register.sql`
does not disable it. Signing by itself does not establish SQL Server trust:
an administrator must approve the signing key or the exact assembly hash.

To generate signing-key trust setup as part of a signed build:

```powershell
dotnet build src\SQLSpatialTools.csproj -p:SqlClrSigningMode=Production -p:SqlClrGenerateTrustScript=true
```

Or request the target directly:

```powershell
dotnet msbuild src\SQLSpatialTools.csproj -restore -t:GenerateSqlClrTrustScript -p:SqlClrSigningMode=Production
```

This writes `output\lib\net472\SQL Scripts\TrustAssembly.sql`. It never connects to SQL
Server or grants permissions automatically. `SqlClrTrustAssemblyPath` can specify
the DLL's deployment path on the server, for example:

```text
-p:SqlClrTrustAssemblyPath="C:\SqlClr\SQLSpatialTools.dll"
```

The administrator runs this script once in `master`. It imports the DLL's public
asymmetric key, creates a key-based login, and grants `UNSAFE ASSEMBLY`. The DLL
must be accessible to the SQL Server service account; the private key must not
be copied to the server. Subsequent builds signed with that key reuse the trust.
For key rotation, provision the new key with distinct SQL key/login names and
review existing registrations. Dependencies need their own applicable trust.

> **Warning:** This trusts other assemblies signed with the same key, not just
> this DLL. Never trust a shared repository key or a development key on a
> production server.

Development trust scripts are explicitly marked for isolated local tests.
Every Framework build removes any old generated trust script; request generation
again for the current build. Unsigned builds can instead be approved by an
administrator using `sys.sp_add_trusted_assembly` with the exact SHA-512 DLL hash.
Do not enable `TRUSTWORTHY` or disable strict security to bypass trust setup.

### Register

1. Configure assembly trust as described above.
2. Select the database where the functionality should be installed.
3. Execute `output\lib\net472\SQL Scripts\Register.sql` on your SQL Server instance.
   The build substitutes the local DLL path; edit it if the server uses a
   different deployment path. `RegisterAndValidate.sql` also receives the built
   script directory. These substitutions do not require PowerShell.

### Unregister

1. Edit the `Unregister.sql` file in the SQL Scripts directory and insert
   the name of the database you are unregistering the functionality
   from where indicated at the beginning of the script.
2. Execute the script on your SQL Server instance.

## Features

See the [public API reference](API.md) for the complete exposed type and method
inventory, including lower-level sinks, KML models, and extension methods.

### Scripts

Several scripts are included in the SQL Scripts directory. These include
scripts for registering and unregistering all of the following
components, as well as several examples of their use.

### Functions

These static methods, implemented in the class `Functions`, can both be
registered in SQL Server and used through T-SQL, as well as be used
directly from the CLR:

#### IsValidGeographyFromGeometry

```csharp
bool IsValidGeographyFromGeometry(SqlGeometry geometry)
```

Check if an input geometry can represent a valid geography without throwing an exception.
This function requires that the geometry be in longitude/latitude coordinates and that
those coordinates are in correct order in the geometry instance (i.e. latitude/longitude
not longitude/latitude). This function will return false (0) if the input geometry is not
in the correct latitude/longitude format, including a valid geography SRID.

#### IsValidGeographyFromText

```csharp
bool IsValidGeographyFromText(string inputWKT, int srid)
```

Check if an input WKT can represent a valid geography. This function requires that
the WKT coordinate values are longitude/latitude values, in that order and that a valid
geography SRID value is supplied.  This function will not throw an exception even in
edge conditions (i.e. longitude/latitude coordinates are reversed to latitude/longitude).

#### MakeValidGeographyFromGeometry

```csharp
SqlGeography MakeValidGeographyFromGeometry(SqlGeometry geometry)
```

Convert an input geometry instance to a valid geography instance.
This function requires that the WKT coordinate values are longitude/latitude values,
in that order and that a valid geography SRID value is supplied.

#### MakeValidGeographyFromText

```csharp
SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)
```

Convert an input WKT to a valid geography instance.
This function requires that the WKT coordinate values are longitude/latitude values,
in that order and that a valid geography SRID value is supplied.

#### ConvexHullGeography

```csharp
SqlGeography ConvexHullGeography(SqlGeography geography)
```

Computes ConvexHull of input geography and returns a polygon (unless all input points are colinear).

#### ConvexHullGeographyFromText

```csharp
SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)
```

Computes ConvexHull of input WKT and returns a polygon (unless all input points are colinear).
This function does not require its input to be a valid geography. This function does require
that the WKT coordinate values are longitude/latitude values, in that order and that a valid
geography SRID value is supplied.

#### DensifyGeography

```csharp
SqlGeography DensifyGeography(SqlGeography g, double maxAngle)
```

Returns a geography instance equivalent to its input, but with no two
consecutive points spaced more than `maxAngle` apart.

#### InterpolateBetweenGeog

```csharp
SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)
```

Takes start and end geography points and returns a new point that is a
given distance from the start toward the end.

#### InterpolateBetweenGeom

```csharp
SqlGeometry InterpolateBetweenGeom(SqlGeometry start, SqlGeometry end, double distance)
```

Takes start and end geometry points and returns a new point that is a
given distance from the start toward the end.

#### LocateAlongGeog

```csharp
SqlGeography LocateAlongGeog(SqlGeography g, double distance)
```

Takes a geography linestring and finds the point a given distance along
it.

#### LocateAlongGeom

```csharp
SqlGeometry LocateAlongGeom(SqlGeometry g, double distance)
```

Takes a geometry linestring and finds the point a given distance along
it.

#### ShiftGeometry

```csharp
SqlGeometry ShiftGeometry(SqlGeometry g, double xShift, double yShift)
```

Takes a geometry instance and shifts it by a given X and Y amount.

#### VacuousGeographyToGeometry

```csharp
SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)
```

A special case of the equirectangular projection, taking each point
`(lat, long) -> (y, x)`.

#### VacuousGeometryToGeography

```csharp
SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)
```

The inverse of the `VacuousGeographyToGeometry` projection.

### Types

These types can be registered in SQL Server or used directly through the
CLR.

#### SqlProjection

This class provides an extensible access point to various projections and
inverse projections. See [projection_example.sql](src/SQL%20Scripts/projection_example.sql) for a sample of
its use. Currently supported projections are:

- Albers Equal Area
- Equirectangular
- Lambert Conformal Conic
- Mercator
- Oblique Mercator
- Transverse Mercator
- Gnomonic

#### AffineTransform

This provides general affine transformations. See the example
[transform_example.sql](src/SQL%20Scripts/transform_example.sql) for a sample of its use.

### Aggregates

While implemented as classes, aggregates are essentially functions that
take a collection of inputs to a single result.

#### GeographyUnionAggregate

This aggregate finds the union of a set of geographies with an optional
additional buffer.

#### GeometryEnvelopeAggregate

This aggregate finds the envelope that contains a set of input
geometries.

### LRS functions

Linear referencing (also called linear reference system or linear referencing system or LRS)
is a method of spatial referencing in engineering and construction, in which the locations of
physical features along a linear element are described in terms of measurements from a fixed point,
such as a milestone along a road.

| Function | Description |
| --- | --- |
| `LRS_ClipGeometrySegment` | Returns a clipped geometry segment after a clip operation. |
| `LRS_ConvertToLrsGeom` | Converts a standard geometry linestring to an LRS geometric segment by adding measure information. |
| `LRS_GetEndMeasure` | Returns the end measure of a geometric segment. |
| `LRS_GetMergePosition` | Checks if two input geometries are spatially connected. If connected, returns the connected/merge position; otherwise, returns false. |
| `LRS_GetStartMeasure` | Returns the start measure of a geometric segment. |
| `LRS_InterpolateBetweenGeom` | Computes a new point with the specified measure between two input points of the geometric segment. |
| `LRS_IsConnected` | Checks if two input geometries are spatially connected. |
| `LRS_IsValidPoint` | Checks if a geometry segment is a valid point type. |
| `LRS_LocatePointAlongGeom` | Returns the point located at a specified measure from the start of a geometric segment. |
| `LRS_MergeGeometrySegment` | Returns the geometry object resulting from the concatenation of two geometric segments. |
| `LRS_MergeAndResetGeometrySegments` | Merges the input segments within tolerance and resets the measure of the resultant geometry segment. |
| `LRS_OffsetGeometrySegments` | Returns the geometric segment at a specified offset from a geometric segment. |
| `LRS_PopulateGeometryMeasures` | Populates the measures of all shape points based on the `startMeasure` and `endMeasure` of a geometric segment, overriding any previously assigned measures between the start and end points. |
| `LRS_ResetMeasure` | Sets all measures of a geometric segment, including the start and end measures, to null values, overriding any previously assigned measures. |
| `LRS_ReverseLinearGeometry` | Returns a new geometric segment by reversing the measure values and direction of the original geometric segment. |
| `LRS_ScaleGeometrySegment` | Returns a new geometric segment by scaling the original geometric segment: redefining and shifting the measure values of all points by the given shift measure. |
| `LRS_SplitGeometrySegment` | Splits a geometric segment into two geometric segments. |
| `LRS_TranslateMeasure` | Returns a new geometric segment by adding the given translate measure to the measure values of all points in the original geometric segment. |
| `LRS_ValidateLRSGeometry` | Checks if the input LRS geometry is valid. |

### Utility functions

| Function | Description |
| --- | --- |
| `Util_PolygonToLine` | Converts all polygon-type elements in a geometry to line-type elements. |
| `Util_RemoveDuplicateVertices` | Removes consecutive duplicate vertices and collinear points of the valid geometry bounds within the tolerance. |
| `Util_Extract` | Returns the geometry specified by the element index and ring index. |
