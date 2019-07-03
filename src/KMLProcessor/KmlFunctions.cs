using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using Microsoft.SqlServer.SpatialToolbox.KMLProcessor;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.ObjectModel;

/// <summary>
/// This class contains the KML import/export functions
/// </summary>
public partial class KMLFunctions
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

		SqlGeographyBuilder constructed = new SqlGeographyBuilder();
		Microsoft.SqlServer.SpatialToolbox.KMLProcessor.KMLProcessor parser = new Microsoft.SqlServer.SpatialToolbox.KMLProcessor.KMLProcessor(kml.Value.Trim());
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

		Collection<GeographyContext> geographies = new Collection<GeographyContext>();

		Microsoft.SqlServer.SpatialToolbox.KMLProcessor.KMLProcessor parser = 
					new Microsoft.SqlServer.SpatialToolbox.KMLProcessor.KMLProcessor(kml.Value.Trim());

		foreach (Geography p in parser.Geographies)
		{
			GeographyContext ge = new GeographyContext();
			ge.Id = p.Id;
			ge.Context = p.Context;
			ge.Shape = p.ToSqlGeography(makeValid.Value);

			geographies.Add(ge);
		}

		return geographies;
	}

	/// <summary>
	/// This method is a helper method for the TVF function ImportKml. A Sql Server uses it to decode a row
	/// returned by the ImportKml function.
	/// </summary>
	/// <param name="rowData">A row returned by the ImportKml function</param>
	/// <param name="Id">Id extracted from a rowData</param>
	/// <param name="Context">Context extracted from a rowData</param>
	/// <param name="Shape">Shape extracted from a rowData</param>
	public static void DecodeGeographyContext(
		object rowData,
		out SqlString Id,
		out SqlString Context,
		out SqlGeography Shape)
	{
		GeographyContext data = rowData as GeographyContext;

		Id = data.Id;
		Context = data.Context;
		Shape = data.Shape;
	}

	#endregion

	#region Export Methods

	/// <summary>
	/// This method converts the given geography instance into the KML format
	/// </summary>
	/// <param name="g">Geography instance to be converted</param>
	/// <returns>KML representation of the given geograpky instance</returns>
	[Microsoft.SqlServer.Server.SqlFunction]
	public static SqlString ExportToKml(SqlGeography g)
	{
		if (g == null || g.IsNull)
			return new SqlString("");

		MemoryStream stream = new MemoryStream();
		XmlWriter writer = XmlWriter.Create(stream);

		KeyholeMarkupLanguageGeography sink = new KeyholeMarkupLanguageGeography(writer);

		g.Populate(sink);

		sink.FinalizeKMLDocument();

		writer.Flush();
		stream.Seek(0, SeekOrigin.Begin);
		byte[] ba = stream.ToArray();
		char[] sData = System.Text.ASCIIEncoding.UTF8.GetChars(ba);
		string s = new string(sData);
		writer.Close();

		// Removes everything before <?xml>
		if (s.IndexOf("<?xml>") > 0)
			s = s.Remove(0, s.IndexOf("<?xml>"));

		return new SqlString(s);
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

		SqlGeography g = SqlGeography.Parse(wkt);
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
			SqlCommand saveGeographyCmd =
				new SqlCommand(
						" insert into Geographies(Geography, KmlId) " +
						" values(geography::Parse('" + box.ToString() + "'), @KmlId);select scope_identity()", con, tran);

			AddParameter(saveGeographyCmd, "KmlId", insertedKmlId, DbType.Int32);

			insertedGeographyId = (int)((decimal)saveGeographyCmd.ExecuteScalar());
			box.DbId = insertedGeographyId;
		}

		#endregion

		#region Insert Lat Lon Box

		SqlCommand saveLatLonBox =
			new SqlCommand(
					" insert into LatLonBoxes(BoxType, GeographyId, rotation, MinAlt, MaxAlt, AltitudeMode) " +
					" values(@BoxType, @GeographyId, @rotation, @MinAlt, @MaxAlt, @AltitudeMode);select scope_identity()",
					con, tran);

		AddParameter(saveLatLonBox, "GeographyId", insertedGeographyId, DbType.Int32);

		LatLonBox latLonBox = box as LatLonBox;
		LatLonAltBox latLonAltBox = box as LatLonAltBox;
		LatLonQuad latLonQuad = box as LatLonQuad;

		string BoxType = "";

		if (latLonBox != null)
		{
			BoxType = "LatLonBox";
			AddParameter(saveLatLonBox, "rotation", latLonBox.Rotation, DbType.Double);
		}
		else
		{
			AddParameter(saveLatLonBox, "rotation", null, DbType.Double);
		}

		if (latLonAltBox != null)
		{
			BoxType = "LatLonAltBox";

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

		if (latLonQuad != null)
		{
			BoxType = "LatLonQuad";
		}

		AddParameter(saveLatLonBox, "BoxType", BoxType, DbType.String);

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


