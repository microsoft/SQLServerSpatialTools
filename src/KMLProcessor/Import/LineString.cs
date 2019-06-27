//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class contains the information about a line string extracted from the KML file
    /// </summary>
    public class LineString : Geography
    {
        #region Private Fields

        /// <summary>
        /// OpenGisGeographyType for this geography instance
        /// </summary>
        protected OpenGisGeographyType GeographyType = OpenGisGeographyType.LineString;

        #endregion

        #region Public Properties

        /// <summary>
        /// The list of points
        /// </summary>
        public List<Point> Points { get; } = new List<Point>();

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public override string WKT => "LINESTRING" + Vertices;

        /// <summary>
        /// Returns the vertices of this linear ring. The returned string will be in the following format:
        /// (V0, V1, V2,..., Vn, V0). Vi will be in the format: longitude latitude [altitude [measure]]
        /// </summary>
        public string Vertices
        {
            get
            {
                var result = "(";
                var isFirst = true;
                foreach (var p in Points)
                {
                    if (!isFirst)
                        result += ", ";
                    else
                        isFirst = false;

                    result += p.Longitude + " " + p.Latitude;
                    if (p.Altitude.HasValue)
                    {
                        result += " " + p.Altitude.Value;

                        if (p.Measure.HasValue)
                            result += " " + p.Measure.Value;
                    }
                }
                result += ")";

                return result;
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
            if (Points == null || Points.Count == 0)
                return;

            sink.BeginGeography(GeographyType);
            sink.BeginFigure(Points[0].Latitude, Points[0].Longitude, Points[0].Altitude, Points[0].Measure);

            for (var i = 1; i < Points.Count; i++)
            {
                sink.AddLine(Points[i].Latitude, Points[i].Longitude, Points[i].Altitude, Points[i].Measure);
            }

            sink.EndFigure();
            sink.EndGeography();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method stores the tessellate flag. M coordinates of all the point will be set
        /// to the "clamp to ground" value.
        /// </summary>
        public void StoreTessellateFlag()
        {
            if (!Tessellate) return;

            foreach (var p in Points)
            {
                if (!p.Measure.HasValue)
                {
                    p.Measure = (int)AltitudeMode.ClampToGround;
                }
            }
        }

        #endregion
    }
}
