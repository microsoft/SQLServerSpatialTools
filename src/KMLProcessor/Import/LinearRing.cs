//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class contains the data about a linear ring extracted from the KML file
    /// </summary>
    public class LinearRing : LineString
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LinearRing()
        {
            GeographyType = OpenGisGeographyType.Polygon;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method switches the ring's orientation
        /// </summary>
        public void SwitchOrientation()
        {
            var i = 1;
            var j = Points.Count - 2; // index of the element before last
            while (i < j)
            {
                var tmp = Points[i];
                Points[i] = Points[j];
                Points[j] = tmp;

                i++;
                j--;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT => "POLYGON(" + Vertices + ")";

        #endregion
    }
}
