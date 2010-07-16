using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class is the geography sink. It will export the given geography instances
	/// into the KML format.
	/// </summary>
	internal class KeyholeMarkupLanguageGeography : KeyholeMarkupLanguageBase, IGeographySink
	{
		#region Constructors

		/// <summary>
		/// Constructor. Creates a KeyholeMarkupLanguageGeography sink which will fill the given
		/// xml writer with data in the KML format
		/// </summary>
		/// <param name="writer">Xml writter to be filled with data</param>
		public KeyholeMarkupLanguageGeography(System.Xml.XmlWriter writer)
			: base(writer)
		{
		}

		#endregion

		#region IGeographySink interface

		/// <summary>
		/// This method will be called when a new geography instance should be passed to the sink
		/// </summary>
		/// <param name="type">Geography type</param>
		public void BeginGeography(OpenGisGeographyType type)
		{
			m_Context.BeginSpatialObject(type);

			switch (type)
			{
				case OpenGisGeographyType.Point:
					{
						StartElement("Point");
						break;
					}
				case OpenGisGeographyType.LineString:
					{
						StartElement("LineString");
						break;
					}
				case OpenGisGeographyType.Polygon:
					{
						StartElement("Polygon");
						break;
					}
				case OpenGisGeographyType.MultiPoint:
				case OpenGisGeographyType.MultiLineString:
				case OpenGisGeographyType.MultiPolygon:
				case OpenGisGeographyType.GeometryCollection:
					{
						StartElement("MultiGeometry");
						break;
					}
				default:
					{
						throw new KMLException("Type not supported: " + type.ToString());
					}
			}
		}

		/// <summary>
		/// This method will be called when the geography instance should be finalized
		/// </summary>
		public void EndGeography()
		{
			m_Context.EndSpatialObject();

			EndElement();
		}

		/// <summary>
		/// This method will be called when a new figure is passed to sink
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="m">M coordinate</param>
		public void BeginFigure(double x, double y, Nullable<double> z, Nullable<double> m)
		{
			m_Context.BeginFigure();

			#region Export of Altitude Mode flag

			if ((m_Context.Type == OpenGisGeographyType.Polygon && m_Context.IsFirstFigure) ||
				 m_Context.Type == OpenGisGeographyType.Point ||
				 m_Context.Type == OpenGisGeographyType.LineString)
			{
				// The following code exports the geography instance's altitude mode. 
				// Altitude mode is stored as the m coordinate of every point, 
				// but altitude mode is a geography instance property, so it must be the same for all vertices, 
				// and we take it from the m coordinate of the first point.

				if (m.HasValue)
				{
					int altitudeModeCode = (int)m.Value;
					if (Enum.IsDefined(typeof(AltitudeMode), altitudeModeCode))
					{
						AltitudeMode altitudeMode = (AltitudeMode)altitudeModeCode;

						switch (altitudeMode)
						{
							case AltitudeMode.absolute:
							case AltitudeMode.relativeToGround:
							case AltitudeMode.relativeToSeaFloor:
							{
								StartElement("altitudeMode");
								WriteString(altitudeMode.ToString());
								EndElement();
								break;
							}
							case AltitudeMode.clampToGround:
							case AltitudeMode.clampToSeaFloor:
							{
								_writer.WriteStartElement("altitudeMode", Constants.GxNamespace);
								WriteString(altitudeMode.ToString());
								_writer.WriteEndElement();
								break;
							}
							default:
							{
								throw new KMLException("Altitude mode not supported: " + altitudeMode.ToString());
							}
						}
					}
				}
			}

			#endregion

			#region Export of Tesselate Flag

			// If the altitude mode is "clamp to ground" or "clamp to sea floor" then a tesselate flag will be exported

			if (m_Context.Type == OpenGisGeographyType.LineString ||
				(m_Context.Type == OpenGisGeographyType.Polygon && m_Context.IsFirstFigure))
			{
				if (m.HasValue)
				{
					int altitudeModeCode = (int)m.Value;
					if (Enum.IsDefined(typeof(AltitudeMode), altitudeModeCode))
					{
						AltitudeMode altitudeMode = (AltitudeMode)altitudeModeCode;
						if (altitudeMode == AltitudeMode.clampToGround ||
							altitudeMode == AltitudeMode.clampToSeaFloor)
						{
							StartElement("tessellate");
							WriteString("1");
							EndElement();
						}
					}
				}
			}

			#endregion

			if (m_Context.Type == OpenGisGeographyType.Point ||
				m_Context.Type == OpenGisGeographyType.LineString)
			{
				StartElement("coordinates");
			}
			else if (m_Context.Type == OpenGisGeographyType.Polygon)
			{
				StartElement(m_Context.IsFirstFigure ? "outerBoundaryIs" : "innerBoundaryIs");
				StartElement("LinearRing");
				StartElement("coordinates");
			}

			_writer.WriteValue(Utilities.ShiftInRange(y, 180));
			_writer.WriteValue(",");
			_writer.WriteValue(Utilities.ShiftInRange(x, 90));
			if (z != null && z.HasValue)
			{
				_writer.WriteValue(",");
				_writer.WriteValue(z.Value);
			}
		}

		/// <summary>
		/// This method should be called when a figure is finalized
		/// </summary>
		public void EndFigure()
		{
			m_Context.EndFigure();

			if (m_Context.Type == OpenGisGeographyType.Point ||
				m_Context.Type == OpenGisGeographyType.LineString)
			{
				EndElement();
			}
			else if (m_Context.Type == OpenGisGeographyType.Polygon)
			{
				EndElement(); // Closing coordinates tag
				EndElement(); // Closing LinearRing tag
				EndElement(); // Closing outerBoundaryIs / innerBoundaryIs tag
			}
		}

		/// <summary>
		/// This method should be called when a new line is passed to the sink
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="m">M coordinate</param>
		public void AddLine(double x, double y, Nullable<double> z, Nullable<double> m)
		{
			_writer.WriteValue("\r\n");

			_writer.WriteValue(Utilities.ShiftInRange(y, 180));
			_writer.WriteValue(",");
			_writer.WriteValue(Utilities.ShiftInRange(x, 90));
			if (z != null && z.HasValue)
			{
				_writer.WriteValue(",");
				_writer.WriteValue(z.Value);
			}
		}

		#endregion

		#region Private Data

		/// <summary>
		/// Export execution context
		/// </summary>
		private ExportContext<OpenGisGeographyType> m_Context = new ExportContext<OpenGisGeographyType>();

		#endregion
	}
}
