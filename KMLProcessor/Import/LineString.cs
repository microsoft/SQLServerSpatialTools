using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the information about a line string extracted from the KML file
	/// </summary>
	public class LineString : Geography
	{
		#region Private Fields

		/// <summary>
		/// OpenGisGeographyType for this geography instance
		/// </summary>
		protected OpenGisGeographyType m_OpenGisGeographyType = OpenGisGeographyType.LineString;

		#endregion

		#region Public Properties

		/// <summary>
		/// The list of points
		/// </summary>
		public List<Point> Points
		{
			get { return m_Points; }
		}
		/// <summary>
		/// Data member for the Points property
		/// </summary>
		public List<Point> m_Points = new List<Point>();

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				return "LINESTRING" + Vertices;
			}
		}

		/// <summary>
		/// Returns the vertices of this linear ring. The returned string will be in the following format:
		/// (V0, V1, V2,..., Vn, V0). Vi will be in the format: longitude latitude [altitude [measure]]
		/// </summary>
		public string Vertices
		{
			get
			{
				string result = "(";
				bool isFirst = true;
				foreach (Point p in Points)
				{
					if (!isFirst)
						result += ", ";
					else
						isFirst = false;

					result += p.Longitude + " " + p.Latitude;
					if (p.Altitude.HasValue)
					{
						result += " " + p.Altitude.Value;

						if (p.Measure.HasValue)
							result += " " + p.Measure.Value;
					}
				}
				result += ")";

				return result;
			}
		}

		#endregion

		#region Geography Methods

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(IGeographySink sink)
		{
			if (Points == null || Points.Count == 0)
				return;

			sink.BeginGeography(m_OpenGisGeographyType);
			sink.BeginFigure(Points[0].Latitude, Points[0].Longitude, Points[0].Altitude, Points[0].Measure);

			for (int i = 1; i < Points.Count; i++)
			{
				sink.AddLine(Points[i].Latitude, Points[i].Longitude, Points[i].Altitude, Points[i].Measure);
			}

			sink.EndFigure();
			sink.EndGeography();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method stores the tesellate flag. M coordinates of all the point will be set
		/// to the "clamp to ground" value.
		/// </summary>
		public void StoreTesselateFlag()
		{
			if (Tesselate)
			{
				foreach (Point p in Points)
				{
					if (!p.Measure.HasValue)
					{
						p.Measure = (int)AltitudeMode.clampToGround;
					}
				}
			}
		}

		#endregion
	}
}
