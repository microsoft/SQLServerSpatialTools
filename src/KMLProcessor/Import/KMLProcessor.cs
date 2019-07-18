//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.SqlServer.Types;
// ReSharper disable NotAccessedField.Local

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class processes the given KML string and produces the equivalent SqlGeography instance
    /// using sinks.
    /// </summary>
    public class KMLProcessor
    {
        #region Constructors

        /// <summary>
        /// Constructor. Accepts the KML string.
        /// </summary>
        /// <param name="kml">KML data to be parsed</param>
        public KMLProcessor(string kml)
        {
            _kml = kml;
            _parser = new KMLParser(kml);
            _parser.Parse();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Spatial reference identifier (SRID).
        /// </summary>
        // ReSharper disable MemberCanBePrivate.Global
        public int Srid { get; set; } = Constants.DefaultSRID;

        /// <summary>
        /// Placemarks extracted from the given KML string
        /// </summary>
        public IList<Placemark> Placemarks => _parser.Placemarks;

        /// <summary>
        /// Models extracted from the given KML string
        /// </summary>
        public IList<Model> Models => _parser.Models;

        /// <summary>
        /// Regions extracted from the given KML string
        /// </summary>
        public IList<Region> Regions => _parser.Regions;

        /// <summary>
        /// Geography instances extracted from the given KML string
        /// </summary>
        public IEnumerable<Geography> Geographies => _parser.Geographies;

        /// <summary>
        /// Ground overlays extracted from the given KML string
        /// </summary>
        public IList<GroundOverlay> GroundOverlays => _parser.GroundOverlays;

        #endregion

        #region Private Data

        /// <summary>
        /// KML/XML parser
        /// </summary>
        private readonly KMLParser _parser;

        /// <summary>
        /// KML string to be parsed
        /// </summary>
        private readonly string _kml;

        #endregion

        #region Public Methods

        /// <summary>
        /// This method populates the given sink with the information about the geography instance
        /// extracted from the KML string
        /// </summary>
        /// <param name="sink">Sink to be filled</param>
        /// <param name="makeValid">If true and the extracted geography instance is invalid then the MakeValid
        /// function will be executed on the extracted geography instance</param>
        public void Populate(
            IGeographySink110 sink,
            bool makeValid)
        {
            sink.SetSrid(Srid);

            var numOfGeographies = _parser.Geographies.Count;
            if (numOfGeographies == 1)
            {
                _parser.Geographies[0].Populate(sink, makeValid);
            }
            else if (numOfGeographies > 1)
            {
                sink.BeginGeography(OpenGisGeographyType.GeometryCollection);

                foreach (var g in _parser.Geographies)
                {
                    g.Populate(sink, makeValid);
                }

                sink.EndGeography();
            }
            else
            {
                // Geography instance is not found. The empty geography collection will be generated.
                sink.BeginGeography(OpenGisGeographyType.GeometryCollection);
                sink.EndGeography();
            }
        }

        #endregion
    }
}
