//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor.Import
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
        public LinearRing OuterRing { get; set; }

        /// <summary>
        /// Inner borders/rings
        /// </summary>
        public IList<LinearRing> InnerRing { get; } = new List<LinearRing>();

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                var wkt = "POLYGON(";

                if (OuterRing != null)
                    wkt += OuterRing.Vertices;
                else
                    throw new Exception("Outer ring is not set.");

                foreach (var linearRing in InnerRing)
                {
                    wkt += ", " + linearRing.Vertices;
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
            if (OuterRing?.Points == null || OuterRing.Points.Count == 0)
                return;

            sink.BeginGeography(OpenGisGeographyType.Polygon);

            // Populates the outer boundary
            sink.BeginFigure(
                        OuterRing.Points[0].Latitude,
                        OuterRing.Points[0].Longitude,
                        OuterRing.Points[0].Altitude,
                        OuterRing.Points[0].Measure);

            for (var i = 1; i < OuterRing.Points.Count; i++)
            {
                sink.AddLine(
                        OuterRing.Points[i].Latitude,
                        OuterRing.Points[i].Longitude,
                        OuterRing.Points[i].Altitude,
                        OuterRing.Points[i].Measure);
            }

            sink.EndFigure();

            if (InnerRing != null && InnerRing.Count > 0)
            {
                // Populates the inner boundaries

                foreach (var linearRing in InnerRing)
                {
                    if (linearRing.Points == null || linearRing.Points.Count == 0)
                        continue;

                    sink.BeginFigure(
                        linearRing.Points[0].Latitude,
                        linearRing.Points[0].Longitude,
                        linearRing.Points[0].Altitude,
                        linearRing.Points[0].Measure);

                    for (var i = 1; i < linearRing.Points.Count; i++)
                    {
                        sink.AddLine(
                            linearRing.Points[i].Latitude,
                            linearRing.Points[i].Longitude,
                            linearRing.Points[i].Altitude,
                            linearRing.Points[i].Measure);
                    }

                    sink.EndFigure();
                }
            }

            sink.EndGeography();
        }

        #endregion
    }
}
