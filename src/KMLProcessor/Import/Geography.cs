//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using Geog = SQLSpatialTools.Functions.General.Geography;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// Base class for all geography instances extracted from the KML file
    /// </summary>
    public abstract class Geography
    {
        #region Public Properties

        /// <summary>
        /// Id of the placemark which contains this geography instance
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// True if point(s) which are part of this geography instance 
        /// should be connected to the ground when they are visually represented in some visualization tool
        /// </summary>
        public bool Extrude { get; set; }

        /// <summary>
        /// True if all lines in this geography instance should follow the terrain. 
        /// This flag is not applicable just for Point instance.
        /// </summary>
        public bool Tessellate { get; set; }

        /// <summary>
        /// True if this geography instance is valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                // This implementation is base on the property that the Sql Server 2008 (10.0 Katmai)
                // will throw an exception if the geography instance is not valid. 

                try
                {
                    var constructed = new SqlGeographyBuilder();
                    constructed.SetSrid(Constants.DefaultSRID);

                    Populate(constructed);

                    // ReSharper disable once UnusedVariable
                    var geography = constructed.ConstructedGeography;

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The context where this geography instance is found. It will
        /// contain the information about the parent and the siblings of this
        /// geography instance, and the information about this geography instance
        /// it self.
        /// </summary>
        public string Context { get; set; }

        #endregion

        #region Abstract Members

        /// <summary>
        /// This method populates the given sink with the data from this geography instance
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        public abstract void Populate(IGeographySink110 sink);

        /// <summary>
        /// SqlGeography instance well-known text.
        /// </summary>
        public abstract string WKT
        {
            get;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method returns the geography instance that corresponds to this KML geography
        /// </summary>
        /// <param name="makeValid">If true and this geography instance is invalid then the MakeValid
        /// function will be executed on this geography instance</param>
        /// <returns>The geography instance that corresponds to this KML geography</returns>
        public SqlGeography ToSqlGeography(bool makeValid)
        {
            if (IsValid)
            {
                var constructed = new SqlGeographyBuilder();
                constructed.SetSrid(Constants.DefaultSRID);
                Populate(constructed);
                return constructed.ConstructedGeography;
            }
            if (makeValid)
            {
                var constructed = new SqlGeographyBuilder();
                constructed.SetSrid(Constants.DefaultSRID);
                MakeValid(constructed);
                return constructed.ConstructedGeography;
            }

            throw new KMLException("Invalid geography instance.");
        }

        /// <summary>
        /// This method returns a string representation of this object
        /// </summary>
        /// <returns>WKT for this geography instance</returns>
        public override string ToString()
        {
            return WKT;
        }

        /// <summary>
        /// This method populates the given sink with the data from this geography instance.
        /// If this geography instance is invalid and the makeValid flag is set then a valid geography instance
        /// will be constructed and the given sink will be populated with that instance.
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        /// <param name="makeValid">If true and this geography instance is invalid then the MakeValid
        /// function will be executed on this geography instance</param>
        public void Populate(
            IGeographySink110 sink,
            bool makeValid)
        {
            if (makeValid)
            {
                if (IsValid)
                    Populate(sink);
                else
                    MakeValid(sink);
            }
            else
            {
                Populate(sink);
            }
        }

        /// <summary>
        /// This method populates the given sink with the valid geography instance constructed from this geography instance.
        /// </summary>
        /// <param name="sink">Sink to be populated</param>
        private void MakeValid(IGeographySink110 sink)
        {
            // 1. Creates the valid geography for this WKT
            var vg = Geog.MakeValidGeographyFromText(WKT, Constants.DefaultSRID);

            // 2. Populates the given sink with the valid geography
            vg.Populate(new FilterSetSridGeographySink(sink));
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Id which was assigned to this geography instance when it was inserted in the database. 
        /// If the value is null then it is not stored in the database yet
        /// </summary>
        internal int? DbId { get; set; }

        #endregion
    }
}
