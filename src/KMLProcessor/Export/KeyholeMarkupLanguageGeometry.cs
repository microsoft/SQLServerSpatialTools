//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
// ReSharper disable UnusedMember.Global

namespace SQLSpatialTools.KMLProcessor.Export
{
	/// <summary>
	/// This class is the geometry sink. It will export the given geometry instances
	/// into the KML format.
	/// </summary>
	internal class KeyholeMarkupLanguageGeometry : KeyholeMarkupLanguageBase, IGeometrySink110
	{
		#region Constructors

		/// <summary>
		/// Constructor. Creates a KeyholeMarkupLanguageGeometry sink which will fill the given
		/// xml writer with data in the KML format
		/// </summary>
		/// <param name="writer">Xml writer to be filled with data</param>
		public KeyholeMarkupLanguageGeometry(System.Xml.XmlWriter writer)
			: base(writer)
		{
		}

		#endregion

		#region IGeometrySink interface

		/// <summary>
		/// This method will be called when a new geometry instance is passed to the sink
		/// </summary>
		/// <param name="type">Geometry type</param>
		public void BeginGeometry(OpenGisGeometryType type)
		{
			_context.BeginSpatialObject(type);

			switch (type)
			{
				case OpenGisGeometryType.Point:
					{
						StartElement("Point");
						StartElement("coordinates");
						break;
					}
				case OpenGisGeometryType.LineString:
					{
						StartElement("LineString");
						StartElement("coordinates");
						break;
					}
				case OpenGisGeometryType.Polygon:
					{
						StartElement("Polygon");
						break;
					}
				case OpenGisGeometryType.MultiPoint:
				case OpenGisGeometryType.MultiLineString:
				case OpenGisGeometryType.MultiPolygon:
				case OpenGisGeometryType.GeometryCollection:
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
		/// This method will be called when the geometry instance is finalized
		/// </summary>
		public void EndGeometry()
		{
			// Gets the type of the geometry instance which should be finalized
			var type = _context.Type;

			_context.EndSpatialObject();

			if (type == OpenGisGeometryType.Point ||
				type == OpenGisGeometryType.LineString)
			{
				EndElement();
			}

			EndElement();
		}

		/// <summary>
		/// This method will be called when a new figure is passed to the sink
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="m">M coordinate</param>
		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_context.BeginFigure();

			if (_context.Type == OpenGisGeometryType.Polygon)
			{
				StartElement(_context.IsFirstFigure ? "outerBoundaryIs" : "innerBoundaryIs");
				StartElement("LinearRing");
				StartElement("coordinates");
			}

			Writer.WriteValue(y);
			Writer.WriteValue(",");
			Writer.WriteValue(x);
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

			if (_context.Type == OpenGisGeometryType.Polygon)
			{
				EndElement(); // Closing coordinates tag
				EndElement(); // Closing LinearRing tag
				EndElement(); // Closing outerBoundaryIs / innerBoundaryIs tag
			}
		}

		/// <summary>
		/// This method should be called when a new line is be passed to the sink
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		/// <param name="m">M coordinate</param>
		public void AddLine(double x, double y, double? z, double? m)
		{
			Writer.WriteValue("\r\n");

			Writer.WriteValue(y);
			Writer.WriteValue(",");
			Writer.WriteValue(x);
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
        private readonly ExportContext<OpenGisGeometryType> _context = new ExportContext<OpenGisGeometryType>();

		#endregion
	}
}
