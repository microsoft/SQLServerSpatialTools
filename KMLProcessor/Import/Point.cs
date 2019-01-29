using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about a Point extracted from the KML file
	/// </summary>
	public class Point : Geography
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public Point()
			: this(0, 0, null, null)
		{
		}

		/// <summary>
		/// Constructor. Constructs the point using the given longitude and latitude.
		/// </summary>
		/// <param name="longitude">Longitude</param>
		/// <param name="latitude">Latitude</param>
		public Point(double longitude, double latitude)
			: this(longitude, latitude, null, null)
		{
		}

		/// <summary>
		/// Constructor. Constructs the point using the given longitude, latitude and altitude
		/// </summary>
		/// <param name="longitude">Longitude</param>
		/// <param name="latitude">Latitude</param>
		/// <param name="altitude">Altitude</param>
		public Point(double longitude, double latitude, double? altitude)
			: this(longitude, latitude, altitude, null)
		{
		}

		/// <summary>
		/// Constructor. Constructs the point using the given longitude, latitude, altitude and measure.
		/// </summary>
		/// <param name="longitude">Longitude</param>
		/// <param name="latitude">Latitude</param>
		/// <param name="altitude">Altitude</param>
		/// <param name="measure">Measure</param>
		public Point(double longitude, double latitude, double? altitude, double? measure)
		{
			Longitude = longitude;
			Latitude = latitude;
			Altitude = altitude;
			Measure = measure;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Longitude.
		/// </summary>
		public double Longitude
		{
			get { return m_Longitude; }
			set
			{
				m_Longitude = value;
			}
		}
		/// <summary>
		/// Data member for the Longitude property
		/// </summary>
		protected double m_Longitude = 0;

		/// <summary>
		/// Latitude.
		/// </summary>
		public double Latitude
		{
			get { return m_Latitude; }
			set
			{
				m_Latitude = value;
			}
		}
		/// <summary>
		/// Data member for the Latitude property
		/// </summary>
		protected double m_Latitude = 0;

		/// <summary>
		/// Altitude.
		/// </summary>
		public double? Altitude
		{
			get { return m_Altitude; }
			set
			{
				m_Altitude = value;
			}
		}
		/// <summary>
		/// Data member for the Altitude property
		/// </summary>
		protected double? m_Altitude = null;

		/// <summary>
		/// Measure
		/// </summary>
		public double? Measure
		{
			get { return m_Measure; }
			set { m_Measure = value; }
		}
		/// <summary>
		/// Data member for the Measure property
		/// </summary>
		protected double? m_Measure = null;

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				string wkt = String.Format("POINT({0})", Coordinate);

				return wkt;
			}
		}

		/// <summary>
		/// Point's coordinate in the format: longitude, latitude [, altitude]
		/// </summary>
		public string Coordinate
		{
			get
			{
				string wkt = String.Format("{0} {1}", m_Longitude, m_Latitude);

				if (m_Altitude.HasValue)
					wkt += " " + m_Altitude.Value;

				if (m_Measure.HasValue)
					wkt += " " + m_Measure.Value;

				return wkt;
			}
		}

		#endregion

		#region Geography methods

		/// <summary>
		/// This method populates the given sink with the data about this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(IGeographySink110 sink)
		{
			sink.BeginGeography(OpenGisGeographyType.Point);
			sink.BeginFigure(Latitude, Longitude, Altitude, Measure);

			sink.EndFigure();
			sink.EndGeography();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method clones this point
		/// </summary>
		/// <returns>Clone of this point</returns>
		public Point Clone()
		{
			Point c = new Point();

			c.Longitude = Longitude;
			c.Latitude = Latitude;
			c.Altitude = Altitude;
			c.Measure = Measure;

			return c;
		}

		#endregion

		#region Overriden Base Methods

		/// <summary>
		/// This method compares the given object with this point
		/// </summary>
		/// <param name="obj">Object to be compared with this point</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;

			Point rhs = obj as Point;
			if (rhs == null)
				return false;

			return (m_Longitude == rhs.m_Longitude &&
					m_Latitude == rhs.m_Latitude &&
					((m_Altitude == null && rhs.m_Altitude == null) || (m_Altitude.Value == rhs.m_Altitude.Value)) &&
					((m_Measure == null && rhs.m_Measure == null) || (m_Measure.Value == rhs.m_Measure.Value)));
		}

		#endregion
	}
}
