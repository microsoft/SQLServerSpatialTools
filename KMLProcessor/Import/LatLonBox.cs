using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about the LatLonBox instance extracted from the KML file
	/// </summary>
	public class LatLonBox : LatLonBoxBase
	{
		#region Public Data

		/// <summary>
		/// The rotation angle.
		/// </summary>
		public double Rotation
		{
			get { return m_Rotation; }
			set
			{
				m_Rotation = value;
			}
		}
		/// <summary>
		/// Data member for the property Rotation
		/// </summary>
		protected double m_Rotation = 0;

		#endregion
	}
}
