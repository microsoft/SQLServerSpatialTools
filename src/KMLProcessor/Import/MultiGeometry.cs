using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class holds the information about a collection of the KML geographies
	/// </summary>
	public class MultiGeometry : Geography
	{
		#region Public Properties

		/// <summary>
		/// Geographies contained in this geography instance
		/// </summary>
		public IList<Geography> Geographies
		{
			get
			{
				return m_Geographies;
			}
		}
		/// <summary>
		/// Data member for the Geographies property
		/// </summary>
		protected List<Geography> m_Geographies = new List<Geography>();

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				string wkt = "GEOMETRYCOLLECTION(";

				bool firstGeography = true;
				foreach (Geography g in m_Geographies)
				{
					if (firstGeography)
						firstGeography = false;
					else
						wkt += ", ";

					wkt += g.WKT;
				}

				wkt += ")";
				return wkt;
			}
		}

		#endregion

		#region Geography Methods

		/// <summary>
		/// This method populates the given sink with the information about this multy geometry instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(Microsoft.SqlServer.Types.IGeographySink110 sink)
		{
			sink.BeginGeography(OpenGisGeographyType.GeometryCollection);

			if (this.Geographies != null && this.Geographies.Count > 0)
			{
				foreach (Geography g in this.Geographies)
				{
					g.Populate(sink);
				}
			}

			sink.EndGeography();
		}

		#endregion
	}
}
