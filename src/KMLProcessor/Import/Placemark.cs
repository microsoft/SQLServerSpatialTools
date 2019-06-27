//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// This class contains the information about a placemark extracted from the KML file
    /// </summary>
    public class Placemark
    {
        #region Public Properties

        /// <summary>
        /// Identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get => _mName;
            set => _mName = string.IsNullOrEmpty(value) ? "" : value;
        }
        /// <summary>
        /// Data member for the property Name
        /// </summary>
        private string _mName = "";

        /// <summary>
        /// Description
        /// </summary>
        public string Description
        {
            get => _mDescription;
            set => _mDescription = string.IsNullOrEmpty(value) ? "" : value;
        }
        /// <summary>
        /// Data member for the Description property
        /// </summary>
        private string _mDescription = "";

        /// <summary>
        /// Address
        /// </summary>
        public string Address
        {
            get => _mAddress;
            set => _mAddress = string.IsNullOrEmpty(value) ? "" : value;
        }
        /// <summary>
        /// Data member for the Address property
        /// </summary>
        private string _mAddress = "";

        /// <summary>
        /// The geography instance which describes this placemark
        /// </summary>
        public Geography Geography { get; set; }

        /// <summary>
        /// The geography instance where this placemark looks at
        /// </summary>
        public Geography LookAt { get; set; }

        #endregion
    }
}
