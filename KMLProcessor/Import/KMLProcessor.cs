using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class processes the given KML string and produces the equivalent SqlGeography instance
	/// using sinks.
	/// </summary>
	public class KMLProcessor
	{
		#region Constructors

		/// <summary>
		/// Constructor. Accepts the KML string.
		/// </summary>
		/// <param name="kml">KML data to be parsed</param>
		public KMLProcessor(string kml)
		{
			m_Kml = kml;

			m_Parser = new KMLParser(kml);
			m_Parser.Parse();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Spatial reference identifier (SRID).
		/// </summary>
		public int Srid
		{
			get
			{
				return m_Srid;
			}
			set
			{
				m_Srid = value;
			}
		}
		/// <summary>
		/// Data member for the Srid property
		/// </summary>
		protected int m_Srid = Constants.DefaultSRID;

		/// <summary>
		/// Placemarks extracted from the given KML string
		/// </summary>
		public IList<Placemark> Placemarks
		{
			get
			{
				return m_Parser.Placemarks;
			}
		}

		/// <summary>
		/// Models extracted from the given KML string
		/// </summary>
		public IList<Model> Models
		{
			get
			{
				return m_Parser.Models;
			}
		}

		/// <summary>
		/// Regions extracted from the given KML string
		/// </summary>
		public IList<Region> Regions
		{
			get
			{
				return m_Parser.Regions;
			}
		}

		/// <summary>
		/// Geography instances extracted from the given KML string
		/// </summary>
		public IList<Geography> Geographies
		{
			get
			{
				return m_Parser.Geographies;
			}
		}

		/// <summary>
		/// Ground overlays extracted from the given KML string
		/// </summary>
		public IList<GroundOverlay> GroundOverlays
		{
			get
			{
				return m_Parser.GroundOverlays;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method populates the given sink with the information about the geography instance
		/// extracted from the KML string
		/// </summary>
		/// <param name="sink">Sink to be filled</param>
        /// <param name="makeValid">If true and the extracted geography instance is invalid then the MakeValid
        /// function will be executed on the extracted geography instance</param>
		public void Populate(
			IGeographySink110 sink,
			bool makeValid)
		{
			sink.SetSrid(m_Srid);

			int numOfGeographies = m_Parser.Geographies.Count;
			if (numOfGeographies == 1)
			{
				m_Parser.Geographies[0].Populate(sink, makeValid);
			}
			else if (numOfGeographies > 1)
			{
				sink.BeginGeography(OpenGisGeographyType.GeometryCollection);

				foreach (Geography g in m_Parser.Geographies)
				{
					g.Populate(sink, makeValid);
				}

				sink.EndGeography();
			}
			else
			{
				// Geography instance is not found. The empty geography collection will be generated.
				sink.BeginGeography(OpenGisGeographyType.GeometryCollection);
				sink.EndGeography();
			}
		}

		#endregion

		#region Protected Data

		/// <summary>
		/// KML/XML parser
		/// </summary>
		protected KMLParser m_Parser;

		/// <summary>
		/// KML string to be parsed
		/// </summary>
		protected string m_Kml;

		#endregion
	}
}
