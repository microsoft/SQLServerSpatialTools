//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor.Import
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
        public IList<Geography> Geographies { get; set; } = new List<Geography>();

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                var wkt = "GEOMETRYCOLLECTION(";

                var firstGeography = true;
                foreach (var g in Geographies)
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
        /// This method populates the given sink with the information about this multi geometry instance
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        public override void Populate(IGeographySink110 sink)
        {
            sink.BeginGeography(OpenGisGeographyType.GeometryCollection);

            if (Geographies != null && Geographies.Count > 0)
            {
                foreach (var geography in Geographies)
                {
                    geography.Populate(sink);
                }
            }

            sink.EndGeography();
        }

        #endregion
    }
}
