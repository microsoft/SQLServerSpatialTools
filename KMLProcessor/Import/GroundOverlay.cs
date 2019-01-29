using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about the ground overlay extracted from the KML file
	/// </summary>
	public class GroundOverlay : Geography
	{
		#region Public Properties

		/// <summary>
		/// Ground Overlay Name
		/// </summary>
		public string Name
		{
			get { return m_Name; }
			internal set
			{
				if (string.IsNullOrEmpty(value))
					m_Name = "";
				else
					m_Name = value;
			}
		}
		/// <summary>
		/// Data member for the Name property
		/// </summary>
		protected string m_Name = "";

		/// <summary>
		/// Box defined inside this ground overlay. 
		/// </summary>
		public Geography Box
		{
			get { return m_Box; }
			internal set
			{
				m_Box = value;
			}
		}
		/// <summary>
		/// Data member for the Box property
		/// </summary>
		protected Geography m_Box = null;

		/// <summary>
		/// Region defined inside this ground overlay.
		/// </summary>
		public Region Region
		{
			get { return m_Region; }
			internal set
			{
				m_Region = value;
			}
		}
		/// <summary>
		/// Data member for the Region property
		/// </summary>
		protected Region m_Region;

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
				else if (Region != null)
				{
					return Region.WKT;
				}
				else
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
			else if (Region != null)
			{
				Region.Populate(sink);
			}
		}

		#endregion
	}
}
