//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Export
{
	/// <summary>
	/// The base class for all KML exporters
	/// </summary>
	public class KeyholeMarkupLanguageBase
	{
		#region Constructors

		/// <summary>
		/// Constructor. Creates a KeyholeMarkupLanguageBase object which will fill 
		/// the given xml writer with a spatial data in the KML format
		/// </summary>
		/// <param name="writer">The Xml writer to be filled with a spatial data in the KML format</param>
        protected KeyholeMarkupLanguageBase(System.Xml.XmlWriter writer)
		{
			Writer = writer;

			BeginKMLDocument();
		}

		#endregion

		#region Protected helper methods

		/// <summary>
		/// This method will create the necessary start elements in the KML file
		/// </summary>
        private void BeginKMLDocument()
		{
			// Creates the beginning of the KML file as:

			Writer.WriteStartElement("kml", Constants.KmlNamespace);
			Writer.WriteAttributeString("xmlns", "gx", null, Constants.GxNamespace);
			Writer.WriteAttributeString("xmlns", "kml", null, Constants.KmlNamespace);
			Writer.WriteAttributeString("xmlns", "atom", null, Constants.AtomNamespace);

			StartElement("Document");								//	<Document>
			StartElement("name");									//		<name>
			WriteString("Generated document");						//			Generated document
			EndElement();											//		</name>

			StartElement("Placemark");								//		<Placemark>
			StartElement("name");									//			<name>
			WriteString("Placemark exported from sql spatial");		//				Placemark exported from sql spatial
			EndElement();											//		</name>
		}

		/// <summary>
		/// This method will start a tag for xml element with the given name
		/// </summary>
		/// <param name="name">Xml element name to be created</param>
		protected void StartElement(string name)
		{
			Writer.WriteStartElement(name);
		}

		/// <summary>
		/// This method will close the last opened (and not closed) element. (A end tag will be added)
		/// </summary>
		protected void EndElement()
		{
			Writer.WriteEndElement();
		}

		/// <summary>
		/// This method will write the given text into the xml writer
		/// </summary>
		/// <param name="text">The text to be written</param>
		protected void WriteString(string text)
		{
			Writer.WriteString(text);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method will close this sink. It will close all xml elements which
		/// were created during sink population.
		/// </summary>
		public void FinalizeKMLDocument()
		{
			EndElement();	//			</Placemark>
			EndElement();	//		</Document>
			EndElement();	//	</Kml>
		}

		/// <summary>
		/// This method will set the spatial reference system identifier (SrId)
		/// </summary>
		/// <param name="srid">Spatial reference system identifier (SrId)</param>
		public void SetSrid(int srid)
		{
			SRID = srid;
		}

		#endregion

		#region Protected Data

		/// <summary>
		/// Xml writer to be filled with data
		/// </summary>
		protected readonly System.Xml.XmlWriter Writer;

		/// <summary>
		/// Spatial reference system identifier (SrId) for this file
		/// </summary>
        protected int SRID;

		#endregion
	}
}
