using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about a Region extracted from the KML file
	/// </summary>
	public class Region : Geography
	{
		#region Public Data

		/// <summary>
		/// Box which defines the border of this region
		/// </summary>
		public Geography Box
		{
			get { return m_Box; }
			set { m_Box = value; }
		}
		/// <summary>
		/// Data member for the Box property
		/// </summary>
		protected Geography m_Box;

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				if (Box != null)
				{
					return Box.WKT;
				}

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
			if (Box != null)
			{
				Box.Populate(sink);
			}
		}

		#endregion
	}
}
