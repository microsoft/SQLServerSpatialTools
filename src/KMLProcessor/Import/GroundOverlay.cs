//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Import
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
            get => _name;
            internal set => _name = string.IsNullOrEmpty(value) ? "" : value;
        }
        /// <summary>
        /// Data member for the Name property
        /// </summary>
        private string _name = "";

        /// <summary>
        /// Box defined inside this ground overlay. 
        /// </summary>
        public Geography Box { private get; set; }

        /// <summary>
        /// Region defined inside this ground overlay.
        /// </summary>
        public Region Region { private get; set; }

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

                if (Region != null)
                {
                    return Region.WKT;
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
            else
            {
                Region?.Populate(sink);
            }
        }

        #endregion
    }
}
