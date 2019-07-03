//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.KMLProcessor.Import;

namespace SQLSpatialTools.KMLProcessor.Export
{
	/// <summary>
	/// This class is the geography sink. It will export the given geography instances
	/// into the KML format.
	/// </summary>
	internal class KeyholeMarkupLanguageGeography : KeyholeMarkupLanguageBase, IGeographySink110
	{
		#region Constructors

		/// <summary>
		/// Constructor. Creates a KeyholeMarkupLanguageGeography sink which will fill the given
		/// xml writer with data in the KML format
		/// </summary>
		/// <param name="writer">Xml writer to be filled with data</param>
		public KeyholeMarkupLanguageGeography(System.Xml.XmlWriter writer)
			: base(writer)
		{
		}

		#endregion

		#region IGeographySink110 interface

		/// <summary>
		/// This method will be called when a new geography instance should be passed to the sink
		/// </summary>
		/// <param name="type">Geography type</param>
		public void BeginGeography(OpenGisGeographyType type)
		{
			_context.BeginSpatialObject(type);

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
			_context.EndSpatialObject();

			EndElement();
		}

		/// <summary>
		/// This method will be called when a new figure is passed to sink
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="m">M coordinate</param>
		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_context.BeginFigure();

			#region Export of Altitude Mode flag

			if ((_context.Type == OpenGisGeographyType.Polygon && _context.IsFirstFigure) ||
				 _context.Type == OpenGisGeographyType.Point ||
				 _context.Type == OpenGisGeographyType.LineString)
			{
				// The following code exports the geography instance's altitude mode. 
				// Altitude mode is stored as the m coordinate of every point, 
				// but altitude mode is a geography instance property, so it must be the same for all vertices, 
				// and we take it from the m coordinate of the first point.

				if (m.HasValue)
				{
					var altitudeModeCode = (int)m.Value;
					if (Enum.IsDefined(typeof(AltitudeMode), altitudeModeCode))
					{
						var altitudeMode = (AltitudeMode)altitudeModeCode;

						switch (altitudeMode)
						{
							case AltitudeMode.Absolute:
							case AltitudeMode.RelativeToGround:
							case AltitudeMode.RelativeToSeaFloor:
							{
								StartElement("altitudeMode");
								WriteString(altitudeMode.ToString());
								EndElement();
								break;
							}
							case AltitudeMode.ClampToGround:
							case AltitudeMode.ClampToSeaFloor:
							{
								Writer.WriteStartElement("altitudeMode", Constants.GxNamespace);
								WriteString(altitudeMode.ToString());
								Writer.WriteEndElement();
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

			#region Export of Tessellate Flag

			// If the altitude mode is "clamp to ground" or "clamp to sea floor" then a tessellate flag will be exported

			if (_context.Type == OpenGisGeographyType.LineString ||
				(_context.Type == OpenGisGeographyType.Polygon && _context.IsFirstFigure))
			{
				if (m.HasValue)
				{
					var altitudeModeCode = (int)m.Value;
					if (Enum.IsDefined(typeof(AltitudeMode), altitudeModeCode))
					{
						var altitudeMode = (AltitudeMode)altitudeModeCode;
						if (altitudeMode == AltitudeMode.ClampToGround ||
							altitudeMode == AltitudeMode.ClampToSeaFloor)
						{
							StartElement("tessellate");
							WriteString("1");
							EndElement();
						}
					}
				}
			}

			#endregion

			if (_context.Type == OpenGisGeographyType.Point ||
				_context.Type == OpenGisGeographyType.LineString)
			{
				StartElement("coordinates");
			}
			else if (_context.Type == OpenGisGeographyType.Polygon)
			{
				StartElement(_context.IsFirstFigure ? "outerBoundaryIs" : "innerBoundaryIs");
				StartElement("LinearRing");
				StartElement("coordinates");
			}

			Writer.WriteValue(Utilities.ShiftInRange(y, 180));
			Writer.WriteValue(",");
			Writer.WriteValue(Utilities.ShiftInRange(x, 90));
			if (z.HasValue)
			{
				Writer.WriteValue(",");
				Writer.WriteValue(z.Value);
			}
		}

		/// <summary>
		/// This method should be called when a figure is finalized
		/// </summary>
		public void EndFigure()
		{
			_context.EndFigure();

			if (_context.Type == OpenGisGeographyType.Point ||
				_context.Type == OpenGisGeographyType.LineString)
			{
				EndElement();
			}
			else if (_context.Type == OpenGisGeographyType.Polygon)
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
		public void AddLine(double x, double y, double? z, double? m)
		{
			Writer.WriteValue("\r\n");

			Writer.WriteValue(Utilities.ShiftInRange(y, 180));
			Writer.WriteValue(",");
			Writer.WriteValue(Utilities.ShiftInRange(x, 90));
			if (z.HasValue)
			{
				Writer.WriteValue(",");
				Writer.WriteValue(z.Value);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        #endregion

        #region Private Data

        /// <summary>
        /// Export execution context
        /// </summary>
        private readonly ExportContext<OpenGisGeographyType> _context = new ExportContext<OpenGisGeographyType>();

		#endregion
	}
}
