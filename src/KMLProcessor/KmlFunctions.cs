//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.KMLProcessor.Export;
using SQLSpatialTools.KMLProcessor.Import;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SQLSpatialTools.KMLProcessor
{
    /// <summary>
    /// This class contains the KML import/export functions
    /// </summary>
    public class KMLFunctions
    {
        #region Public Methods

        #region Import Methods

        /// <summary>
        /// This function parses the given KML string and returns the extracted geography instance.
        /// </summary>
        /// <param name="kml">KML string to be parsed</param>
        /// <param name="makeValid">If true and the extracted geography instance is invalid then the MakeValid
        /// function will be executed on the extracted geography instance</param>
        /// <returns>Extracted geography instance</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlGeography KmlToGeography(
            SqlString kml,
            SqlBoolean makeValid)
        {
            if (kml == null || string.IsNullOrEmpty(kml.Value))
            {
                return SqlGeography.Null;
            }

            var constructed = new SqlGeographyBuilder();
            var parser = new Import.KMLProcessor(kml.Value.Trim());
            parser.Populate(constructed, makeValid.Value);
            return constructed.ConstructedGeography;
        }

        #endregion

        #region TVF Import Functions

        /// <summary>
        /// This TVF function parses the given KML string. All geography instances will be extracted and 
        /// returned along with the context where they were found. 
        /// One record will be returned for each extracted geography instance.
        /// 
        /// The return table format is: (Id NVARCHAR(200), Context NVARCHAR(MAX), Shape Geography)
        /// </summary>
        /// <param name="kml">KML string which should be processed</param>
        /// <param name="makeValid">If true and extracted geography instance is invalid then the MakeValid
        /// function will be executed on that geography instance</param>
        [Microsoft.SqlServer.Server.SqlFunction(
            FillRowMethodName = "DecodeGeographyContext",
            TableDefinition = "Id NVARCHAR(200), Context NVARCHAR(MAX), Shape Geography"
        )]
        public static IEnumerable ImportKml(
            SqlString kml,
            SqlBoolean makeValid)
        {
            if (kml == null || string.IsNullOrEmpty(kml.Value))
                return new Collection<GeographyContext>();

            var geographies = new Collection<GeographyContext>();

            var parser = 
                new Import.KMLProcessor(kml.Value.Trim());

            foreach (var p in parser.Geographies)
            {
                var ge = new GeographyContext
                {
                    Id = p.Id, Context = p.Context, Shape = p.ToSqlGeography(makeValid.Value)
                };

                geographies.Add(ge);
            }

            return geographies;
        }

        /// <summary>
        /// This method is a helper method for the TVF function ImportKml. A Sql Server uses it to decode a row
        /// returned by the ImportKml function.
        /// </summary>
        /// <param name="rowData">A row returned by the ImportKml function</param>
        /// <param name="id">Id extracted from a rowData</param>
        /// <param name="context">Context extracted from a rowData</param>
        /// <param name="shape">Shape extracted from a rowData</param>
        public static void DecodeGeographyContext(
            object rowData,
            out SqlString id,
            out SqlString context,
            out SqlGeography shape)
        {
            id = new SqlString();
            context = new SqlString();
            shape = null;

            if (!(rowData is GeographyContext data)) return;

            id = data.Id;
            context = data.Context;
            shape = data.Shape;

        }

        #endregion

        #region Export Methods

        /// <summary>
        /// This method converts the given geography instance into the KML format
        /// </summary>
        /// <param name="g">Geography instance to be converted</param>
        /// <returns>KML representation of the given geography instance</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString ExportToKml(SqlGeography g)
        {
            if (g == null || g.IsNull)
                return new SqlString("");

            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream);

            var sink = new KeyholeMarkupLanguageGeography(writer);

            g.Populate(sink);

            sink.FinalizeKMLDocument();

            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var ba = stream.ToArray();
            var sData = System.Text.Encoding.UTF8.GetChars(ba);
            var str = new string(sData);
            writer.Close();

            // Removes everything before <?xml>
            var xmlIndex = str.IndexOf("<?xml>", StringComparison.Ordinal);
            if (xmlIndex > 0)
                str = str.Remove(0, xmlIndex);

            return new SqlString(str);
        }

        /// <summary>
        /// This method converts the given geography instance represented as a WKT string, into the string in the KML format.
        /// </summary>
        /// <param name="wkt">Geography instance represented as the WKT string</param>
        /// <returns>The KML string</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString ExportWKTToKml(SqlString wkt)
        {
            if (wkt == null || string.IsNullOrEmpty(wkt.Value))
                return new SqlString("");

            var g = SqlGeography.Parse(wkt);
            return ExportToKml(g);
        }

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// This method stores the given box into the database
        /// </summary>
        /// <param name="box">Box to be saved</param>
        /// <param name="insertedKmlId">The KmlId to connect this object to</param>
        /// <param name="con">Sql connection</param>
        /// <param name="tran">Sql transaction</param>
        /// <returns>The database id which is assigned to the given box</returns>
        protected static int? SaveLatLonBox(Geography box, int insertedKmlId, SqlConnection con, SqlTransaction tran)
        {
            #region Insert Geography

            int? insertedGeographyId = null;

            if (box != null)
            {
                var saveGeographyCmd =
                    new SqlCommand(
                        " insert into Geographies(Geography, KmlId) " +
                        " values(geography::Parse('" + box + "'), @KmlId);select scope_identity()", con, tran);

                AddParameter(saveGeographyCmd, "KmlId", insertedKmlId, DbType.Int32);

                insertedGeographyId = (int)((decimal)saveGeographyCmd.ExecuteScalar());
                box.DbId = insertedGeographyId;
            }

            #endregion

            #region Insert Lat Lon Box

            var saveLatLonBox =
                new SqlCommand(
                    " insert into LatLonBoxes(BoxType, GeographyId, rotation, MinAlt, MaxAlt, AltitudeMode) " +
                    " values(@BoxType, @GeographyId, @rotation, @MinAlt, @MaxAlt, @AltitudeMode);select scope_identity()",
                    con, tran);

            AddParameter(saveLatLonBox, "GeographyId", insertedGeographyId, DbType.Int32);

            var boxType = "";

            if (box is LatLonBox latLonBox)
            {
                boxType = "LatLonBox";
                AddParameter(saveLatLonBox, "rotation", latLonBox.Rotation, DbType.Double);
            }
            else
            {
                AddParameter(saveLatLonBox, "rotation", null, DbType.Double);
            }

            if (box is LatLonAltBox latLonAltBox)
            {
                boxType = "LatLonAltBox";

                AddParameter(saveLatLonBox, "MinAlt", latLonAltBox.MinAltitude, DbType.Double);
                AddParameter(saveLatLonBox, "MaxAlt", latLonAltBox.MaxAltitude, DbType.Double);
                AddParameter(saveLatLonBox, "AltitudeMode", latLonAltBox.AltitudeMode.ToString(), DbType.String);
            }
            else
            {
                AddParameter(saveLatLonBox, "MinAlt", null, DbType.Double);
                AddParameter(saveLatLonBox, "MaxAlt", null, DbType.Double);
                AddParameter(saveLatLonBox, "AltitudeMode", null, DbType.Double);
            }

            // ReSharper disable once UnusedVariable
            if (box is LatLonQuad latLonQuad)
            {
                boxType = "LatLonQuad";
            }

            AddParameter(saveLatLonBox, "BoxType", boxType, DbType.String);

            int? insertedLatLonBox = (int)((decimal)saveLatLonBox.ExecuteScalar());

            #endregion

            return insertedLatLonBox;
        }

        /// <summary>
        /// This method adds a new parameter to the given sql command
        /// </summary>
        /// <param name="cmd">Command to add parameter to</param>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="type">Parameter database type</param>
        protected static void AddParameter(SqlCommand cmd, string parameterName, object value, DbType type)
        {
            if (value == null)
                value = DBNull.Value;

            cmd.Parameters.Add(new SqlParameter()
            {
                ParameterName = parameterName,
                Value = value,
                DbType = type
            });
        }

        #endregion
    }
}


