using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// Base class for the all KML boxes. All KML boxes will be converted to polygon instances.
	/// </summary>
	public class LatLonBoxBase : Geography
	{
		#region Public data

		/// <summary>
		/// The north side latitude
		/// </summary>
		public double North
		{
			get { return m_North; }
			set
			{
				m_North = value;
			}
		}
		/// <summary>
		/// Data member for the North property
		/// </summary>
		protected double m_North = 0;

		/// <summary>
		/// The south side latitude
		/// </summary>
		public double South
		{
			get { return m_South; }
			set
			{
				m_South = value;
			}
		}
		/// <summary>
		/// Data member for the South property
		/// </summary>
		protected double m_South = 0;

		/// <summary>
		/// The east side longitude
		/// </summary>
		public double East
		{
			get { return m_East; }
			set
			{
				m_East = value;
			}
		}
		/// <summary>
		/// Data member for the East property
		/// </summary>
		protected double m_East = 0;

		/// <summary>
		/// The west side longitude
		/// </summary>
		public double West
		{
			get { return m_West; }
			set
			{
				m_West = value;
			}
		}
		/// <summary>
		/// Data member for the West property
		/// </summary>
		protected double m_West = 0;

		/// <summary>
		/// Altitude
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
			set
			{
				m_Measure = value;
			}
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
				CheckCoordinates();

				string wkt = "polygon((";

				// (west, south)
				wkt += new Point(West, South, m_Altitude, m_Measure).Coordinate;

				// (east, south)
				wkt += "," + new Point(East, South, m_Altitude, m_Measure).Coordinate;

				// (east, notrh)
				wkt += "," + new Point(East, North, m_Altitude, m_Measure).Coordinate;

				// (west, north)
				wkt += "," + new Point(West, North, m_Altitude, m_Measure).Coordinate;

				// (west, south)
				wkt += "," + new Point(West, South, m_Altitude, m_Measure).Coordinate;
				
				wkt += "))";
				return wkt;
			}
		}

		#endregion

		#region Geography Methods

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(Microsoft.SqlServer.Types.IGeographySink110 sink)
		{
			CheckCoordinates();

			// The coordinates for this geography instance will be:
			// (west, south), (east, south), (east, notrh), (west, north), (west, south)
			sink.BeginGeography(OpenGisGeographyType.Polygon);

			// (west, south)
			sink.BeginFigure(South, West, m_Altitude, m_Measure);

			// (east, south)
			sink.AddLine(South, East, m_Altitude, m_Measure);

			// (east, notrh)
			sink.AddLine(North, East, m_Altitude, m_Measure);

			// (west, north)
			sink.AddLine(North, West, m_Altitude, m_Measure);

			// (west, south)
			sink.AddLine(South, West, m_Altitude, m_Measure);

			sink.EndFigure();

			sink.EndGeography();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// This method checks if the south latitude is less then the north latitude. 
		/// If not then the south and the north will be swapped.
		/// This method also checks if the west longitude if less then the east longitude.
		/// If not then the east and the west will be swapped.
		/// </summary>
		protected void CheckCoordinates()
		{
			// Checks if the South and the North latitude are in the range (-90, 90)
			South = Utilities.ShiftInRange(South, 90);
			North = Utilities.ShiftInRange(North, 90);

			// Checks if the West and the East longitude are in the range (-180, 180)
			West = Utilities.ShiftInRange(West, 180);
			East = Utilities.ShiftInRange(East, 180);

			// Ensures that the south is less then the north
			if (South > North)
			{
				double tmp = South;
				South = North;
				North = tmp;
			}

			// Ensures that the west is less then the east
			if (West > East)
			{
				double tmp = West;
				West = East;
				East = tmp;
			}
		}

		#endregion
	}
}
