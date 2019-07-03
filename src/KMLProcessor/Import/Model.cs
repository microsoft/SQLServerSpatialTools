using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about a KML Model element
	/// </summary>
	public class Model : Geography
	{
		#region Public Data

		/// <summary>
		/// The location of the model
		/// </summary>
		public Point Location
		{
			get { return m_Location; }
			set
			{
				m_Location = value;
			}
		}
		/// <summary>
		/// Data member for the Location property
		/// </summary>
		protected Point m_Location = null;

		/// <summary>
		/// The orientation heading
		/// </summary>
		public double OrientationHeading
		{
			get { return m_OrientationHeading;  }
			set { m_OrientationHeading = value; }
		}
		/// <summary>
		/// Data member for the OrientationHeading property
		/// </summary>
		protected double m_OrientationHeading;

		/// <summary>
		/// The orientation tilt
		/// </summary>
		public double OrientationTilt
		{
			get { return m_OrientationTilt; }
			set { m_OrientationTilt = value; }
		}
		/// <summary>
		/// Data member for the OrientationTilt property
		/// </summary>
		protected double m_OrientationTilt;

		/// <summary>
		/// The orientation roll
		/// </summary>
		public double OrientationRoll
		{
			get { return m_OrientationRoll; }
			set { m_OrientationRoll = value; }
		}
		/// <summary>
		/// Data member for the OrientationRoll property
		/// </summary>
		protected double m_OrientationRoll;

		/// <summary>
		/// The x-axis scale factor
		/// </summary>
		public double ScaleX
		{
			get { return m_ScaleX; }
			set { m_ScaleX = value; }
		}
		/// <summary>
		/// Data member for the ScaleX property
		/// </summary>
		protected double m_ScaleX;

		/// <summary>
		/// The y-axis scale factor
		/// </summary>
		public double ScaleY
		{
			get { return m_ScaleY; }
			set { m_ScaleY = value; }
		}
		/// <summary>
		/// Data member for the ScaleY property
		/// </summary>
		protected double m_ScaleY;

		/// <summary>
		/// The z-axis scale factor
		/// </summary>
		public double ScaleZ
		{
			get { return m_ScaleZ; }
			set { m_ScaleZ = value; }
		}
		/// <summary>
		/// Data member for the ScaleZ property
		/// </summary>
		protected double m_ScaleZ;

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				if (Location != null)
					return Location.WKT;

				throw new KMLException("WKT is not defined.");
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
			if (Location != null)
			{
				Location.Populate(sink);
			}
		}

		#endregion
	}
}
