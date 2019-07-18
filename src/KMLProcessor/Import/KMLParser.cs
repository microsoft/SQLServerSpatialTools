using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
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
			m_Document = new XmlDocument();
			m_Document.LoadXml(kml);
		}

		#endregion

		#region Public Data

		/// <summary>
		/// Geography instances extracted from the KML string
		/// </summary>
		public IList<Geography> Geographies
		{
			get
			{
				return m_Geographies;
			}
		}

		/// <summary>
		/// Placemarks extracted from the KML string
		/// </summary>
		public IList<Placemark> Placemarks
		{
			get
			{
				return m_Placemarks;
			}
		}

		/// <summary>
		/// Models extracted from the KML string
		/// </summary>
		public IList<Model> Models
		{
			get
			{
				return m_Models;
			}
		}

		/// <summary>
		/// Regions extracted from the KML string
		/// </summary>
		public IList<Region> Regions
		{
			get
			{
				return m_Regions;
			}
		}

		/// <summary>
		/// Ground overlays extracted from the KML string
		/// </summary>
		public IList<GroundOverlay> GroundOverlays
		{
			get
			{
				return m_GroundOverlays;
			}
		}

		#endregion

		#region Protected Data

		/// <summary>
		/// Xml document whose data will be traversed
		/// </summary>
		protected XmlDocument m_Document;

		/// <summary>
		/// Geography instances extracted from the KML string
		/// </summary>
		protected List<Geography> m_Geographies = new List<Geography>();

		/// <summary>
		/// Placemarks extracted from the KML string
		/// </summary>
		protected List<Placemark> m_Placemarks = new List<Placemark>();

		/// <summary>
		/// Models extracted from the KML string
		/// </summary>
		protected List<Model> m_Models = new List<Model>();

		/// <summary>
		/// Regions extracted from the KML string
		/// </summary>
		protected List<Region> m_Regions = new List<Region>();

		/// <summary>
		/// Ground overlays extracted from the KML string
		/// </summary>
		protected List<GroundOverlay> m_GroundOverlays = new List<GroundOverlay>();

		#endregion

		#region Protected Methods

		/// <summary>
		/// This method visits the given KML node
		/// </summary>
		/// <param name="node">KML node to be visited</param>
		protected void Visit(XmlNode node)
		{
			if (node == null) return;

			string nodeName = node.Name.ToLowerInvariant();
			XmlElement elem = node as XmlElement;

			if (elem != null)
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
		/// <param name="elem">KML Ground Overlay element</param>
		protected void VisitGroundOverlay(XmlElement element)
		{
			if (element == null) return;

			GroundOverlay groundOverlay = new GroundOverlay();

			VisitGeography(element, groundOverlay);
			groundOverlay.Name = GetChildElementText(element, "name");

			XmlElement latLonAltBoxElem = GetFirstChild(element, "LatLonAltBox");
			XmlElement latLonBoxElem = GetFirstChild(element, "LatLonBox");
			XmlElement latLonQuadElem = GetFirstChild(element, "gx:LatLonQuad");

			int numOfGegoraphiesBefore = m_Geographies.Count;

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

			int numOfGeographiesAfter = m_Geographies.Count;

			if (numOfGeographiesAfter == numOfGegoraphiesBefore + 1)
			{
				groundOverlay.Box = m_Geographies[m_Geographies.Count - 1];
				m_Geographies.RemoveAt(m_Geographies.Count - 1);
			}
			else if (numOfGeographiesAfter > numOfGegoraphiesBefore + 1)
			{
				// Multiple geography instances found in a single ground overlay. 
				// The last geography instance is the border.

				groundOverlay.Box = m_Geographies[m_Geographies.Count - 1];
				m_Geographies.RemoveAt(m_Geographies.Count - 1);
			}

			#region Check if a region is also defined inside this ground overlay

			XmlElement regionElem = GetFirstChild(element, "Region");
			if (regionElem != null)
			{
				int numOfRegionsBefore = m_Regions.Count;

				VisitRegion(regionElem);

				int numOfRegionsAfter = m_Regions.Count;

				if (numOfRegionsAfter == numOfRegionsBefore + 1)
				{
					groundOverlay.Region = m_Regions[numOfRegionsAfter - 1];
				}
				else if (numOfRegionsAfter > numOfRegionsBefore + 1)
				{
					// Multiple regions found in a single ground overlay. The last region is the border.

					groundOverlay.Region = m_Regions[numOfRegionsAfter - 1];
				}
			}

			#endregion

			m_GroundOverlays.Add(groundOverlay);
		}

		/// <summary>
		/// This method visits a KML placemark element
		/// </summary>
		/// <param name="elem">KML placemark element</param>
		protected void VisitPlacemark(XmlElement elem)
		{
			if (elem == null) return;

			Placemark placemark = new Placemark();

			placemark.Id = GetAttribute(elem, "id");

			// Visits the child nodes 
			foreach (XmlNode child in elem.ChildNodes)
			{
				string childName = child.Name.ToLowerInvariant();
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
					int numOfGeographiesBefore = m_Geographies.Count;

					VisitLookAt(child as XmlElement);

					int numOfGeographiesAfter = m_Geographies.Count;

					if (numOfGeographiesAfter > numOfGeographiesBefore)
					{
						// Point found where this placemark looks at

						placemark.LookAt = m_Geographies[m_Geographies.Count - 1];
						m_Geographies.RemoveAt(m_Geographies.Count - 1);
					}
				}
				else
				{
					int numOfGeographiesBefore = m_Geographies.Count;

					Visit(child);

					int numOfGeographiesAfter = m_Geographies.Count;

					if (numOfGeographiesAfter > numOfGeographiesBefore)
					{
						// Found the geography instance which describes this placemark

						placemark.Geography = m_Geographies[m_Geographies.Count - 1];
					}
				}
			}

			m_Placemarks.Add(placemark);
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
		protected IList<Point> ParseCoordinates(string coordinates)
		{
			List<Point> points = new List<Point>();

			if (string.IsNullOrEmpty(coordinates) || coordinates.Trim().Length == 0)
				return points;

			// Removes the spaces inside the touples
			while (coordinates.Contains(", "))
			{
				coordinates = coordinates.Replace(", ", ",");
			}

			// Splits the tuples
			string[] coordinate = coordinates.Trim().Split(' ', '\t', '\n');
			if (coordinate == null)
				return points;

			// Process each tuple
			foreach (string p in coordinate)
			{
				string tp = p.Trim();
				if (tp.Length == 0)
					continue;

				string[] cords = tp.Split(',');

				try
				{
					double longitude = double.Parse(cords[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
					double latitude = double.Parse(cords[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
					double? altitude = null;

					if (cords.Length == 3)  // the altitude is optional
					{
						altitude = double.Parse(cords[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
					}

					Point point = new Point();
					point.Longitude = longitude;
					point.Latitude = latitude;
					point.Altitude = altitude;

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
		protected void VisitGeography(XmlElement elem, Geography geography)
		{
			#region The id in the KML string

			geography.Id = GetAttribute(elem, "id");

			#endregion

			#region The extrude flag

			XmlElement extrudeElem = GetFirstChild(elem, "extrude");
			if (extrudeElem != null && extrudeElem.HasChildNodes)
			{
				string val = extrudeElem.ChildNodes[0].InnerText;
				if (val != null && val.Trim().Equals("1"))
				{
					geography.Extrude = true;
				}
			}

			#endregion

			#region The tessellate flag

			XmlElement tessellateElem = GetFirstChild(elem, "tessellate");
			if (tessellateElem != null && tessellateElem.HasChildNodes)
			{
				string val = tessellateElem.ChildNodes[0].InnerText;
				if (val != null && val.Trim().Equals("1"))
				{
					geography.Tesselate = true;
				}
			}

			#endregion
		}

		/// <summary>
		/// This method visits a KML Point element
		/// </summary>
		/// <param name="node">KML Point element to be visited</param>
		protected void VisitPoint(XmlElement elem)
		{
			if (elem == null) return;

			// Extracts the point coordinates
			XmlElement coordinates = GetFirstChild(elem, "coordinates");
			if (coordinates == null || !coordinates.HasChildNodes)
				return;

			XmlNode coordinatesTextNode = coordinates.ChildNodes[0];

			string coordinatesText = coordinatesTextNode.InnerText;

			IList<Point> pts = ParseCoordinates(coordinatesText);

			if (pts.Count == 0)
				return;

			Point point = pts[0];

			// Extracts the altitude mode
			point.Measure = GetAltitudeModeCode(elem);

			// Extracts the other non-geographic data
			VisitGeography(elem, point);

			// Sets the geography instance context
			point.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;

			m_Geographies.Add(point);
		}

		/// <summary>
		/// This method visits a KML LineString element
		/// </summary>
		/// <param name="elem">KML LineString element to be visited</param>
		protected void VisitLineString(XmlElement elem)
		{
			VisitLineString(elem, false);
		}

		/// <summary>
		/// This method visits a KML LineString element
		/// </summary>
		/// <param name="elem">KML line string element to be visited</param>
		/// <param name="isLineRing">True if the line is the ring</param>
		protected void VisitLineString(
			XmlElement elem,
			bool isLineRing)
		{
			if (elem == null) return;

			XmlNode coordinates = GetFirstChild(elem, "coordinates");
			if (coordinates == null || !coordinates.HasChildNodes)
				return;

			XmlNode txt = coordinates.ChildNodes[0];

			string allCoordinates = txt.InnerText;

			IList<Point> points = ParseCoordinates(allCoordinates);
			if (points == null || points.Count == 0)
				return;

			LineString ls = null;
			if (isLineRing)
			{
				ls = new LinearRing();
			}
			else
			{
				ls = new LineString();
			}

			ls.Points.AddRange(points);

			int? altitudeModeCode = GetAltitudeModeCode(elem);

			foreach (Point p in ls.Points)
			{
				p.Measure = altitudeModeCode;
			}

			// Extract other non-geographic data
			VisitGeography(elem, ls);

			#region Handle tesselate flag

			ls.StoreTesselateFlag();

			#endregion

			// Sets the geography instance context
			ls.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;
			
			m_Geographies.Add(ls);
		}

		/// <summary>
		/// This method visits a KML LinearRing element
		/// </summary>
		/// <param name="node">KML LinearRing element to be visited</param>
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

			XmlElement outerBoundaryIs = GetFirstChild(elem, "outerBoundaryIs");
			if (outerBoundaryIs == null)
				return;

			XmlElement outerLinearRing = GetFirstChild(outerBoundaryIs, "LinearRing");
			if (outerLinearRing == null)
				return;

			int numOfGeographiesBefore = this.Geographies.Count;

			VisitLinearRing(outerLinearRing);

			int numOfGeographiesAfter = this.Geographies.Count;

			// Extracts the outer ring
			if (numOfGeographiesAfter == numOfGeographiesBefore)
				throw new KMLException("Outer ring not found!");

			int index = this.Geographies.Count - 1;
			Geography g = this.Geographies[index];
			this.m_Geographies.RemoveAt(index);

			LinearRing outerRing = g as LinearRing;
			if (outerRing == null)
				throw new KMLException("Outer ring not created!");

			Polygon polygon = new Polygon();

			bool isValidPolygon = true;

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

			XmlNodeList innerBoundaryIsLst = elem.GetElementsByTagName("innerBoundaryIs");
			if (innerBoundaryIsLst != null)
			{
				foreach (XmlNode innerBorderNode in innerBoundaryIsLst)
				{
					XmlElement innerBoundaryIs = innerBorderNode as XmlElement;
					if (innerBoundaryIs == null)
						continue;

					XmlElement innerLinearRing = GetFirstChild(innerBoundaryIs, "LinearRing");
					if (innerLinearRing == null)
						continue;

					numOfGeographiesBefore = this.Geographies.Count;

					VisitLinearRing(innerLinearRing);

					numOfGeographiesAfter = this.Geographies.Count;

					// Extracts the inner ring
					if (numOfGeographiesBefore == numOfGeographiesAfter)
						throw new KMLException("Inner ring not found!");

					index = this.Geographies.Count - 1;
					g = this.Geographies[index];
					this.m_Geographies.RemoveAt(index);

					LinearRing innerRing = g as LinearRing;
					if (innerRing == null)
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
			}

			#endregion

			// Extracts the other non-geographic data
			VisitGeography(elem, polygon);

			#region Extract altitude mode

			int? altitudeModeCode = GetAltitudeModeCode(elem);
			if (altitudeModeCode.HasValue)
			{
				// Updates all the points on the outer bound
				if (polygon.OuterRing != null)
				{
					foreach (Point p in polygon.OuterRing.Points)
					{
						p.Measure = altitudeModeCode;
					}
				}

				// Update all the points on all the inner bounds
				if (polygon.InnerRing != null)
				{
					foreach (LinearRing r in polygon.InnerRing)
					{
						if (r.Points == null)
							return;

						foreach (Point p1 in r.Points)
						{
							p1.Measure = altitudeModeCode;
						}
					}
				}
			}

			#endregion

			#region Handles tesselate flag

			if (polygon.Tesselate)
			{
				if (polygon.OuterRing != null)
					polygon.OuterRing.StoreTesselateFlag();

				if (polygon.InnerRing != null)
				{
					foreach (LinearRing r in polygon.InnerRing)
						r.StoreTesselateFlag();
				}
			}

			#endregion

			// Sets the geography instance context
			polygon.Context = (elem.ParentNode != null) ? elem.ParentNode.OuterXml : elem.OuterXml;

			m_Geographies.Add(polygon);
		}

		/// <summary>
		/// This method visits a KML MultyGeometry element
		/// </summary>
		/// <param name="element">KML MultyGeometry element to be visited</param>
		protected void VisitMultiGeometry(XmlElement element)
		{
			if (element == null)
				return;

			MultiGeometry mg = new MultiGeometry();

			if (element.HasChildNodes)
			{
				foreach (XmlNode child in element.ChildNodes)
				{
					int lastIndexBefore = this.Geographies.Count - 1;

					Visit(child);

					int lastIndex = this.Geographies.Count - 1;

					if (lastIndex > lastIndexBefore)
					{
						Geography childGeography = this.Geographies[lastIndex];
						this.Geographies.RemoveAt(lastIndex);
						mg.Geographies.Add(childGeography);
					}
				}
			}

			// Sets the geography instance context
			mg.Context = (element.ParentNode != null) ? element.ParentNode.OuterXml : element.OuterXml;

			this.Geographies.Add(mg);
		}

		/// <summary>
		/// This method visits a KML Region element
		/// </summary>
		/// <param name="element">KML Region element to be visited</param>
		protected void VisitRegion(XmlElement element)
		{
			if (element == null) return;

			Region r = new Region();

			VisitGeography(element, r);

			XmlElement latLonAltBoxElem = GetFirstChild(element, "LatLonAltBox");
			XmlElement latLonBoxElem = GetFirstChild(element, "LatLonBox");
			XmlElement latLonQuadElem = GetFirstChild(element, "gx:LatLonQuad");

			int numOfGegoraphiesBefore = m_Geographies.Count;

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

			int numOfGeographiesAfter = m_Geographies.Count;

			if (numOfGeographiesAfter > numOfGegoraphiesBefore)
			{
				// Found region's box

				r.Box = m_Geographies[m_Geographies.Count - 1];
				m_Geographies.RemoveAt(m_Geographies.Count - 1);
			}

			m_Regions.Add(r);
		}

		/// <summary>
		/// This method visits a KML LatLonAltBox element
		/// </summary>
		/// <param name="element">KML LatLonAltBox element to be visited</param>
		protected void VisitLatLonAltBox(XmlElement element)
		{
			if (element == null) return;

			#region Required properties

			double north = GetChildElementText<double>(element, "north");
			double south = GetChildElementText<double>(element, "south");
			double west = GetChildElementText<double>(element, "west");
			double east = GetChildElementText<double>(element, "east");

			#endregion

			#region Optional properties

			double minAltitude = 0;
			double maxAltitude = 0;
			AltitudeMode? altitudeMode;
			string minAltitudeS = GetChildElementText(element, "minAltitude");
			if (!string.IsNullOrEmpty(minAltitudeS))
			{
				minAltitude = double.Parse(minAltitudeS);
			}

			string maxAltitudeS = GetChildElementText(element, "maxAltitude");
			if (!string.IsNullOrEmpty(maxAltitudeS))
			{
				maxAltitude = double.Parse(maxAltitudeS);
			}

			altitudeMode = GetAltitudeMode(element);

			#endregion

			LatLonAltBox b = new LatLonAltBox();
			b.North = north;
			b.South = south;
			b.West = west;
			b.East = east;
			b.MinAltitude = minAltitude;
			b.MaxAltitude = maxAltitude;
			b.AltitudeMode = altitudeMode;

			m_Geographies.Add(b);
		}

		/// <summary>
		/// This method visits a KML LatLonBox element
		/// </summary>
		/// <param name="element">KML LatLonBox element to be visited</param>
		protected void VisitLatLonBox(XmlElement element)
		{
			if (element == null) return;

			#region Required properties

			double north = GetChildElementText<double>(element, "north");
			double south = GetChildElementText<double>(element, "south");
			double west = GetChildElementText<double>(element, "west");
			double east = GetChildElementText<double>(element, "east");

			#endregion

			#region Optional properties

			double rotation = 0;

			string rotationS = GetChildElementText(element, "rotation");
			if (!string.IsNullOrEmpty(rotationS))
			{
				rotation = double.Parse(rotationS);
			}

			#endregion

			LatLonBox b = new LatLonBox();
			b.North = north;
			b.South = south;
			b.West = west;
			b.East = east;
			b.Rotation = rotation;

			m_Geographies.Add(b);
		}

		/// <summary>
		/// This method visits a KML LatLonQuad element
		/// </summary>
		/// <param name="element">LatLonQuad KML element to be visited</param>
		protected void VisitLatLonQuad(XmlElement element)
		{
			if (element == null) return;

			string coordinates = GetChildElementText(element, "coordinates");

			IList<Point> points = ParseCoordinates(coordinates);
			if (points == null || points.Count != 4) 
				throw new KMLException("In KML element LatLonQuad has to have exactly the 4 coordinates.");

			// Closes the polygon. Stores the last point the same as the first one.
			points.Add(points[0].Clone());

			LatLonQuad llq = new LatLonQuad();

			llq.Points.Clear();
			llq.Points.AddRange(points);

			m_Geographies.Add(llq);
		}

		/// <summary>
		/// This method visits a KML element. Method extracts the child elements:
		/// Longitude, Latitude, Altitude and AltitudeMode and creates the point instance from those values.
		/// </summary>
		/// <param name="element">KML element to be visited</param>
		protected void VisitLonLatAltElement(XmlElement element)
		{
			double longitude = 0, latitude = 0, altitude = 0;
			int? altitudeMode = null;
			try
			{
				XmlElement longitudeElem = GetFirstChild(element, "longitude");
				if (longitudeElem != null && longitudeElem.HasChildNodes)
				{
					string s = longitudeElem.ChildNodes[0].InnerText;
					longitude = double.Parse(s);
				}
				XmlElement latitudeElem = GetFirstChild(element, "latitude");
				if (latitudeElem != null && latitudeElem.HasChildNodes)
				{
					string s = latitudeElem.ChildNodes[0].InnerText;
					latitude = double.Parse(s);
				}
				XmlElement altitudeElem = GetFirstChild(element, "altitude");
				if (altitudeElem != null && altitudeElem.HasChildNodes)
				{
					string s = altitudeElem.ChildNodes[0].InnerText;
					altitude = double.Parse(s);
				}

				altitudeMode = GetAltitudeModeCode(element);
			}
			catch (Exception exc)
			{
				throw new KMLException("Location coordinates in wrong format!", exc);
			}

			Point pt = new Point(longitude, latitude, altitude, altitudeMode);
			m_Geographies.Add(pt);
		}

		/// <summary>
		/// This method visits a KML Model element
		/// </summary>
		/// <param name="element">KML model element to be visited</param>
		protected void VisitModel(XmlElement element)
		{
			if (element == null) return;

			Model m = new Model();

			#region Extract Location

			XmlElement locationElem = GetFirstChild(element, "location");
			if (locationElem != null)
			{
				VisitLonLatAltElement(locationElem);

				m.Location = m_Geographies[m_Geographies.Count - 1] as Point;
				m_Geographies.RemoveAt(m_Geographies.Count - 1);
			}

			#endregion

			#region Extract other non-geographic data

			VisitGeography(element, m);

			XmlElement orientationElem = GetFirstChild(element, "orientation");
			if (orientationElem != null)
			{
				m.OrientationHeading = GetChildElementText<double>(orientationElem, "heading", 0);
				m.OrientationTilt = GetChildElementText<double>(orientationElem, "tilt", 0);
				m.OrientationRoll = GetChildElementText<double>(orientationElem, "roll", 0);
			}

			XmlElement scaleElem = GetFirstChild(element, "scale");
			if (scaleElem != null)
			{
				m.ScaleX = GetChildElementText<double>(scaleElem, "x", 1);
				m.ScaleY = GetChildElementText<double>(scaleElem, "y", 1);
				m.ScaleZ = GetChildElementText<double>(scaleElem, "z", 1);
			}

			#endregion

			m_Models.Add(m);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// This method parses the given KML string
		/// </summary>
		public void Parse()
		{
			// Visits the all child nodes
			foreach (XmlNode n in m_Document.ChildNodes)
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
			string text = System.IO.File.ReadAllText(fileFullPathName);

			KMLParser p = new KMLParser(text);

			return p;
		}

		/// <summary>
		/// This method returns the first child element with the given name in the given parent element
		/// </summary>
		/// <param name="element">Parent element</param>
		/// <param name="childElementName">Child element name</param>
		/// <returns>Child element if it exists, null othervise</returns>
		public static XmlElement GetFirstChild(XmlElement element, string childElementName)
		{
			if (element == null || element.HasChildNodes == false || string.IsNullOrEmpty(childElementName))
				return null;

			childElementName = childElementName.ToLowerInvariant();

			foreach (XmlNode node in element.ChildNodes)
			{
				XmlElement c = node as XmlElement;
				if (c == null)
					continue;

				if (c.Name.ToLowerInvariant().Equals(childElementName))
					return c;
			}

			return null;
		}

		/// <summary>
		/// This method determines the altitude mode for the given KML geograpy element.
		/// </summary>
		/// <param name="elem">KML geography element whose altitude mode should be found</param>
		/// <returns>Altitude mode, or null if the altitude mode is not found</returns>
		private static AltitudeMode? GetAltitudeMode(XmlElement elem)
		{
			string altitudeModeVal = "clamptoground";
			XmlElement altitudeMode = GetFirstChild(elem, "altitudeMode");
			if (altitudeMode == null)
				altitudeMode = GetFirstChild(elem, "gx:altitudeMode");

			if (altitudeMode == null)
				return null;

			if (altitudeMode != null && altitudeMode.HasChildNodes && altitudeMode.ChildNodes[0].Value != null)
			{
				altitudeModeVal = altitudeMode.ChildNodes[0].Value.Trim().ToLowerInvariant();
			}

			if (altitudeModeVal.Equals("clamptoground"))
				return AltitudeMode.clampToGround;
			else if (altitudeModeVal.Equals("relativetoground"))
				return AltitudeMode.relativeToGround;
			else if (altitudeModeVal.Equals("absolute"))
				return AltitudeMode.absolute;
			else if (altitudeModeVal.Equals("relativetoseafloor"))
				return AltitudeMode.relativeToSeaFloor;
			else if (altitudeModeVal.Equals("clamptoseafloor"))
				return AltitudeMode.clampToSeaFloor;

			throw new KMLException("Altitude mode not supported: " + altitudeModeVal);
		}

		/// <summary>
		/// This method determines the altitude mode code for the given KML element.
		/// </summary>
		/// <param name="elem">KML element</param>
		/// <returns>Altitude mode code, or null if the altitude mode is not found</returns>
		public static int? GetAltitudeModeCode(XmlElement elem)
		{
			AltitudeMode? altitideMode = GetAltitudeMode(elem);

			if (altitideMode == null)
				return null;

			return (int)altitideMode;
		}

		/// <summary>
		/// This method returns the tesselate flag value for the given KML element.
		/// </summary>
		/// <param name="elem">KML element</param>
		/// <returns>Tesselate flag value</returns>
		public static bool GetTesselateFlag(XmlElement elem)
		{
			bool tessalateFlag = false;
			XmlNode tessellate = GetFirstChild(elem, "tessellate");
			if (tessellate != null && tessellate.HasChildNodes)
			{
				XmlNode tessTxt = tessellate.ChildNodes[0];
				if (tessTxt.InnerText != null && tessTxt.InnerText.Trim().Equals("1"))
					tessalateFlag = true;
			}

			return tessalateFlag;
		}

		/// <summary>
		/// This method returns the attribute value of the given xml element. 
		/// </summary>
		/// <param name="elem">Xml element whose attribute is requested</param>
		/// <param name="attributeName">Attribute name</param>
		/// <returns>Attribute value, or null value if the attribute is not set</returns>
		public static string GetAttribute(XmlElement elem, string attributeName)
		{
			XmlAttribute attribute = elem.Attributes[attributeName];
			if (attribute == null)
				return null;

			return attribute.Value;
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

			XmlElement child = GetFirstChild(element, childElementName);
			if (child == null) return null;

			return child.InnerText;
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
			string childInnerText = GetChildElementText(element, childElementName);

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
			string childInnerText = GetChildElementText(element, childElementName);

			if (string.IsNullOrEmpty(childInnerText))
				return defaultValue;

			return (T)Convert.ChangeType(childInnerText, typeof(T));
		}

		#endregion
	}
}
