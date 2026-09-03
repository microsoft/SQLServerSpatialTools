# Public API reference

This is the exposed .NET API of `SQLSpatialTools.dll`. Spatial and KML operations do not open database connections.

The compiled library exposes **52 classes** and **8 enums**, with **334 declared public/protected method overloads** and **45 constructors**. The inventory also includes **147 properties, fields, and enum values**.

The surface is shared by `net472` and `net10.0`; SQL CLR registration is supported only for `net472`. Public visibility does not imply that a type is registered by the SQL deployment scripts.

Signatures use short parameter type names; each heading identifies the declaring namespace and type. `this` denotes an extension-method receiver. Inherited members are not repeated: follow the base-type note for library inheritance; standard .NET members inherited from `object`, `Exception`, or `Attribute` are omitted. Property accessors are represented by their properties. Internal implementation types and methods are excluded.

## Contents

- [SQL CLR aggregates](#sql-clr-aggregates)
- [General spatial functions](#general-spatial-functions)
- [Linear referencing functions](#linear-referencing-functions)
- [Geometry utility functions](#geometry-utility-functions)
- [KML entry points and helpers](#kml-entry-points-and-helpers)
- [KML export infrastructure](#kml-export-infrastructure)
- [KML parsing and data models](#kml-parsing-and-data-models)
- [Geography event sinks](#geography-event-sinks)
- [Geometry event sinks](#geometry-event-sinks)
- [SQL CLR user-defined types](#sql-clr-user-defined-types)
- [Shared utilities and enums](#shared-utilities-and-enums)

## Choosing an API

| Area | Purpose |
| --- | --- |
| General functions | Validation, repair, interpolation, location by distance, artifact filtering, and geometry/geography conversion. |
| LRS functions | Manipulation of M (measure) values and measured segments: clipping, merging, splitting, scaling, offsets, and validation. |
| KML | In-memory KML import/export, XML parsing, and KML feature/shape models. KML methods process caller-provided data without opening files or database connections. |
| SQL types and aggregates | Map projections, affine transforms, geography collections/unions, and geometry envelopes. |
| Sinks | Lower-level streaming adapters consumed by spatial `Populate` pipelines. Some callbacks deliberately reject unsupported shape events. |
| Utilities | WKT conversion, shape/dimension checks, tolerance and measure comparisons, defaults, and error codes. |

## SQL CLR aggregates

Namespace: `SQLSpatialTools.Aggregates`.

### GeographyCollectionAggregate

Collects geography values into a geography collection through the SQL CLR aggregate lifecycle.

Implements: `Microsoft.SqlServer.Server.IBinarySerialize`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyCollectionAggregate()` | Constructs this type with the supplied configuration (or its default state). |
| `public void Accumulate(SqlGeography g)` | Adds one spatial value to the aggregate. |
| `public void Init()` | Initializes or resets the aggregate state. |
| `public void Merge(GeographyCollectionAggregate group)` | Combines another partial aggregate with this aggregate. |
| `public void Read(BinaryReader r)` | Restores the value or aggregate state from binary serialization. |
| `public SqlGeography Terminate()` | Produces the aggregate's spatial result. |
| `public void Write(BinaryWriter w)` | Writes the value or aggregate state to binary serialization. |

### GeographyUnionAggregate

Computes the spatial union of geography values through the SQL CLR aggregate lifecycle.

Implements: `Microsoft.SqlServer.Server.IBinarySerialize`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyUnionAggregate()` | Constructs this type with the supplied configuration (or its default state). |
| `public void Accumulate(SqlGeography g)` | Adds one spatial value to the aggregate. |
| `public void Init()` | Initializes or resets the aggregate state. |
| `public void Merge(GeographyUnionAggregate group)` | Combines another partial aggregate with this aggregate. |
| `public void Read(BinaryReader r)` | Restores the value or aggregate state from binary serialization. |
| `public SqlGeography Terminate()` | Produces the aggregate's spatial result. |
| `public void Write(BinaryWriter w)` | Writes the value or aggregate state to binary serialization. |

### GeometryEnvelopeAggregate

Computes a bounding envelope for a set of planar geometries.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryEnvelopeAggregate()` | Constructs this type with the supplied configuration (or its default state). |
| `public void Accumulate(SqlGeometry geometry)` | Adds one spatial value to the aggregate. |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void Init()` | Initializes or resets the aggregate state. |
| `public void Merge(GeometryEnvelopeAggregate group)` | Combines another partial aggregate with this aggregate. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |
| `public SqlGeometry Terminate()` | Produces the aggregate's spatial result. |

## General spatial functions

Namespace: `SQLSpatialTools.Functions.General`.

### Geography

This class contains General Geography type functions that can be registered in SQL Server.

| Constructor or method | Summary |
| --- | --- |
| `public static SqlGeography ConvexHullGeography(SqlGeography geography)` | Computes ConvexHull of input geography and returns a polygon (unless all input points are collinear). |
| `public static SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)` | Computes ConvexHull of input WKT and returns a polygon (unless all input points are collinear). |
| `public static SqlGeography DensifyGeography(SqlGeography geography, double maxAngle)` | Inserts intermediate vertices so consecutive points do not exceed the specified angular separation. |
| `public static SqlGeography FilterArtifactsGeography(SqlGeography geography, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)` | Filters empty shapes, points, short linestrings, and thin rings according to the supplied options. |
| `public static SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)` | Find the point that is the given distance from the start point in the direction of the end point. |
| `public static bool IsValidGeographyFromGeometry(SqlGeometry geometry)` | Check if an input geometry can represent a valid geography without throwing an exception. |
| `public static bool IsValidGeographyFromText(string inputWKT, int srid)` | Check if an input WKT can represent a valid geography. |
| `public static SqlGeography LocatePointAlongGeog(SqlGeography geography, double distance)` | Finds a point at the specified distance along a geography linestring. |
| `public static SqlGeography MakeValidGeographyFromGeometry(SqlGeometry geometry)` | Convert an input geometry instance to a valid geography instance. |
| `public static SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)` | Convert an input WKT to a valid geography instance. |
| `public static SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)` | Performs a complete trivial conversion from geography to geometry, simply taking each point (lat,long) -> (y, x). |

### Geometry

This class contains General Geometry type functions that can be registered in SQL Server.

| Constructor or method | Summary |
| --- | --- |
| `public static SqlGeometry FilterArtifactsGeometry(SqlGeometry geometry, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)` | Filters empty shapes, points, short linestrings, and thin rings according to the supplied options. |
| `public static SqlGeometry GeomFromXYMText(string wktXYM, int srid)` | Parses coordinate text and converts Z values into M (measure) values. |
| `public static SqlGeometry InterpolateBetweenGeom(SqlGeometry start, SqlGeometry end, double distance)` | Find the point that is the given distance from the start point in the direction of the end point. |
| `public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double distance)` | Finds a point at a distance along a planar geometry. |
| `public static SqlGeometry MakeValidForGeography(SqlGeometry geometry)` | Make the input geometry valid to use in geography manipulation |
| `public static SqlGeometry ReverseLinestring(SqlGeometry geometry)` | Reverse the Line Segment. |
| `public static SqlGeometry ShiftGeometry(SqlGeometry geometry, double xShift, double yShift)` | Translates a geometry by the specified X and Y offsets. |
| `public static SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)` | This implements a completely trivial conversion from geometry to geography, simply taking each point (x,y) --> (long, lat). |

## Linear referencing functions

Namespace: `SQLSpatialTools.Functions.LRS`.

### Geometry

This provides LRS data manipulation on planar Geometry data type.

| Constructor or method | Summary |
| --- | --- |
| `public static SqlGeometry ClipGeometrySegment(SqlGeometry geometry, double clipStartMeasure, double clipEndMeasure, double tolerance = 0.5)` | Clip a geometry segment based on specified measure. |
| `public static SqlGeometry ConvertToLrsGeom(SqlGeometry geometry, double? startMeasure, double? endMeasure)` | Converts a geometry to measured form using the supplied endpoint measures. |
| `public static SqlDouble GetEndMeasure(SqlGeometry geometry)` | Get end point measure of a LRS Geom Segment. |
| `public static string GetMergePosition(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = 0.5)` | Identifies connected endpoints within tolerance, or returns a false result as text. |
| `public static SqlDouble GetStartMeasure(SqlGeometry geometry)` | Get start point measure of a LRS Geom Segment. |
| `public static SqlGeometry InterpolateBetweenGeom(SqlGeometry startPoint, SqlGeometry endPoint, double measure)` | Find the point with specified measure, going from the start point in the direction of the end point. |
| `public static SqlBoolean IsConnected(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = 0.5)` | Checks if two geometric segments are spatially connected. |
| `public static SqlBoolean IsValidPoint(SqlGeometry geometry)` | Checks if an LRS point is valid. |
| `public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double measure)` | Locate the Geometry Point along the specified measure on the Geometry. |
| `public static SqlGeometry MergeAndResetGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = 0.5)` | Merges connected segments within tolerance and repopulates measures starting at zero. |
| `public static SqlGeometry MergeGeometrySegments(SqlGeometry geometry1, SqlGeometry geometry2, double tolerance = 0.5)` | Merge two geometry segments to one geometry. |
| `public static SqlGeometry MultiplyGeometryMeasures(SqlGeometry geometry, double multiplyMeasure)` | Multiplies M values on points, linestrings, and multilinestrings. |
| `public static SqlGeometry OffsetGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double offset, double tolerance = 0.5)` | Returns the geometric segment at a specified offset from a geometric segment. |
| `public static SqlGeometry PopulateGeometryMeasures(SqlGeometry geometry, double? startMeasure, double? endMeasure)` | (Re)populate measures across shape points. |
| `public static SqlGeometry ResetMeasure(SqlGeometry geometry)` | Resets Geometry Measure values. |
| `public static SqlGeometry ReverseAndTranslateGeometry(SqlGeometry geometry, double translateMeasure)` | Reverses a linear geometry and translates its measure values. |
| `public static SqlGeometry ReverseLinearGeometry(SqlGeometry geometry)` | Reverses the ordering of a linear geometry. |
| `public static SqlGeometry ScaleGeometrySegment(SqlGeometry geometry, double startMeasure, double endMeasure, double shiftMeasure)` | Rescales a segment's measure interval and applies a measure shift. |
| `public static void SplitGeometrySegment(SqlGeometry geometry, double splitMeasure, out SqlGeometry geometry1, out SqlGeometry geometry2)` | Split a geometry into geometry segments based on split measure. |
| `public static SqlGeometry TranslateMeasure(SqlGeometry geometry, double translateMeasure)` | Adds an offset to the geometry's M values. |
| `public static string ValidateLRSGeometry(SqlGeometry geometry)` | Validates the LRS geometry. |

## Geometry utility functions

Namespace: `SQLSpatialTools.Functions.Util`.

### Geometry

Utility class to manipulate planar Geometry data type.

| Constructor or method | Summary |
| --- | --- |
| `public static SqlGeometry ExtractGeometry(SqlGeometry sqlGeometry, int elementIndex, int ringIndex = 0)` | Extracts the geometry identified by the element index and optional ring index. |
| `public static SqlGeometry PolygonToLine(SqlGeometry geometry)` | Utility method for converting Polygon types to LineString types. |
| `public static SqlGeometry RemoveDuplicateVertices(SqlGeometry geometry, double tolerance = 0.5)` | Utility Method for removing the consecutive duplicate vertices |

## KML entry points and helpers

Namespace: `SQLSpatialTools.KMLProcessor`.

### Constants

KML namespace URIs and the default SRID.

Properties and fields:

```csharp
public static string AtomNamespace { get; }
public static int DefaultSRID { get; }
public static string GxNamespace { get; }
public static string KmlNamespace { get; }
```

### FilterSetSridGeographySink

This class will propagate all method calls to the given target sink, except the method call to the SetSrid method.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public FilterSetSridGeographySink(IGeographySink110 targetSink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | This method call will not be propagated to the target sink |

### GeographyContext

This class contains the information about the context where a geography instance is found.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyContext()` | Constructs this type with the supplied configuration (or its default state). |

Properties and fields:

```csharp
public string Context { get; set; }
public string Id { get; set; }
public SqlGeography Shape { get; set; }
```

### KMLException

Reports failures encountered while parsing or converting KML.

| Constructor or method | Summary |
| --- | --- |
| `public KMLException(string message, Exception innerException)` | Constructs this type with the supplied configuration (or its default state). |
| `public KMLException(string message)` | Constructs this type with the supplied configuration (or its default state). |

### KMLFunctions

This class contains the KML import/export functions

| Constructor or method | Summary |
| --- | --- |
| `public KMLFunctions()` | Constructs this type with the supplied configuration (or its default state). |
| `public static void DecodeGeographyContext(object rowData, out SqlString id, out SqlString context, out SqlGeography shape)` | Converts one imported geography/context record into SQL table-valued-function output columns. |
| `public static SqlString ExportToKml(SqlGeography g)` | This method converts the given geography instance into the KML format |
| `public static SqlString ExportWKTToKml(SqlString wkt)` | This method converts the given geography instance represented as a WKT string, into the string in the KML format. |
| `public static IEnumerable ImportKml(SqlString kml, SqlBoolean makeValid)` | Extracts KML geography records with their identifiers and XML context for table-valued output. |
| `public static SqlGeography KmlToGeography(SqlString kml, SqlBoolean makeValid)` | This function parses the given KML string and returns the extracted geography instance. |

### Utilities

This class contains the utility methods

| Constructor or method | Summary |
| --- | --- |
| `public static double ShiftInRange(double value, double upperBorder)` | This method shifts the given value into the range [-upperBorder, upperBorder] |

## KML export infrastructure

Namespace: `SQLSpatialTools.KMLProcessor.Export`.

### KeyholeMarkupLanguageBase

The base class for all KML exporters

| Constructor or method | Summary |
| --- | --- |
| `protected KeyholeMarkupLanguageBase(XmlWriter writer)` | Constructs this type with the supplied configuration (or its default state). |
| `protected void EndElement()` | This method will close the last opened (and not closed) element. |
| `public void FinalizeKMLDocument()` | Closes the XML elements opened while writing the KML document. |
| `public void SetSrid(int srid)` | This method will set the spatial reference system identifier (SrId) |
| `protected void StartElement(string name)` | This method will start a tag for xml element with the given name |
| `protected void WriteString(string text)` | This method will write the given text into the xml writer |

Properties and fields:

```csharp
protected int SRID
protected readonly XmlWriter Writer
```

## KML parsing and data models

Namespace: `SQLSpatialTools.KMLProcessor.Import`.

### AltitudeMode

Altitude modes allowed in KML

Enum values:

```csharp
Absolute = 2
ClampToGround = 0
ClampToSeaFloor = 3
RelativeToGround = 1
RelativeToSeaFloor = 4
```

### Geography

Base class for all geography instances extracted from the KML file

| Constructor or method | Summary |
| --- | --- |
| `protected Geography()` | Constructs this type with the supplied configuration (or its default state). |
| `public abstract void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |
| `public void Populate(IGeographySink110 sink, bool makeValid)` | Sends this shape to a geography sink, optionally repairing invalid input. |
| `public SqlGeography ToSqlGeography(bool makeValid)` | This method returns the geography instance that corresponds to this KML geography |
| `public override string ToString()` | This method returns a string representation of this object |

Properties and fields:

```csharp
public string Context { get; set; }
public bool Extrude { get; set; }
public string Id { get; set; }
public bool IsValid { get; }
public bool Tessellate { get; set; }
public abstract string WKT { get; }
```

### GroundOverlay

This class holds the information about the ground overlay extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public GroundOverlay()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public Geography Box { set; }
public string Name { get; }
public Region Region { set; }
public override string WKT { get; }
```

### KMLParser

This class parses the given KML string

| Constructor or method | Summary |
| --- | --- |
| `public KMLParser(string kml)` | Constructs this type with the supplied configuration (or its default state). |
| `public static int? GetAltitudeModeCode(XmlElement elem)` | This method determines the altitude mode code for the given KML element. |
| `public static string GetAttribute(XmlElement elem, string attributeName)` | This method returns the attribute value of the given xml element. |
| `public static string GetChildElementText(XmlElement element, string childElementName)` | This method returns the inner text in the child element of the given parent element |
| `public static T GetChildElementText<T>(XmlElement element, string childElementName, T defaultValue)` | Converts child-element text to T, using the supplied default when the element is absent. |
| `public static T GetChildElementText<T>(XmlElement element, string childElementName)` | Converts child-element text to T. |
| `public static XmlElement GetFirstChild(XmlElement element, string childElementName)` | This method returns the first child element with the given name in the given parent element |
| `public static bool GetTessellateFlag(XmlElement elem)` | This method returns the tessellate flag value for the given KML element. |
| `public void Parse()` | This method parses the given KML string |
| `protected static IList<Point> ParseCoordinates(string coordinates)` | This method parses the given coordinates string. |
| `protected void Visit(XmlNode node)` | This method visits the given KML node |
| `protected static void VisitGeography(XmlElement elem, Geography geography)` | Copies common KML shape metadata into a geography model. |
| `protected void VisitGroundOverlay(XmlElement element)` | This method visits a KML Ground Overlay element |
| `protected void VisitLatLonAltBox(XmlElement element)` | This method visits a KML LatLonAltBox element |
| `protected void VisitLatLonBox(XmlElement element)` | This method visits a KML LatLonBox element |
| `protected void VisitLatLonQuad(XmlElement element)` | This method visits a KML LatLonQuad element |
| `protected void VisitLinearRing(XmlElement elem)` | This method visits a KML LinearRing element |
| `protected void VisitLineString(XmlElement elem, bool isLineRing = false)` | This method visits a KML LineString element |
| `protected void VisitLonLatAltElement(XmlElement element)` | Extracts longitude, latitude, altitude, and altitude mode into a point model. |
| `protected void VisitLookAt(XmlElement elem)` | This method visits a LookAt element |
| `protected void VisitModel(XmlElement element)` | This method visits a KML Model element |
| `protected void VisitMultiGeometry(XmlElement element)` | This method visits a KML MultiGeometry element |
| `protected void VisitPlacemark(XmlElement elem)` | This method visits a KML placemark element |
| `protected void VisitPoint(XmlElement elem)` | This method visits a KML Point element |
| `protected void VisitPolygon(XmlElement elem)` | This method visits a KML Polygon element |
| `protected void VisitRegion(XmlElement element)` | This method visits a KML Region element |

Properties and fields:

```csharp
protected readonly XmlDocument Document
public IList<Geography> Geographies { get; protected set; }
public IList<GroundOverlay> GroundOverlays { get; protected set; }
public IList<Model> Models { get; protected set; }
public IList<Placemark> Placemarks { get; protected set; }
public IList<Region> Regions { get; protected set; }
```

### KMLProcessor

This class processes the given KML string and produces the equivalent SqlGeography instance using sinks.

| Constructor or method | Summary |
| --- | --- |
| `public KMLProcessor(string kml)` | Constructs this type with the supplied configuration (or its default state). |
| `public void Populate(IGeographySink110 sink, bool makeValid)` | This method populates the given sink with the information about the geography instance extracted from the KML string |

Properties and fields:

```csharp
public IEnumerable<Geography> Geographies { get; }
public IList<GroundOverlay> GroundOverlays { get; }
public IList<Model> Models { get; }
public IList<Placemark> Placemarks { get; }
public IList<Region> Regions { get; }
public int Srid { get; set; }
```

### LatLonAltBox

This class contains the information about the LatLonAltBox element extracted from KML

Inherits `SQLSpatialTools.KMLProcessor.Import.LatLonBoxBase`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LatLonAltBox()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public AltitudeMode? AltitudeMode { get; set; }
public double MaxAltitude { get; set; }
public double MinAltitude { get; set; }
```

### LatLonBox

This class holds the information about the LatLonBox instance extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.LatLonBoxBase`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LatLonBox()` | Constructs this type with the supplied configuration (or its default state). |

Properties and fields:

```csharp
public double Rotation { get; set; }
```

### LatLonBoxBase

Base class for KML latitude/longitude boxes, represented spatially as polygons.

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LatLonBoxBase()` | Constructs this type with the supplied configuration (or its default state). |
| `protected void CheckCoordinates()` | Normalizes north/south and east/west coordinate ordering. |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public double? Altitude { set; }
public double East { set; }
public double? Measure { set; }
public double North { set; }
public double South { set; }
public double West { set; }
public override string WKT { get; }
```

### LatLonQuad

This class holds the information about a LatLonQuad instance extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LatLonQuad()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public List<Point> Points { get; }
public override string WKT { get; }
```

### LinearRing

This class contains the data about a linear ring extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.LineString`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LinearRing()` | Constructs this type with the supplied configuration (or its default state). |
| `public void SwitchOrientation()` | This method switches the ring's orientation |

Properties and fields:

```csharp
public override string WKT { get; }
```

### LineString

This class contains the information about a line string extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public LineString()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |
| `public void StoreTessellateFlag()` | Encodes the tessellation/clamp-to-ground information in the points' M values. |

Properties and fields:

```csharp
protected OpenGisGeographyType GeographyType
public List<Point> Points { get; }
public string Vertices { get; }
public override string WKT { get; }
```

### Model

This class holds the information about a KML Model element

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public Model()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public Point Location { set; }
public double OrientationHeading { get; set; }
public double OrientationRoll { get; set; }
public double OrientationTilt { get; set; }
public double ScaleX { get; set; }
public double ScaleY { get; set; }
public double ScaleZ { get; set; }
public override string WKT { get; }
```

### MultiGeometry

This class holds the information about a collection of the KML geographies

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public MultiGeometry()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the information about this multi geometry instance |

Properties and fields:

```csharp
public IList<Geography> Geographies { get; set; }
public override string WKT { get; }
```

### Placemark

This class contains the information about a placemark extracted from the KML file

| Constructor or method | Summary |
| --- | --- |
| `public Placemark()` | Constructs this type with the supplied configuration (or its default state). |

Properties and fields:

```csharp
public string Address { get; set; }
public string Description { get; set; }
public Geography Geography { get; set; }
public string Id { get; set; }
public Geography LookAt { get; set; }
public string Name { get; set; }
```

### Point

This class holds the information about a Point extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public Point()` | Constructs this type with the supplied configuration (or its default state). |
| `public Point(double longitude, double latitude, double? altitude = null, double? measure = null)` | Constructs this type with the supplied configuration (or its default state). |
| `public Point Clone()` | This method clones this point |
| `public override bool Equals(object obj)` | This method compares the given object with this point |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data about this geography instance |

Properties and fields:

```csharp
public double? Altitude { get; set; }
public string Coordinate { get; }
public double Latitude { get; set; }
public double Longitude { get; set; }
public double? Measure { get; set; }
public override string WKT { get; }
```

### Polygon

This class contains the information about a polygon extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public Polygon()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public IList<LinearRing> InnerRing { get; }
public LinearRing OuterRing { get; set; }
public override string WKT { get; }
```

### Region

This class holds the information about a Region extracted from the KML file

Inherits `SQLSpatialTools.KMLProcessor.Import.Geography`; its inherited API remains available.

| Constructor or method | Summary |
| --- | --- |
| `public Region()` | Constructs this type with the supplied configuration (or its default state). |
| `public override void Populate(IGeographySink110 sink)` | This method populates the given sink with the data from this geography instance |

Properties and fields:

```csharp
public Geography Box { set; }
public override string WKT { get; }
```

## Geography event sinks

Namespace: `SQLSpatialTools.Sinks.Geography`.

### DensifyGeographySink

Inserts intermediate vertices in a geography stream to limit angular separation along great-circle arcs.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public DensifyGeographySink(IGeographySink110 sink, double angle)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeographyEmptyShapeFilter

Removes empty shapes from a geography event stream.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyEmptyShapeFilter(IGeographySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeographyPointFilter

Removes point components from a geography event stream.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyPointFilter(IGeographySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeographyShortLineStringFilter

Removes geography linestrings shorter than a supplied tolerance.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyShortLineStringFilter(IGeographySink110 sink, double tolerance)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeographyThinRingFilter

Filters thin geography polygon rings using an area-to-length tolerance.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeographyThinRingFilter(IGeographySink110 sink, double tolerance)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### Projector

Projects geography events into planar geometry using a supplied map projection.

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public Projector(SqlProjection projection, IGeometrySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### VacuousGeographyToGeometrySink

This class implements a completely trivial conversion from geography to geometry, simply taking each point(lat, long) --> (y, x).

Implements: `Microsoft.SqlServer.Types.IGeographySink110`, `Microsoft.SqlServer.Types.IGeographySink`.

| Constructor or method | Summary |
| --- | --- |
| `public VacuousGeographyToGeometrySink(int targetSrid, IGeometrySink110 target)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double latitude, double longitude, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double latitude, double longitude, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeography(OpenGisGeographyType type)` | Begins a geography shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeography()` | Completes the current geography shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

## Geometry event sinks

Namespace: `SQLSpatialTools.Sinks.Geometry`.

### GeometryEmptyShapeFilter

Removes empty shapes from a geometry event stream.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryEmptyShapeFilter(IGeometrySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeometryPointFilter

Removes point components from a geometry event stream.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryPointFilter(IGeometrySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeometryShortLineStringFilter

Removes planar linestrings shorter than a supplied tolerance.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryShortLineStringFilter(IGeometrySink110 sink, double tolerance)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeometryThinRingFilter

Filters thin planar polygon rings using an area-to-length tolerance.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryThinRingFilter(IGeometrySink110 sink, double tolerance)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeometryToPointGeographySink

Sink which extracts points from a geometry instance and forwards them to a geography sink.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryToPointGeographySink(IGeographySink110 sink)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### GeometryTransformer

Applies an affine transform while forwarding geometry events to another sink.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public GeometryTransformer(IGeometrySink110 sink, AffineTransform transform)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### ShiftGeometrySink

This class implements a geometry sink that will shift an input geometry by a given amount in the x and y directions.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public ShiftGeometrySink(double xShift, double yShift, IGeometrySink110 target)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### UnProjector

Converts projected geometry events back to geography through a SqlProjection.

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public UnProjector(SqlProjection projection, IGeographySink110 sink, int newSrid)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

### VacuousGeometryToGeographySink

This class implements a completely trivial conversion from geometry to geography, simply taking each point(x, y) --> (long, lat).

Implements: `Microsoft.SqlServer.Types.IGeometrySink110`, `Microsoft.SqlServer.Types.IGeometrySink`.

| Constructor or method | Summary |
| --- | --- |
| `public VacuousGeometryToGeographySink(int targetSrid, IGeographySink110 target)` | Constructs this type with the supplied configuration (or its default state). |
| `public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)` | Handles a circular-arc event in this sink's spatial pipeline. |
| `public void AddLine(double x, double y, double? z, double? m)` | Handles the next line vertex in this sink's spatial pipeline. |
| `public void BeginFigure(double x, double y, double? z, double? m)` | Begins a figure at the supplied coordinate. |
| `public void BeginGeometry(OpenGisGeometryType type)` | Begins a geometry shape of the supplied type. |
| `public void EndFigure()` | Completes the current figure. |
| `public void EndGeometry()` | Completes the current geometry shape. |
| `public void SetSrid(int srid)` | Handles the spatial reference identifier for this sink. |

## SQL CLR user-defined types

Namespace: `SQLSpatialTools.Types.SQL`.

### AffineTransform

SQL CLR-compatible planar affine transform: translation, rotation, scaling, and text/binary serialization.

Implements: `System.Data.SqlTypes.INullable`, `Microsoft.SqlServer.Server.IBinarySerialize`.

| Constructor or method | Summary |
| --- | --- |
| `public AffineTransform()` | Constructs this type with the supplied configuration (or its default state). |
| `public SqlGeometry Apply(SqlGeometry geometry)` | Applies this affine transform to a geometry. |
| `public double GetX(double x, double y)` | Calculates the transformed X coordinate. |
| `public double GetY(double x, double y)` | Calculates the transformed Y coordinate. |
| `public static AffineTransform Parse(SqlString str)` | Reconstructs the value from its textual representation. |
| `public void Read(BinaryReader r)` | Restores the value or aggregate state from binary serialization. |
| `public static AffineTransform Rotate(double angleDeg)` | Creates a planar rotation transform using an angle in degrees. |
| `public static AffineTransform Scale(double sx, double sy)` | Creates a transform with independent X and Y scale factors. |
| `public override string ToString()` | Returns the value's textual representation. |
| `public static AffineTransform Translate(double cx, double cy)` | Creates a transform with X and Y translation offsets. |
| `public void Write(BinaryWriter binaryWriter)` | Writes the value or aggregate state to binary serialization. |

Properties and fields:

```csharp
public bool IsNull { get; }
public static AffineTransform Null { get; }
```

### SqlProjection

SQL CLR-compatible map projection with factories for seven projection families and forward/inverse conversion.

Implements: `System.Data.SqlTypes.INullable`, `Microsoft.SqlServer.Server.IBinarySerialize`.

| Constructor or method | Summary |
| --- | --- |
| `public SqlProjection()` | Constructs this type with the supplied configuration (or its default state). |
| `public static SqlProjection AlbersEqualArea(double longitude0, double latitude0, double parallel1, double parallel2)` | Creates an Albers equal-area projection. |
| `public static SqlProjection Equirectangular(double longitude0, double parallel)` | Creates an equirectangular projection. |
| `public static SqlProjection Gnomonic(double longitude, double latitude)` | Creates a gnomonic projection centered on the supplied location. |
| `public static SqlProjection LambertConformalConic(double longitude0, double latitude, double fi1, double fi2)` | Creates a Lambert conformal conic projection. |
| `public static SqlProjection Mercator(double longitude0)` | Creates a Mercator projection. |
| `public static SqlProjection ObliqueMercator(double longitude0, double fi1, double lambda1, double fi2, double lambda2)` | Creates an oblique Mercator projection. |
| `public static SqlProjection Parse(SqlString s)` | Reconstructs the value from its textual representation. |
| `public SqlGeometry Project(SqlGeography geography)` | Projects a geography into planar geometry. |
| `public void ProjectPoint(double latitudeDeg, double longitudeDeg, out double x, out double y)` | Projects latitude/longitude in degrees to planar X/Y. |
| `public void Read(BinaryReader r)` | Restores the value or aggregate state from binary serialization. |
| `public override string ToString()` | Returns the value's textual representation. |
| `public static SqlProjection TransverseMercator(double longitude0)` | Creates a transverse Mercator projection. |
| `public SqlGeography Unproject(SqlGeometry geometry)` | Converts projected geometry back to geography. |
| `public void UnprojectPoint(double x, double y, out double latitudeDeg, out double longitudeDeg)` | Converts planar X/Y to latitude/longitude in degrees. |
| `public SqlGeography UnprojectWithSRID(SqlGeometry geometry, SqlInt32 newSrid)` | Converts projected geometry back to geography with an explicit output SRID. |
| `public void Write(BinaryWriter w)` | Writes the value or aggregate state to binary serialization. |

Properties and fields:

```csharp
public bool IsNull { get; }
public static SqlProjection Null { get; }
```

## Shared utilities and enums

Namespace: `SQLSpatialTools.Utility`.

### Constants

Shared spatial defaults: SRID, tolerance, and point-text format.

Properties and fields:

```csharp
public const int DefaultSRID = 4326
public const string PointSqlCharFormat = "POINT({0} {1} {2} {3})"
public const double Tolerance = 0.5
```

### DimensionalInfo

Identifies XY, XYM, XYZ, and XYZM coordinate layouts.

Enum values:

```csharp
Dim2D = 1
Dim2DWithMeasure = 2
Dim3D = 3
Dim3DWithMeasure = 4
None = 0
```

### ErrorMessage

Named error-message constants used by geometry and LRS validation.

Properties and fields:

```csharp
public const string DistanceMustBeBetweenTwoPoints = "The distance value provided exceeds the distance between the two points."
public const string DistanceMustBePositive = "The distance must be positive."
public const string InvalidElementIndex = "Invalid index for element to be extracted."
public const string InvalidGeometry = "Invalid geometry"
public const string InvalidSubElementIndex = "Invalid index for sub-element to be extracted."
public const string LinearGeometryMeasureMustBeInRange = "{0} is not within the measure range {1} : {2} of the linear geometry."
public const string LinearMeasureRange = "The given measure for linear referencing was out of range."
public const string LineOrMultiLineStringCompatible = "LINESTRING or MULTILINE STRING is currently the only spatial type supported."
public const string LineOrPointCompatible = "LINESTRING or POINT is currently the only spatial type supported."
public const string LineStringCompatible = "LINESTRING is currently the only spatial type supported."
public const string LRSCompatible = "POINT, LINESTRING or MULTILINE STRING is currently the only spatial type supported."
public const string MeasureRange = "Measure not within range."
public const string MultiLineStringCompatible = "MULTILINE STRING is currently the only spatial type supported."
public const string PointCompatible = "Start and End geometry must be a point."
public const string SRIDCompatible = "SRID's of geography\\geometry objects doesn't match."
public const string TwoDimensionalCoordinates = "Cannot operate on 2 Dimensional co-ordinates without measure values."
public const string WKT3DOnly = "Input WKT should only have three dimensions!"
```

### LinearMeasureProgress

Progression of Geom Segment based on measure.

Enum values:

```csharp
Decreasing = 2
Increasing = 1
None = 0
```

### LRSErrorCodes

LRS validity and connectivity status codes.

Enum values:

```csharp
InvalidLRS = 13331
InvalidLRSMeasure = 13333
InvalidLRSPoint = 13332
LRSSegmentNotConnected = 13334
LRSSegmentNotDefined = 13335
ValidLRS = 1
```

### MergeInputType

Merge Input Segments Geometry type.

Enum values:

```csharp
LSLS = 1
LSMLS = 2
MLSLS = 3
MLSMLS = 4
None = 0
```

### MergePosition

Merging position of Geom Segments

Enum values:

```csharp
BothEnds = 5
CrossEnds = 6
EndEnd = 4
EndStart = 3
None = 0
StartEnd = 2
StartStart = 1
```

### OGCType

Enum for Open Geo spatial Consortium Type Names

Enum values:

```csharp
CircularString = 2
CompoundCurve = 3
CurvePolygon = 5
GeometryCollection = 6
LineString = 1
MultiLineString = 8
MultiPoint = 7
MultiPolygon = 9
Point = 0
Polygon = 4
```

### SlopeValue

Special slope classifications used by spatial calculations.

Enum values:

```csharp
NegativeInfinity = 4
NegativeZero = 2
None = 0
PositiveInfinity = 3
PositiveZero = 1
```

### SpatialExtensions

Spatial type checks, WKT conversion, coordinate/measure access, tolerance comparisons, and LRS validation helpers.

| Constructor or method | Summary |
| --- | --- |
| `public static bool CheckGeomPoint(this SqlGeometry sqlGeometry)` | Identifies point geometry through its spatial event stream, including inputs with invalid coordinates. |
| `public static bool Compare(this SqlString sqlString, string targetString)` | Compares sql string for equality. |
| `public static bool Contains(this OpenGisGeometryType type, OpenGisGeometryType[] geometryTypes)` | Returns true if the input type is in the list of supported types. |
| `public static bool EqualsTo(this double left, double right)` | Checks if two double values are equal. |
| `public static bool EqualsTo(this double left, double? right)` | Checks if two double values are equal. |
| `public static bool EqualsTo(this double? left, double right)` | Checks if two double values are equal. |
| `public static bool EqualsTo(this double? left, double? right)` | Checks if two double values are equal. |
| `public static DimensionalInfo GetDimension(this string wktGeometry)` | Determines the coordinate layout represented by a WKT string. |
| `public static double GetDistance(double x1, double y1, double x2, double y2)` | Gets the distance between two x,y co-ordinates. |
| `public static double GetEndPointMeasure(this SqlGeometry sqlGeometry)` | Get end point measure of Geometry |
| `public static SqlGeography GetGeog(this string geogString, int srid = 4326)` | Convert WKT string to SqlGeography object. |
| `public static SqlGeometry GetGeom(this string geomWKT, int srid = 4326)` | Converts WKT string to SqlGeometry object. |
| `public static SqlGeometry GetGeom(this string geomWKT, SqlInt32 srid)` | Converts WKT string to SqlGeometry object. |
| `public static string GetGeomText(this string geomText)` | Extracts the geometry type name from WKT text. |
| `public static string GetLineWKTFromCurvePolygon(this SqlGeometry geometry)` | Converts CurvePolygon to Line representation. |
| `public static string GetLineWKTFromMultiPolygon(this SqlGeometry geometry)` | Rewrites multipolygon WKT into multilinestring WKT through text replacement. |
| `public static string GetLineWKTFromPolygon(this SqlGeometry geometry)` | Converts Polygon to Multi Linestring or LineString Does only based on string matching and replacement. |
| `public static MergeInputType GetMergeType(this SqlGeometry geometry1, SqlGeometry geometry2)` | Classifies a merge by the input linestring/multilinestring combination. |
| `public static double? GetOffset(this SqlGeometry sqlGeometry1, SqlGeometry sqlGeometry2)` | Calculates the difference between the first geometry's end measure and the second geometry's start measure. |
| `public static SqlGeometry GetPoint(double x, double y, double? z, double? m, int srid = 4326)` | Construct SqlGeometry Point from x, y, z, m values |
| `public static SqlGeometry GetPoint(SqlGeometry sqlGeometry)` | Reconstructs a point using the input point's coordinates, measures, and SRID. |
| `public static double GetPointOffset(this SqlGeometry point1, SqlGeometry point2)` | Calculates the measure difference between two points. |
| `public static SqlGeometry GetPointWithUpdatedM(SqlGeometry sqlGeometry, double updatedMeasure)` | Reconstructs a point with a replacement M value. |
| `public static int GetPrecisionLength(this double decimalDigit)` | Returns the formatted fractional-part length, including the decimal point when present. |
| `public static double GetStartPointMeasure(this SqlGeometry sqlGeometry)` | Get start point measure of Geometry |
| `public static string GetString(this DimensionalInfo value)` | Returns the enum value's StringValueAttribute text. |
| `public static string GetString(this LRSErrorCodes value)` | Returns the enum value's StringValueAttribute text. |
| `public static string GetString(this OGCType value)` | Returns the enum value's StringValueAttribute text. |
| `public static bool IsCircularString(this SqlGeometry sqlGeometry)` | Check if Geometry is CircularString |
| `public static bool IsCompoundCurve(this SqlGeometry sqlGeometry)` | Check if Geometry is CompoundCurve |
| `public static bool IsCurvePolygon(this SqlGeometry sqlGeometry, bool checkValid = true)` | Check if Geometry is CurvePolygon |
| `public static bool IsCurvePolygon(this SqlGeometry sqlGeometry)` | Check if Geometry is CurvePolygon |
| `public static bool IsExtremeMeasuresMatch(double geomStartMeasure, double geomEndMeasure, double inputStartMeasure, double inputEndMeasure)` | Determines whether the input start or end measure matches the start or end point measure of geometry. |
| `public static bool IsGeometryCollection(this SqlGeometry sqlGeometry)` | Check if Geometry is GeometryCollection |
| `public static bool IsLineString(this SqlGeometry sqlGeometry)` | Check if Geometry is LineString |
| `public static bool IsLRSType(this SqlGeometry geometry)` | Check if the geometry collection is of POINT, LINESTRING, MULTILINESTRING. |
| `public static bool IsMultiLineString(this SqlGeometry sqlGeometry)` | Check if Geometry is MultiLineString |
| `public static bool IsMultiPoint(this SqlGeometry sqlGeometry)` | Check if Geometry is MultiPoint |
| `public static bool IsMultiPolygon(this SqlGeometry sqlGeometry, bool checkValid = true)` | Check if Geometry is MultiPolygon |
| `public static bool IsMultiPolygon(this SqlGeometry sqlGeometry)` | Check if Geometry is MultiPolygon |
| `public static bool IsNotNullOrEmpty(this SqlGeometry sqlGeometry)` | Determines whether the geometry segment is not null or empty. |
| `public static bool IsNullOrEmpty(this SqlGeometry sqlGeometry)` | Determines whether the geometry segment is null or empty. |
| `public static bool IsOfSupportedTypes(this SqlGeometry geometry, OpenGisGeometryType supportedType, params OpenGisGeometryType[] additionalSupportedTypes)` | Check if the input geometry is of the supported specified type |
| `public static bool IsPoint(this SqlGeography sqlGeography)` | Check if Geography type is Point |
| `public static bool IsPoint(this SqlGeometry sqlGeometry)` | Check if Geometry is Point |
| `public static bool IsPoint(this string wktGeometry)` | Check if Geometry is Point; Works on invalid types too. |
| `public static bool IsPolygon(this SqlGeometry sqlGeometry, bool checkValid = true)` | Check if Geometry is Polygon |
| `public static bool IsPolygon(this SqlGeometry sqlGeometry)` | Check if Geometry is Polygon |
| `public static bool IsTolerable(this double distance, double tolerance)` | Determines whether the specified distance is within tolerance. |
| `public static bool IsWithinRange(this double currentMeasure, double startMeasure, double endMeasure)` | Checks whether the measure lies between the supplied endpoint measures. |
| `public static bool IsWithinRange(this double currentMeasure, SqlGeometry sqlGeometry)` | Checks whether the measure lies within the geometry's endpoint measures. |
| `public static bool IsWithinRange(this double currentMeasure, SqlGeometry startPoint, SqlGeometry endPoint)` | Check whether the measure falls between start and end geometry points |
| `public static bool IsWithinRange(this double? currentMeasure, double? startMeasure, double? endMeasure)` | Checks whether a nullable measure lies between the supplied nullable endpoint measures. |
| `public static bool IsWithinTolerance(double x1, double y1, double x2, double y2, double tolerance)` | Determines whether the x y distance between 2 x,y points is within tolerance |
| `public static bool IsWithinTolerance(this SqlGeometry startPoint, SqlGeometry endPoint, double tolerance)` | Determines whether the x y distance between start and point is tolerable. |
| `public static bool IsXYWithinRange(double x1, double y1, double x2, double y2, double tolerance = 0)` | Checks whether both X and Y coordinate differences are within tolerance. |
| `public static bool IsXYWithinRange(this SqlGeometry sourcePoint, SqlGeometry pointToCompare, double tolerance = 0)` | Checks whether difference of X and Y point between source and point to compare is within tolerable range |
| `public static bool NotEqualsTo(this double left, double right)` | Checks if two double values are not equal. |
| `public static DimensionalInfo STGetDimension(this SqlGeometry sqlGeometry)` | Gets the dimension info of input geometry |
| `public static bool STHasEqualStartAndEndMeasure(this SqlGeometry geometry)` | To check whether start measure value is equal to End Measure value |
| `public static bool STHasLinearMeasure(this SqlGeometry geometry)` | Checks whether the geometry's measure progression is increasing or decreasing. |
| `public static LinearMeasureProgress STLinearMeasureProgress(this SqlGeometry geometry)` | Get the linear progression of the geometry based on measure value; whether increasing or decreasing. |
| `public static bool STSameDirection(this SqlGeometry geometrySegment1, SqlGeometry geometrySegment2)` | Checks if both the segments are in same direction. |
| `public static void ThrowException(string messageFormat, params object[] args)` | Throws ArgumentException based on message format and parameters |
| `public static void ThrowLRSError(LRSErrorCodes lrsErrorCode)` | Throws LRS Error based on error code. |
| `public static short Value(this LinearMeasureProgress linearMeasureProgress)` | Get the int value of enum |
| `public static string Value(this LRSErrorCodes lrsErrorCodes)` | Get the string value for the specified LRS Error Code. |

### StringValueAttribute

This attribute is used to represent a string value for a value in an enum.

| Constructor or method | Summary |
| --- | --- |
| `public StringValueAttribute(string value)` | Constructs this type with the supplied configuration (or its default state). |

Properties and fields:

```csharp
public string StringValue { get; }
```
