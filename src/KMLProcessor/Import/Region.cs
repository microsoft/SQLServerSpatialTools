//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Import
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
        public Geography Box { private get; set; }

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                if (Box != null)
                    return Box.WKT;

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
            Box?.Populate(sink);
        }

        #endregion
    }
}
