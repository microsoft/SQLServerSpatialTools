//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class contains the information about the LatLonAltBox element extracted from KML
    /// </summary>
    public class LatLonAltBox : LatLonBoxBase
    {
        #region Public Data

        /// <summary>
        /// The minimal altitude
        /// </summary>
        public double MinAltitude
        {
            get => _minAltitude;
            set
            {
                if (value < 0)
                    throw new KMLException("Altitude has to be greater than 0");

                _minAltitude = value;

                if (_maxAltitude < _minAltitude)
                    _maxAltitude = _minAltitude;
            }
        }
        /// <summary>
        /// Data member for the MinAltitude property
        /// </summary>
        private double _minAltitude;

        /// <summary>
        /// The maximal altitude
        /// </summary>
        public double MaxAltitude
        {
            get => _maxAltitude;
            set
            {
                if (value < 0)
                    throw new KMLException("Altitude has to be greater than 0");

                _maxAltitude = value;
                if (_minAltitude > _maxAltitude)
                    _minAltitude = _maxAltitude;
            }
        }
        /// <summary>
        /// Data member for the property MaxAltitude
        /// </summary>
        private double _maxAltitude;

        /// <summary>
        /// Altitude mode
        /// </summary>
        public AltitudeMode? AltitudeMode { get; set; }

        #endregion

        #region Geography Methods

        /// <summary>
        /// This method populates the given sink with the data from this geography instance
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        public override void Populate(Microsoft.SqlServer.Types.IGeographySink110 sink)
        {
            // Initializes the altitude to the maximal value
            _maxAltitude = MaxAltitude;

            // Converts and stores the altitude mode to the spatial m coordinate
            if (AltitudeMode != null)
                Measure = (int)AltitudeMode.Value;

            base.Populate(sink);
        }

        #endregion
    }
}
