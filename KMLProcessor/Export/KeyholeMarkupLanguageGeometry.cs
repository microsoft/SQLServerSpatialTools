using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class is the geometry sink. It will export the given geometry instances
	/// into the KML format.
	/// </summary>
	internal class KeyholeMarkupLanguageGeometry : KeyholeMarkupLanguageBase, IGeometrySink
	{
		#region Constructors

		/// <summary>
		/// Constructor. Creates a KeyholeMarkupLanguageGeometry sink which will fill the given
		/// xml writer with data in the KML format
		/// </summary>
		/// <param name="writer">Xml writter to be filled with data</param>
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
			m_Context.BeginSpatialObject(type);

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
			OpenGisGeometryType type = m_Context.Type;

			m_Context.EndSpatialObject();

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
		public void BeginFigure(double x, double y, Nullable<double> z, Nullable<double> m)
		{
			m_Context.BeginFigure();

			if (m_Context.Type == OpenGisGeometryType.Polygon)
			{
				StartElement(m_Context.IsFirstFigure ? "outerBoundaryIs" : "innerBoundaryIs");
				StartElement("LinearRing");
				StartElement("coordinates");
			}

			_writer.WriteValue(y);
			_writer.WriteValue(",");
			_writer.WriteValue(x);
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

			if (m_Context.Type == OpenGisGeometryType.Polygon)
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
		public void AddLine(double x, double y, Nullable<double> z, Nullable<double> m)
		{
			_writer.WriteValue("\r\n");

			_writer.WriteValue(y);
			_writer.WriteValue(",");
			_writer.WriteValue(x);
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
		private ExportContext<OpenGisGeometryType> m_Context = new ExportContext<OpenGisGeometryType>();

		#endregion
	}
}
