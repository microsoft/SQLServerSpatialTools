using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the information about a polygon extracted from the KML file
	/// </summary>
	public class Polygon : Geography
	{
		#region Public Properties

		/// <summary>
		/// Outer border
		/// </summary>
		public LinearRing OuterRing
		{
			get { return m_OuterRing; }
			set
			{
				m_OuterRing = value;
			}
		}
		/// <summary>
		/// Data member for the OuterRing property
		/// </summary>
		protected LinearRing m_OuterRing = null;

		/// <summary>
		/// Inner borders/rings
		/// </summary>
		public IList<LinearRing> InnerRing
		{
			get { return m_InnerRing; }
		}
		/// <summary>
		/// Data member for the InnerRing property
		/// </summary>
		public List<LinearRing> m_InnerRing = new List<LinearRing>();

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public override string WKT
		{
			get
			{
				string wkt = "POLYGON(";

				if (m_OuterRing != null)
					wkt += m_OuterRing.Vertices;
				else
					throw new Exception("Outer ring is not set.");

				foreach (LinearRing ir in m_InnerRing)
				{
					wkt += ", " + ir.Vertices;
				}

				wkt += ")";

				return wkt;
			}
		}

		#endregion

		#region Geography Methods

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public override void Populate(IGeographySink110 sink)
		{
			if (this.OuterRing == null || this.OuterRing.Points == null || this.OuterRing.Points.Count == 0)
				return;

			sink.BeginGeography(OpenGisGeographyType.Polygon);

			// Populates the outer boundary
			sink.BeginFigure(
						this.OuterRing.Points[0].Latitude,
						this.OuterRing.Points[0].Longitude,
						this.OuterRing.Points[0].Altitude,
						this.OuterRing.Points[0].Measure);

			for (int i = 1; i < this.OuterRing.Points.Count; i++)
			{
				sink.AddLine(
						this.OuterRing.Points[i].Latitude,
						this.OuterRing.Points[i].Longitude,
						this.OuterRing.Points[i].Altitude,
						this.OuterRing.Points[i].Measure);
			}

			sink.EndFigure();

			if (this.InnerRing != null && this.InnerRing.Count > 0)
			{
				// Populates the inner boundaries

				for (int j = 0; j < this.InnerRing.Count; j++)
				{
					if (this.InnerRing[j].Points == null || this.InnerRing[j].Points.Count == 0)
						continue;

					sink.BeginFigure(
							this.InnerRing[j].Points[0].Latitude,
							this.InnerRing[j].Points[0].Longitude,
							this.InnerRing[j].Points[0].Altitude,
							this.InnerRing[j].Points[0].Measure);

					for (int i = 1; i < this.InnerRing[j].Points.Count; i++)
					{
						sink.AddLine(
								this.InnerRing[j].Points[i].Latitude,
								this.InnerRing[j].Points[i].Longitude,
								this.InnerRing[j].Points[i].Altitude,
								this.InnerRing[j].Points[i].Measure);
					}

					sink.EndFigure();
				}
			}

			sink.EndGeography();
		}

		#endregion
	}
}

