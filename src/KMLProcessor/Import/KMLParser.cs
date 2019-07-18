//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
// ReSharper disable MemberCanBePrivate.Global

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class parses the given KML string
    /// </summary>
    public class KMLParser
    {
        #region Constructors

        /// <summary>
        /// Constructor. Accepts the string in the KML format.
        /// </summary>
        /// <param name="kml">KML string to be parsed</param>
        public KMLParser(string kml)
        {
            Document = new XmlDocument();
            Document.LoadXml(kml);
        }

        #endregion

        #region Public Data

        /// <summary>
        /// Geography instances extracted from the KML string
        /// </summary>
        public IList<Geography> Geographies { protected set; get; } = new List<Geography>();

        /// <summary>
        /// Placemarks extracted from the KML string
        /// </summary>
        public IList<Placemark> Placemarks { protected set; get; } = new List<Placemark>();

        /// <summary>
        /// Models extracted from the KML string
        /// </summary>
        public IList<Model> Models { protected set; get; } = new List<Model>();

        /// <summary>
        /// Regions extracted from the KML string
        /// </summary>
        public IList<Region> Regions { protected set; get; } = new List<Region>();

        /// <summary>
        /// Ground overlays extracted from the KML string
        /// </summary>
        public IList<GroundOverlay> GroundOverlays { protected set; get; } = new List<GroundOverlay>();

        #endregion

        #region Protected Data

        /// <summary>
        /// Xml document whose data will be traversed
        /// </summary>
        protected readonly XmlDocument Document;

        #endregion

        #region Protected Methods

        /// <summary>
        /// This method visits the given KML node
        /// </summary>
        /// <param name="node">KML node to be visited</param>
        protected void Visit(XmlNode node)
        {
            if (node == null) return;

            var nodeName = node.Name.ToLowerInvariant();

            if (node is XmlElement elem)
            {
                if (nodeName.Equals("point"))
                {
                    VisitPoint(elem);
                }
                else if (nodeName.Equals("linestring"))
                {
                    VisitLineString(elem);
                }
                else if (nodeName.Equals("linearring"))
                {
                    VisitLinearRing(elem);
                }
                else if (nodeName.Equals("polygon"))
                {
                    VisitPolygon(elem);
                }
                else if (nodeName.Equals("multigeometry"))
                {
                    VisitMultiGeometry(elem);
                }
                else if (nodeName.Equals("placemark"))
                {
                    VisitPlacemark(elem);
                }
                else if (nodeName.Equals("model"))
                {
                    VisitModel(elem);
                }
                else if (nodeName.Equals("region"))
                {
                    VisitRegion(elem);
                }
                else if (nodeName.Equals("groundoverlay"))
                {
                    VisitGroundOverlay(elem);
                }
                else
                {
                    // Visits the child nodes
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        Visit(child);
                    }
                }
            }
        }

        /// <summary>
        /// This method visits a KML Ground Overlay element
        /// </summary>
        /// <param name="element">KML Ground Overlay element</param>
        protected void VisitGroundOverlay(XmlElement element)
        {
            if (element == null) return;

            var groundOverlay = new GroundOverlay();

            VisitGeography(element, groundOverlay);
            groundOverlay.Name = GetChildElementText(element, "name");

            var latLonAltBoxElem = GetFirstChild(element, "LatLonAltBox");
            var latLonBoxElem = GetFirstChild(element, "LatLonBox");
            var latLonQuadElem = GetFirstChild(element, "gx:LatLonQuad");

            var numOfGeographiesBefore = Geographies.Count;

            if (latLonAltBoxElem != null)
            {
                VisitLatLonAltBox(latLonAltBoxElem);
            }
            else if (latLonBoxElem != null)
            {
                VisitLatLonBox(latLonBoxElem);
            }
            else if (latLonQuadElem != null)
            {
                VisitLatLonQuad(latLonQuadElem);
            }

            var numOfGeographiesAfter = Geographies.Count;

            if (numOfGeographiesAfter == numOfGeographiesBefore + 1)
            {
                groundOverlay.Box = Geographies[Geographies.Count - 1];
                Geographies.RemoveAt(Geographies.Count - 1);
            }
            else if (numOfGeographiesAfter > numOfGeographiesBefore + 1)
            {
                // Multiple geography instances found in a single ground overlay. 
                // The last geography instance is the border.

                groundOverlay.Box = Geographies[Geographies.Count - 1];
                Geographies.RemoveAt(Geographies.Count - 1);
            }

            #region Check if a region is also defined inside this ground overlay

            var regionElem = GetFirstChild(element, "Region");
            if (regionElem != null)
            {
                var numOfRegionsBefore = Regions.Count;

                VisitRegion(regionElem);

                var numOfRegionsAfter = Regions.Count;

                if (numOfRegionsAfter == numOfRegionsBefore + 1)
                {
                    groundOverlay.Region = Regions[numOfRegionsAfter - 1];
                }
                else if (numOfRegionsAfter > numOfRegionsBefore + 1)
                {
                    // Multiple regions found in a single ground overlay. The last region is the border.

                    groundOverlay.Region = Regions[numOfRegionsAfter - 1];
                }
            }

            #endregion

            GroundOverlays.Add(groundOverlay);
        }

        /// <summary>
        /// This method visits a KML placemark element
        /// </summary>
        /// <param name="elem">KML placemark element</param>
        protected void VisitPlacemark(XmlElement elem)
        {
            if (elem == null) return;

            var placemark = new Placemark();

            placemark.Id = GetAttribute(elem, "id");

            // Visits the child nodes 
            foreach (XmlNode child in elem.ChildNodes)
            {
                var childName = child.Name.ToLowerInvariant();
                if (childName.Equals("name"))
                {
                    if (child.HasChildNodes)
                    {
                        placemark.Name = child.ChildNodes[0].InnerText;
                    }
                }
                else if (childName.Equals("description"))
                {
                    if (child.HasChildNodes)
                    {
                        placemark.Description = child.ChildNodes[0].InnerText;
                    }
                }
                else if (childName.Equals("address"))
                {
                    if (child.HasChildNodes)
                    {
                        placemark.Address = child.ChildNodes[0].InnerText;
                    }
                }
                else if (childName.Equals("lookat") && (child as XmlElement) != null)
                {
                    var numOfGeographiesBefore = Geographies.Count;

                    VisitLookAt(child as XmlElement);

                    var numOfGeographiesAfter = Geographies.Count;

                    if (numOfGeographiesAfter > numOfGeographiesBefore)
                    {
                        // Point found where this placemark looks at

                        placemark.LookAt = Geographies[Geographies.Count - 1];
                        Geographies.RemoveAt(Geographies.Count - 1);
                    }
                }
                else
                {
                    var numOfGeographiesBefore = Geographies.Count;

                    Visit(child);

                    var numOfGeographiesAfter = Geographies.Count;

                    if (numOfGeographiesAfter > numOfGeographiesBefore)
                    {
                        // Found the geography instance which describes this placemark

                        placemark.Geography = Geographies[Geographies.Count - 1];
                    }
                }
            }

            Placemarks.Add(placemark);
        }

        /// <summary>
        /// This method visits a LookAt element
        /// </summary>
        /// <param name="elem">KML lookat element to be visited</param>
        protected void VisitLookAt(XmlElement elem)
        {
            if (elem == null) return;

            VisitLonLatAltElement(elem);
        }

        /// <summary>
        /// This method parses the given coordinates string.
        /// </summary>
        /// <param name="coordinates">String with coordinates to be parsed</param>
        /// <returns>List of points contained in the given string</returns>
        protected static IList<Point> ParseCoordinates(string coordinates)
        {
            var points = new List<Point>();

            if (string.IsNullOrEmpty(coordinates) || coordinates.Trim().Length == 0)
                return points;

            // Removes the spaces inside the tuples
            while (coordinates.Contains(", "))
            {
                coordinates = coordinates.Replace(", ", ",");
            }

            // Splits the tuples
            var coordinate = coordinates.Trim().Split(' ', '\t', '\n');

            // Process each tuple
            foreach (var p in coordinate)
            {
                var tp = p.Trim();
                if (tp.Length == 0)
                    continue;

                var cords = tp.Split(',');

                try
                {
                    var longitude = double.Parse(cords[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    var latitude = double.Parse(cords[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    double? altitude = null;

                    if (cords.Length == 3)  // the altitude is optional
                    {
                        altitude = double.Parse(cords[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    var point = new Point {Longitude = longitude, Latitude = latitude, Altitude = altitude};

                    // If point is the same as the previous point then skip it
                    if (points.Count > 0 && points[points.Count - 1].Equals(point))
                        continue;

                    points.Add(point);
                }
                catch (Exception exc)
                {
                    throw new KMLException("Coordinate is in wrong format: " + tp, exc);
                }
            }

            return points;
        }

        /// <summary>
        /// This method performs the visit which is common to all geography instances.
        /// It stores extracted information into the given geography object.
        /// </summary>
        /// <param name="elem">Xml element to be visited</param>
        /// <param name="geography">Geography object where data should be stored</param>
        protected static void VisitGeography(XmlElement elem, Geography geography)
        {
            #region The id in the KML string

            geography.Id = GetAttribute(elem, "id");

            #endregion

            #region The extrude flag

            var extrudeElem = GetFirstChild(elem, "extrude");
            if (extrudeElem != null && extrudeElem.HasChildNodes)
            {
                var val = extrudeElem.ChildNodes[0].InnerText;
                if (val.Trim().Equals("1"))
                {
                    geography.Extrude = true;
                }
            }

            #endregion

            #region The tessellate flag

            var tessellateElem = GetFirstChild(elem, "tessellate");
            if (tessellateElem != null && tessellateElem.HasChildNodes)
            {
                var val = tessellateElem.ChildNodes[0].InnerText;
                if (val.Trim().Equals("1"))
                {
                    geography.Tessellate = true;
                }
            }

            #endregion
        }

        /// <summary>
        /// This method visits a KML Point element
        /// </summary>
        /// <param name="elem">KML Point element to be visited</param>
        protected void VisitPoint(XmlElement elem)
        {
            if (elem == null) return;

            // Extracts the point coordinates
            var coordinates = GetFirstChild(elem, "coordinates");
            if (coordinates == null || !coordinates.HasChildNodes)
                return;

            var coordinatesTextNode = coordinates.ChildNodes[0];

            var coordinatesText = coordinatesTextNode.InnerText;

            var pts = ParseCoordinates(coordinatesText);

            if (pts.Count == 0)
                return;

            var point = pts[0];

            // Extracts the altitude mode
            point.Measure = GetAltitudeModeCode(elem);

            // Extracts the other non-geographic data
            VisitGeography(elem, point);

            // Sets the geography instance context
            point.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;

            Geographies.Add(point);
        }

        /// <summary>
        /// This method visits a KML LineString element
        /// </summary>
        /// <param name="elem">KML line string element to be visited</param>
        /// <param name="isLineRing">True if the line is the ring</param>
        protected void VisitLineString(
            XmlElement elem,
            bool isLineRing = false)
        {
            if (elem == null) return;

            XmlNode coordinates = GetFirstChild(elem, "coordinates");
            if (coordinates == null || !coordinates.HasChildNodes)
                return;

            var txt = coordinates.ChildNodes[0];

            var allCoordinates = txt.InnerText;

            var points = ParseCoordinates(allCoordinates);
            if (points == null || points.Count == 0)
                return;

            var ls = isLineRing ? new LinearRing() : new LineString();

            ls.Points.AddRange(points);

            var altitudeModeCode = GetAltitudeModeCode(elem);

            foreach (var p in ls.Points)
            {
                p.Measure = altitudeModeCode;
            }

            // Extract other non-geographic data
            VisitGeography(elem, ls);

            #region Handle tessellate flag

            ls.StoreTessellateFlag();

            #endregion

            // Sets the geography instance context
            ls.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;
            
            Geographies.Add(ls);
        }

        /// <summary>
        /// This method visits a KML LinearRing element
        /// </summary>
        /// <param name="elem">KML LinearRing element to be visited</param>
        protected void VisitLinearRing(XmlElement elem)
        {
            VisitLineString(elem, true);
        }

        /// <summary>
        /// This method visits a KML Polygon element
        /// </summary>
        /// <param name="elem">KML Polygon element to be visited</param>
        protected void VisitPolygon(XmlElement elem)
        {
            if (elem == null) return;

            #region Extract Outer Border

            var outerBoundaryIs = GetFirstChild(elem, "outerBoundaryIs");
            if (outerBoundaryIs == null)
                return;

            var outerLinearRing = GetFirstChild(outerBoundaryIs, "LinearRing");
            if (outerLinearRing == null)
                return;

            var numOfGeographiesBefore = Geographies.Count;

            VisitLinearRing(outerLinearRing);

            var numOfGeographiesAfter = Geographies.Count;

            // Extracts the outer ring
            if (numOfGeographiesAfter == numOfGeographiesBefore)
                throw new KMLException("Outer ring not found!");

            var index = Geographies.Count - 1;
            var g = Geographies[index];
            Geographies.RemoveAt(index);

            if (!(g is LinearRing outerRing))
                throw new KMLException("Outer ring not created!");

            var polygon = new Polygon();

            var isValidPolygon = true;

            #region Check outer ring orientation. Outer boundary should be oriented counter-clockwise

            if (!outerRing.IsValid)
            {
                // Checks if just the ring's orientation is wrong
                outerRing.SwitchOrientation();

                if (!outerRing.IsValid)
                {
                    isValidPolygon = false;
                    outerRing.SwitchOrientation();
                }
            }

            #endregion

            polygon.OuterRing = outerRing;

            #endregion

            #region Extract Inner Borders

            var innerBoundaryIsLst = elem.GetElementsByTagName("innerBoundaryIs");
            foreach (XmlNode innerBorderNode in innerBoundaryIsLst)
            {
                if (!(innerBorderNode is XmlElement innerBoundaryIs))
                    continue;

                var innerLinearRing = GetFirstChild(innerBoundaryIs, "LinearRing");
                if (innerLinearRing == null)
                    continue;

                numOfGeographiesBefore = Geographies.Count;

                VisitLinearRing(innerLinearRing);

                numOfGeographiesAfter = Geographies.Count;

                // Extracts the inner ring
                if (numOfGeographiesBefore == numOfGeographiesAfter)
                    throw new KMLException("Inner ring not found!");

                index = Geographies.Count - 1;
                g = Geographies[index];
                Geographies.RemoveAt(index);

                if (!(g is LinearRing innerRing))
                    throw new KMLException("Inner ring not created!");

                #region Check the ring's orientation. Inner rings should be clockwise oriented

                if (isValidPolygon)
                {
                    if (innerRing.IsValid)
                    {
                        // The inner rings should be in the opposite direction from the outer ring

                        innerRing.SwitchOrientation();
                    }
                }

                #endregion

                polygon.InnerRing.Add(innerRing);
            }

            #endregion

            // Extracts the other non-geographic data
            VisitGeography(elem, polygon);

            #region Extract altitude mode

            var altitudeModeCode = GetAltitudeModeCode(elem);
            if (altitudeModeCode.HasValue)
            {
                // Updates all the points on the outer bound
                if (polygon.OuterRing != null)
                {
                    foreach (var p in polygon.OuterRing.Points)
                    {
                        p.Measure = altitudeModeCode;
                    }
                }

                // Update all the points on all the inner bounds
                if (polygon.InnerRing != null)
                {
                    foreach (var r in polygon.InnerRing)
                    {
                        if (r.Points == null)
                            return;

                        foreach (var p1 in r.Points)
                        {
                            p1.Measure = altitudeModeCode;
                        }
                    }
                }
            }

            #endregion

            #region Handles tessellate flag

            if (polygon.Tessellate)
            {
                polygon.OuterRing?.StoreTessellateFlag();

                if (polygon.InnerRing != null)
                {
                    foreach (var r in polygon.InnerRing)
                        r.StoreTessellateFlag();
                }
            }

            #endregion

            // Sets the geography instance context
            polygon.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;

            Geographies.Add(polygon);
        }

        /// <summary>
        /// This method visits a KML MultiGeometry element
        /// </summary>
        /// <param name="element">KML MultiGeometry element to be visited</param>
        protected void VisitMultiGeometry(XmlElement element)
        {
            if (element == null)
                return;

            var mg = new MultiGeometry();

            if (element.HasChildNodes)
            {
                foreach (XmlNode child in element.ChildNodes)
                {
                    var lastIndexBefore = Geographies.Count - 1;

                    Visit(child);

                    var lastIndex = Geographies.Count - 1;

                    if (lastIndex > lastIndexBefore)
                    {
                        var childGeography = Geographies[lastIndex];
                        Geographies.RemoveAt(lastIndex);
                        mg.Geographies.Add(childGeography);
                    }
                }
            }

            // Sets the geography instance context
            mg.Context = (element.ParentNode != null) ? element.ParentNode.OuterXml : element.OuterXml;

            Geographies.Add(mg);
        }

        /// <summary>
        /// This method visits a KML Region element
        /// </summary>
        /// <param name="element">KML Region element to be visited</param>
        protected void VisitRegion(XmlElement element)
        {
            if (element == null) return;

            var r = new Region();

            VisitGeography(element, r);

            var latLonAltBoxElem = GetFirstChild(element, "LatLonAltBox");
            var latLonBoxElem = GetFirstChild(element, "LatLonBox");
            var latLonQuadElem = GetFirstChild(element, "gx:LatLonQuad");

            var numOfGeographiesBefore = Geographies.Count;

            if (latLonAltBoxElem != null)
            {
                VisitLatLonAltBox(latLonAltBoxElem);
            }
            else if (latLonBoxElem != null)
            {
                VisitLatLonBox(latLonBoxElem);
            }
            else if (latLonQuadElem != null)
            {
                VisitLatLonQuad(latLonQuadElem);
            }

            var numOfGeographiesAfter = Geographies.Count;

            if (numOfGeographiesAfter > numOfGeographiesBefore)
            {
                // Found region's box

                r.Box = Geographies[Geographies.Count - 1];
                Geographies.RemoveAt(Geographies.Count - 1);
            }

            Regions.Add(r);
        }

        /// <summary>
        /// This method visits a KML LatLonAltBox element
        /// </summary>
        /// <param name="element">KML LatLonAltBox element to be visited</param>
        protected void VisitLatLonAltBox(XmlElement element)
        {
            if (element == null) return;

            #region Required properties

            var north = GetChildElementText<double>(element, "north");
            var south = GetChildElementText<double>(element, "south");
            var west = GetChildElementText<double>(element, "west");
            var east = GetChildElementText<double>(element, "east");

            #endregion

            #region Optional properties

            double minAltitude = 0;
            double maxAltitude = 0;
            var minAltitudeS = GetChildElementText(element, "minAltitude");
            if (!string.IsNullOrEmpty(minAltitudeS))
            {
                minAltitude = double.Parse(minAltitudeS);
            }

            var maxAltitudeS = GetChildElementText(element, "maxAltitude");
            if (!string.IsNullOrEmpty(maxAltitudeS))
            {
                maxAltitude = double.Parse(maxAltitudeS);
            }

            var altitudeMode = GetAltitudeMode(element);

            #endregion

            var altBox = new LatLonAltBox
            {
                North = north,
                South = south,
                West = west,
                East = east,
                MinAltitude = minAltitude,
                MaxAltitude = maxAltitude,
                AltitudeMode = altitudeMode
            };

            Geographies.Add(altBox);
        }

        /// <summary>
        /// This method visits a KML LatLonBox element
        /// </summary>
        /// <param name="element">KML LatLonBox element to be visited</param>
        protected void VisitLatLonBox(XmlElement element)
        {
            if (element == null) return;

            #region Required properties

            var north = GetChildElementText<double>(element, "north");
            var south = GetChildElementText<double>(element, "south");
            var west = GetChildElementText<double>(element, "west");
            var east = GetChildElementText<double>(element, "east");

            #endregion

            #region Optional properties

            double rotation = 0;

            var rotationS = GetChildElementText(element, "rotation");
            if (!string.IsNullOrEmpty(rotationS))
            {
                rotation = double.Parse(rotationS);
            }

            #endregion

            var altBox = new LatLonBox
            {
                North = north,
                South = south,
                West = west,
                East = east,
                Rotation = rotation
            };

            Geographies.Add(altBox);
        }

        /// <summary>
        /// This method visits a KML LatLonQuad element
        /// </summary>
        /// <param name="element">LatLonQuad KML element to be visited</param>
        protected void VisitLatLonQuad(XmlElement element)
        {
            if (element == null) return;

            var coordinates = GetChildElementText(element, "coordinates");

            var points = ParseCoordinates(coordinates);
            if (points == null || points.Count != 4) 
                throw new KMLException("In KML element LatLonQuad has to have exactly the 4 coordinates.");

            // Closes the polygon. Stores the last point the same as the first one.
            points.Add(points[0].Clone());

            var llq = new LatLonQuad();

            llq.Points.Clear();
            llq.Points.AddRange(points);

            Geographies.Add(llq);
        }

        /// <summary>
        /// This method visits a KML element. Method extracts the child elements:
        /// Longitude, Latitude, Altitude and AltitudeMode and creates the point instance from those values.
        /// </summary>
        /// <param name="element">KML element to be visited</param>
        protected void VisitLonLatAltElement(XmlElement element)
        {
            double longitude = 0, latitude = 0, altitude = 0;
            int? altitudeMode;
            try
            {
                var longitudeElem = GetFirstChild(element, "longitude");
                if (longitudeElem != null && longitudeElem.HasChildNodes)
                {
                    var s = longitudeElem.ChildNodes[0].InnerText;
                    longitude = double.Parse(s);
                }
                var latitudeElem = GetFirstChild(element, "latitude");
                if (latitudeElem != null && latitudeElem.HasChildNodes)
                {
                    var s = latitudeElem.ChildNodes[0].InnerText;
                    latitude = double.Parse(s);
                }
                var altitudeElem = GetFirstChild(element, "altitude");
                if (altitudeElem != null && altitudeElem.HasChildNodes)
                {
                    var s = altitudeElem.ChildNodes[0].InnerText;
                    altitude = double.Parse(s);
                }

                altitudeMode = GetAltitudeModeCode(element);
            }
            catch (Exception exc)
            {
                throw new KMLException("Location coordinates in wrong format!", exc);
            }

            var pt = new Point(longitude, latitude, altitude, altitudeMode);
            Geographies.Add(pt);
        }

        /// <summary>
        /// This method visits a KML Model element
        /// </summary>
        /// <param name="element">KML model element to be visited</param>
        protected void VisitModel(XmlElement element)
        {
            if (element == null) return;

            var m = new Model();

            #region Extract Location

            var locationElem = GetFirstChild(element, "location");
            if (locationElem != null)
            {
                VisitLonLatAltElement(locationElem);

                m.Location = Geographies[Geographies.Count - 1] as Point;
                Geographies.RemoveAt(Geographies.Count - 1);
            }

            #endregion

            #region Extract other non-geographic data

            VisitGeography(element, m);

            var orientationElem = GetFirstChild(element, "orientation");
            if (orientationElem != null)
            {
                m.OrientationHeading = GetChildElementText<double>(orientationElem, "heading", 0);
                m.OrientationTilt = GetChildElementText<double>(orientationElem, "tilt", 0);
                m.OrientationRoll = GetChildElementText<double>(orientationElem, "roll", 0);
            }

            var scaleElem = GetFirstChild(element, "scale");
            if (scaleElem != null)
            {
                m.ScaleX = GetChildElementText<double>(scaleElem, "x", 1);
                m.ScaleY = GetChildElementText<double>(scaleElem, "y", 1);
                m.ScaleZ = GetChildElementText<double>(scaleElem, "z", 1);
            }

            #endregion

            Models.Add(m);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// This method parses the given KML string
        /// </summary>
        public void Parse()
        {
            // Visits the all child nodes
            foreach (XmlNode n in Document.ChildNodes)
            {
                Visit(n);
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// This method parses the given XML file context
        /// </summary>
        /// <param name="fileFullPathName">File full path</param>
        /// <returns>KMLParser with loaded file context</returns>
        public static KMLParser ParseFile(string fileFullPathName)
        {
            var text = System.IO.File.ReadAllText(fileFullPathName);

            var p = new KMLParser(text);

            return p;
        }

        /// <summary>
        /// This method returns the first child element with the given name in the given parent element
        /// </summary>
        /// <param name="element">Parent element</param>
        /// <param name="childElementName">Child element name</param>
        /// <returns>Child element if it exists, null otherwise</returns>
        public static XmlElement GetFirstChild(XmlElement element, string childElementName)
        {
            if (element == null || element.HasChildNodes == false || string.IsNullOrEmpty(childElementName))
                return null;

            childElementName = childElementName.ToLowerInvariant();

            foreach (XmlNode node in element.ChildNodes)
            {
                var c = node as XmlElement;
                if (c == null)
                    continue;

                if (c.Name.ToLowerInvariant().Equals(childElementName))
                    return c;
            }

            return null;
        }

        /// <summary>
        /// This method determines the altitude mode for the given KML geography element.
        /// </summary>
        /// <param name="elem">KML geography element whose altitude mode should be found</param>
        /// <returns>Altitude mode, or null if the altitude mode is not found</returns>
        private static AltitudeMode? GetAltitudeMode(XmlElement elem)
        {
            var altitudeModeVal = "clamptoground";
            var altitudeMode = GetFirstChild(elem, "altitudeMode") ?? GetFirstChild(elem, "gx:altitudeMode");

            if (altitudeMode == null)
                return null;

            if (altitudeMode.HasChildNodes && altitudeMode.ChildNodes[0].Value != null)
            {
                altitudeModeVal = altitudeMode.ChildNodes[0].Value.Trim().ToLowerInvariant();
            }

            if (altitudeModeVal.Equals("clamptoground"))
                return AltitudeMode.ClampToGround;
            if (altitudeModeVal.Equals("relativetoground"))
                return AltitudeMode.RelativeToGround;
            if (altitudeModeVal.Equals("absolute"))
                return AltitudeMode.Absolute;
            if (altitudeModeVal.Equals("relativetoseafloor"))
                return AltitudeMode.RelativeToSeaFloor;
            if (altitudeModeVal.Equals("clamptoseafloor"))
                return AltitudeMode.ClampToSeaFloor;

            throw new KMLException("Altitude mode not supported: " + altitudeModeVal);
        }

        /// <summary>
        /// This method determines the altitude mode code for the given KML element.
        /// </summary>
        /// <param name="elem">KML element</param>
        /// <returns>Altitude mode code, or null if the altitude mode is not found</returns>
        public static int? GetAltitudeModeCode(XmlElement elem)
        {
            var altitudeMode = GetAltitudeMode(elem);

            if (altitudeMode == null)
                return null;

            return (int)altitudeMode;
        }

        /// <summary>
        /// This method returns the tessellate flag value for the given KML element.
        /// </summary>
        /// <param name="elem">KML element</param>
        /// <returns>Tessellate flag value</returns>
        public static bool GetTessellateFlag(XmlElement elem)
        {
            var tessellateFlag = false;
            XmlNode tessellate = GetFirstChild(elem, "tessellate");
            if (tessellate != null && tessellate.HasChildNodes)
            {
                var tessTxt = tessellate.ChildNodes[0];
                if (tessTxt.InnerText.Trim().Equals("1"))
                    tessellateFlag = true;
            }

            return tessellateFlag;
        }

        /// <summary>
        /// This method returns the attribute value of the given xml element. 
        /// </summary>
        /// <param name="elem">Xml element whose attribute is requested</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns>Attribute value, or null value if the attribute is not set</returns>
        public static string GetAttribute(XmlElement elem, string attributeName)
        {
            var attribute = elem.Attributes[attributeName];

            return attribute?.Value;
        }

        /// <summary>
        /// This method returns the inner text in the child element of the given parent element
        /// </summary>
        /// <param name="element">Parent element</param>
        /// <param name="childElementName">Child element name</param>
        /// <returns>Child element inner text</returns>
        public static string GetChildElementText(XmlElement element, string childElementName)
        {
            if (element == null || !element.HasChildNodes) return null;

            var child = GetFirstChild(element, childElementName);

            return child?.InnerText;
        }

        /// <summary>
        /// This method returns the inner text in the child element of the given parent element.
        /// The inner text value is converted to the given type T
        /// </summary>
        /// <typeparam name="T">Type to convert the inner text value into</typeparam>
        /// <param name="element">Parent element</param>
        /// <param name="childElementName">Child element name</param>
        /// <returns>Child text value converted into the type T</returns>
        public static T GetChildElementText<T>(XmlElement element, string childElementName)
        {
            var childInnerText = GetChildElementText(element, childElementName);

            if (string.IsNullOrEmpty(childInnerText))
                return default(T);

            return (T)Convert.ChangeType(childInnerText, typeof(T));
        }

        /// <summary>
        /// This method returns the inner text in the child element of the given parent element.
        /// The inner text value is converted to the given type T. If the child with the given name doesn't exist then
        /// the given default value will be returned.
        /// </summary>
        /// <typeparam name="T">Type to convert the inner text value into</typeparam>
        /// <param name="element">Parent element</param>
        /// <param name="childElementName">Child element name</param>
        /// <param name="defaultValue">In case that the child doesn't exist, this value will be returned</param>
        /// <returns>Child text value converted into the type T, or the default value if the child is not found</returns>
        public static T GetChildElementText<T>(XmlElement element, string childElementName, T defaultValue)
        {
            var childInnerText = GetChildElementText(element, childElementName);

            if (string.IsNullOrEmpty(childInnerText))
                return defaultValue;

            return (T)Convert.ChangeType(childInnerText, typeof(T));
        }

        #endregion
    }
}
