//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.KMLProcessor.Import
{
#pragma warning disable CS0659 
    // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    /// <summary>
    /// This class holds the information about a Point extracted from the KML file
    /// </summary>
    public class Point : Geography
#pragma warning restore CS0659
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Point() : this(0, 0)
        {
        }

        /// <summary>
        /// Constructor. Constructs the point using the given longitude, latitude, altitude and measure.
        /// </summary>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <param name="altitude">Altitude</param>
        /// <param name="measure">Measure</param>
        public Point(double longitude, double latitude, double? altitude = null, double? measure = null)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
            Measure = measure;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Longitude.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Latitude.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Altitude.
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Measure
        /// </summary>
        public double? Measure { get; set; }

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                var wkt = $"POINT({Coordinate})";

                return wkt;
            }
        }

        /// <summary>
        /// Point's coordinate in the format: longitude, latitude [, altitude]
        /// </summary>
        public string Coordinate
        {
            get
            {
                var wkt = $"{Longitude} {Latitude}";

                if (Altitude.HasValue)
                    wkt += " " + Altitude.Value;

                if (Measure.HasValue)
                    wkt += " " + Measure.Value;

                return wkt;
            }
        }

        #endregion

        #region Geography methods

        /// <summary>
        /// This method populates the given sink with the data about this geography instance
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        public override void Populate(IGeographySink110 sink)
        {
            sink.BeginGeography(OpenGisGeographyType.Point);
            sink.BeginFigure(Latitude, Longitude, Altitude, Measure);

            sink.EndFigure();
            sink.EndGeography();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method clones this point
        /// </summary>
        /// <returns>Clone of this point</returns>
        public Point Clone()
        {
            return new Point
            {
                Longitude = Longitude,
                Latitude = Latitude,
                Altitude = Altitude,
                Measure = Measure
            };
        }

        #endregion

        #region Overridden Base Methods

        /// <summary>
        /// This method compares the given object with this point
        /// </summary>
        /// <param name="obj">Object to be compared with this point</param>
        /// <returns></returns>
#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            if (this == obj)
                return true;

            if (!(obj is Point rhs))
                return false;

            return Longitude.EqualsTo(rhs.Longitude) &&
                   Latitude.EqualsTo(rhs.Latitude) &&
                   // ReSharper disable PossibleInvalidOperationException
                   (Altitude == null && rhs.Altitude == null || Altitude.Value.EqualsTo(rhs.Altitude.Value)) &&
                   (Measure == null && rhs.Measure == null || (Measure.Value.EqualsTo(rhs.Measure.Value)));
                   // ReSharper restore PossibleInvalidOperationException
        }

        #endregion
    }
}
