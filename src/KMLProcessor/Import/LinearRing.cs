using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the data about a linear ring extracted from the KML file
	/// </summary>
	public class LinearRing : LineString
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public LinearRing()
		{
			m_OpenGisGeographyType = OpenGisGeographyType.Polygon;
		}

		#endregion

		#region Public Mehods

		/// <summary>
		/// This method switchs the ring's orientation
		/// </summary>
		public void SwitchOrientation()
		{
			int i = 1;
			int j = Points.Count - 2; // index of the element before last
			while (i < j)
			{
				Point tmp = m_Points[i];
				m_Points[i] = m_Points[j];
				m_Points[j] = tmp;

				i++;
				j--;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				return "POLYGON(" + Vertices + ")";
			}
		}

		#endregion
	}
}
