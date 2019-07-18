//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class holds the information about the LatLonBox instance extracted from the KML file
    /// </summary>
    public class LatLonBox : LatLonBoxBase
    {
        #region Public Data

        /// <summary>
        /// The rotation angle.
        /// </summary>
        public double Rotation { get; set; }

        #endregion
    }
}
