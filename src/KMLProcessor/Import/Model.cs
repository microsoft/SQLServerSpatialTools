//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class holds the information about a KML Model element
    /// </summary>
    public class Model : Geography
    {
        #region Public Data

        /// <summary>
        /// The location of the model
        /// </summary>
        public Point Location { private get; set; }

        /// <summary>
        /// The orientation heading
        /// </summary>
        public double OrientationHeading { get; set; }

        /// <summary>
        /// The orientation tilt
        /// </summary>
        public double OrientationTilt { get; set; }

        /// <summary>
        /// The orientation roll
        /// </summary>
        public double OrientationRoll { get; set; }

        /// <summary>
        /// The x-axis scale factor
        /// </summary>
        public double ScaleX { get; set; }

        /// <summary>
        /// The y-axis scale factor
        /// </summary>
        public double ScaleY { get; set; }

        /// <summary>
        /// The z-axis scale factor
        /// </summary>
        public double ScaleZ { get; set; }

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                if (Location != null)
                    return Location.WKT;

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
            Location?.Populate(sink);
        }

        #endregion
    }
}
