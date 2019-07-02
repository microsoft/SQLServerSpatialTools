//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// Base class for the all KML boxes. All KML boxes will be converted to polygon instances.
    /// </summary>
    public class LatLonBoxBase : Geography
    {
        #region Public data

        /// <summary>
        /// The north side latitude
        /// </summary>
        public double North { private get; set; }

        /// <summary>
        /// The south side latitude
        /// </summary>
        public double South { private get; set; }

        /// <summary>
        /// The east side longitude
        /// </summary>
        public double East { private get; set; }

        /// <summary>
        /// The west side longitude
        /// </summary>
        public double West { private get; set; }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Altitude
        /// </summary>
        public double? Altitude { private get; set; }

        /// <summary>
        /// Measure
        /// </summary>
        public double? Measure { private get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT
        {
            get
            {
                CheckCoordinates();

                var wkt = "polygon((";

                // (west, south)
                wkt += new Point(West, South, Altitude, Measure).Coordinate;

                // (east, south)
                wkt += "," + new Point(East, South, Altitude, Measure).Coordinate;

                // (east, north)
                wkt += "," + new Point(East, North, Altitude, Measure).Coordinate;

                // (west, north)
                wkt += "," + new Point(West, North, Altitude, Measure).Coordinate;

                // (west, south)
                wkt += "," + new Point(West, South, Altitude, Measure).Coordinate;
                
                wkt += "))";
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
            CheckCoordinates();

            // The coordinates for this geography instance will be:
            // (west, south), (east, south), (east, north), (west, north), (west, south)
            sink.BeginGeography(OpenGisGeographyType.Polygon);

            // (west, south)
            sink.BeginFigure(South, West, Altitude, Measure);

            // (east, south)
            sink.AddLine(South, East, Altitude, Measure);

            // (east, north)
            sink.AddLine(North, East, Altitude, Measure);

            // (west, north)
            sink.AddLine(North, West, Altitude, Measure);

            // (west, south)
            sink.AddLine(South, West, Altitude, Measure);

            sink.EndFigure();

            sink.EndGeography();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// This method checks if the south latitude is less then the north latitude. 
        /// If not then the south and the north will be swapped.
        /// This method also checks if the west longitude if less then the east longitude.
        /// If not then the east and the west will be swapped.
        /// </summary>
        protected void CheckCoordinates()
        {
            // Checks if the South and the North latitude are in the range (-90, 90)
            South = Utilities.ShiftInRange(South, 90);
            North = Utilities.ShiftInRange(North, 90);

            // Checks if the West and the East longitude are in the range (-180, 180)
            West = Utilities.ShiftInRange(West, 180);
            East = Utilities.ShiftInRange(East, 180);

            // Ensures that the south is less then the north
            if (South > North)
            {
                var tmp = South;
                South = North;
                North = tmp;
            }

            // Ensures that the west is less then the east
            if (West > East)
            {
                var tmp = West;
                West = East;
                East = tmp;
            }
        }

        #endregion
    }
}
