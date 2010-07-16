using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about a LatLonQuad instance extracted from the KML file
	/// </summary>
	public class LatLonQuad : Geography
	{
		#region Public Data

		/// <summary>
		/// The quad vertices
		/// </summary>
		public List<Point> Points
		{
			get { return m_Points; }
		}
		/// <summary>
		/// Data member for the Points property
		/// </summary>
		protected List<Point> m_Points = new List<Point>();

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				if (Points == null || Points.Count == 0)
					throw new KMLException("WKT is not defined.");

				string wkt = "polygon((";

				bool first = true;
				foreach (Point p in Points)
				{
					if (first)
						first = false;
					else
						wkt += ", ";

					wkt += p.WKT;
				}

				wkt += "))";
				return wkt;
			}
		}

		#endregion

		#region Geography methods

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(Microsoft.SqlServer.Types.IGeographySink sink)
		{
			if (Points == null || Points.Count == 0)
				return;

			sink.BeginGeography(OpenGisGeographyType.Polygon);

			sink.BeginFigure(Points[0].Latitude, Points[0].Longitude, Points[0].Altitude, Points[0].Measure);

			for (int i = 1; i < Points.Count; i++)
			{
				sink.AddLine(Points[i].Latitude, Points[i].Longitude, Points[i].Altitude, Points[i].Measure);
			}

			sink.EndFigure();

			sink.EndGeography();
		}

		#endregion
	}
}
