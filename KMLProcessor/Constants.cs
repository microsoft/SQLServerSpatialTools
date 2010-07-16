using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	public class Constants
	{
		/// <summary>
		/// SQL Server default SRID. SQL Server uses the default SRID of 4326, 
		/// which maps to the WGS 84 spatial reference system, when using methods on geography instances.
		///
		/// Source: http://msdn.microsoft.com/en-us/library/bb964707.aspx
		/// </summary>
		public static int DefaultSRID
		{
			get
			{
				return 4326;
			}
		}

		/// <summary>
		/// Google's KML extensions namespace
		/// </summary>
		public static string GxNamespace
		{
			get
			{
				return "http://www.google.com/kml/ext/2.2";
			}
		}

		/// <summary>
		/// KML namespace
		/// </summary>
		public static string KmlNamespace
		{
			get
			{
				return "http://www.opengis.net/kml/2.2";
			}
		}

		/// <summary>
		/// Atom namespace
		/// </summary>
		public static string AtomNamespace
		{
			get
			{
				return "http://www.w3.org/2005/Atom";
			}
		}
	}
}
