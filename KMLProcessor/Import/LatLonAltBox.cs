using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the information about the LatLonAltBox element extracted from KML
	/// </summary>
	public class LatLonAltBox : LatLonBoxBase
	{
		#region Public Data

		/// <summary>
		/// The minimal altitude
		/// </summary>
		public double MinAltitude
		{
			get { return m_MinAltitude; }
			set
			{
				if (value < 0)
					throw new KMLException("Altitude has to be greater than 0");

				m_MinAltitude = value;

				if (m_MaxAltitude < m_MinAltitude)
					m_MaxAltitude = m_MinAltitude;
			}
		}
		/// <summary>
		/// Data member for the MinAltitude property
		/// </summary>
		protected double m_MinAltitude = 0;

		/// <summary>
		/// The maximal altitude
		/// </summary>
		public double MaxAltitude
		{
			get { return m_MaxAltitude; }
			set
			{
				if (value < 0)
					throw new KMLException("Altitude has to be greater than 0");

				m_MaxAltitude = value;
				if (m_MinAltitude > m_MaxAltitude)
					m_MinAltitude = m_MaxAltitude;
			}
		}
		/// <summary>
		/// Data member for the property MaxAltitude
		/// </summary>
		protected double m_MaxAltitude = 0;

		/// <summary>
		/// Altitude mode
		/// </summary>
		public AltitudeMode? AltitudeMode
		{
			get { return m_AltitudeMode; }
			set { m_AltitudeMode = value; }
		}
		/// <summary>
		/// Data member for the AltitudeMode property
		/// </summary>
		protected AltitudeMode? m_AltitudeMode = null;

		#endregion

		#region Geography Methods

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(Microsoft.SqlServer.Types.IGeographySink sink)
		{
			// Initializes the altitude to the maximal value
			m_Altitude = MaxAltitude;

			// Converts and stores the altitude mode to the spatial m coordinate
			if (AltitudeMode != null)
				m_Measure = (int)AltitudeMode.Value;

			base.Populate(sink);
		}

		#endregion
	}
}
