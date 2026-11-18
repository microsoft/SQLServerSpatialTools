//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLSpatialTools.KMLProcessor.Import;

namespace SQLSpatialTools.UnitTests.Functions
{
    [TestClass]
    public class KmlFunctionTests
    {
        [TestMethod]
        public void KmlParserRejectsDocumentTypeDefinitions()
        {
            const string kml =
                "<!DOCTYPE kml [<!ENTITY external SYSTEM \"file:///nonexistent\">]><kml>&external;</kml>";

            Assert.ThrowsExactly<XmlException>(() => new KMLParser(kml));
        }

        [TestMethod]
        public void KmlParserAcceptsOrdinaryKml()
        {
            const string kml =
                "<kml><Placemark><Point><coordinates>1,2</coordinates></Point></Placemark></kml>";

            var parser = new KMLParser(kml);
            parser.Parse();

            Assert.AreEqual(1, parser.Geographies.Count);
        }
    }
}
