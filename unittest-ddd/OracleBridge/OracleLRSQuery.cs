//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.UnitTests.DDD
{
    internal static class OracleLRSQuery
    {
        #region Oracle Queries

        public const string MergeGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                    + "SDO_LRS.CONCATENATE_GEOM_SEGMENTS("
                                                    + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),"
                                                    + "SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2})) from dual";

        public const string MergeAndResetGeomSegmentQuery = "DECLARE "
                                                            + "result_geom  SDO_GEOMETRY;"
                                                            + ""
                                                            + "BEGIN "
                                                            + "Select SDO_LRS.CONCATENATE_GEOM_SEGMENTS (SDO_UTIL.FROM_WKTGEOMETRY('{0}'), SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2})  into result_geom from dual;"
                                                            + ""
                                                            + "SDO_LRS.REDEFINE_GEOM_SEGMENT(result_geom);"
                                                            + " "
                                                            + "	INSERT INTO TEMP_DATA (Output1)"
                                                            + "SELECT SDO_UTIL.TO_WKTGEOMETRY(result_geom)"
                                                            + " FROM dual;"
                                                            + " END;";

        public const string ClipGeomSegmentQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                   + "SDO_LRS.CLIP_GEOM_SEGMENT("
                                                   + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'), {1}, {2}, {3})"
                                                   + ") from dual";

        public const string ClipGeomSegmentPointQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                        + "SDO_LRS.CLIP_GEOM_SEGMENT("
                                                        + " SDO_GEOMETRY(3001, NULL, NULL, "
                                                        + "SDO_ELEM_INFO_ARRAY(1, 1, 1), "
                                                        + "SDO_ORDINATE_ARRAY({0}))"
                                                        + ", {1}, {2}, {3})"
                                                        + ") from dual";

        public const string GetEndMeasureQuery = "SELECT "
                                                 + "SDO_LRS.GEOM_SEGMENT_END_MEASURE("
                                                 + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                 + ") from dual";

        public const string GetStartMeasureQuery = "SELECT "
                                                   + "SDO_LRS.GEOM_SEGMENT_START_MEASURE("
                                                   + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                   + ") from dual";

        public const string GetIsConnectedGeomSegmentQuery = "SELECT "
                                                             + "SDO_LRS.CONNECTED_GEOM_SEGMENTS("
                                                             + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),"
                                                             + "SDO_UTIL.FROM_WKTGEOMETRY('{1}'), {2}) from dual";

        public const string GetLocatePointAlongGeomQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                           + "SDO_LRS.LOCATE_PT("
                                                           + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'),{1})"
                                                           + ")from dual";

        public const string GetReverseLinearGeomQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                        + "SDO_LRS.REVERSE_GEOMETRY("
                                                        + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                        + "))from dual";

        public const string GetPopulateMeasureNonQuery = "DECLARE geom_segment SDO_GEOMETRY;"
                                                         + ""
                                                         + "BEGIN"
                                                         + "	SELECT SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                         + "	INTO geom_segment"
                                                         + "	FROM dual;"
                                                         + ""
                                                         + "	SDO_LRS.REDEFINE_GEOM_SEGMENT(geom_segment{1});"
                                                         + ""
                                                         + "	INSERT INTO TEMP_DATA (Output1)"
                                                         + "	SELECT SDO_UTIL.TO_WKTGEOMETRY(geom_segment)"
                                                         + "	FROM dual;"
                                                         + "END;";

        public const string GetPopulateMeasurePoint = "DECLARE geom_segment SDO_GEOMETRY; "
                                                      + "BEGIN "
                                                      + "SELECT SDO_GEOMETRY(3001, NULL, NULL, "
                                                      + "SDO_ELEM_INFO_ARRAY(1, 1, 1), "
                                                      + "SDO_ORDINATE_ARRAY({0})) "
                                                      + "INTO geom_segment FROM DUAL; "
                                                      + "SDO_LRS.REDEFINE_GEOM_SEGMENT(geom_segment{1}); "
                                                      + "INSERT INTO TEMP_DATA (Output1) SELECT SDO_UTIL.TO_WKTGEOMETRY(geom_segment) FROM dual; "
                                                      + "END; ";

        public const string GetConvertToLrsGeom     = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                      + "SDO_LRS.CONVERT_TO_LRS_GEOM("
                                                      + "SDO_UTIL.FROM_WKTGEOMETRY('{0}') {1})"
                                                      + ")from dual";

        public const string GetConvertToLrsGeomPoint = "SELECT SDO_UTIL.TO_WKTGEOMETRY( "
                                                      + "SDO_LRS.CONVERT_TO_LRS_GEOM("
                                                      + "SDO_GEOMETRY(2001, NULL, NULL, SDO_ELEM_INFO_ARRAY(1, 1, 1), SDO_ORDINATE_ARRAY({0})) {1}) "
                                                      + ")FROM dual";
              

        public const string GetSplitGeometrySegmentQuery = "DECLARE "
                                                           + "geom_segment SDO_GEOMETRY;"
                                                           + "result_geom_1 SDO_GEOMETRY;"
                                                           + "result_geom_2 SDO_GEOMETRY;"
                                                           + ""
                                                           + "BEGIN "
                                                           + "SELECT SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                           + " INTO geom_segment"
                                                           + " FROM dual;"
                                                           + " "
                                                           + "SDO_LRS.SPLIT_GEOM_SEGMENT(geom_segment,{1},result_geom_1,result_geom_2);"
                                                           + " "
                                                           + "	INSERT INTO TEMP_DATA (Output1,Output2)"
                                                           + "SELECT SDO_UTIL.TO_WKTGEOMETRY(result_geom_1), SDO_UTIL.TO_WKTGEOMETRY(result_geom_2)"
                                                           + " FROM dual;"
                                                           + "END;";

        public const string DropTempTableQuery = "DECLARE"
                                                 + "   C INT;"
                                                 + "BEGIN"
                                                 + "   SELECT COUNT(*) INTO C FROM USER_TABLES WHERE TABLE_NAME = UPPER('TEMP_DATA');"
                                                 + "   IF C = 1 THEN"
                                                 + "      EXECUTE IMMEDIATE 'DROP TABLE TEMP_DATA';"
                                                 + "   END IF;"
                                                 + "END;";


        public const string CreateTempTableQuery = "CREATE TABLE TEMP_DATA ("
                                                   + " Output_ID NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY"
                                                   + " ,Output1 VARCHAR2(1000)"
                                                   + " ,Output2 VARCHAR2(1000)"
                                                   + " )";

        public const string CreateTempTableIndexQuery = "CREATE UNIQUE INDEX UN_PK ON Temp_Data (Output_ID)";

        public const string CreateTempTablePkQuery = "ALTER TABLE Temp_Data ADD (CONSTRAINT UN_PK PRIMARY KEY (Output_ID) USING INDEX UN_PK ENABLE VALIDATE)";

        public const string GetOneResultFromTempTable = "SELECT Output1"
                                                        + " FROM TEMP_DATA"
                                                        + " WHERE OUTPUT_ID IN ("
                                                        + " SELECT MAX(OUTPUT_ID)"
                                                        + " FROM TEMP_DATA"
                                                        + " )";

        public const string GetTwoResultFromTempTable = "SELECT Output1, Output2"
                                                        + " FROM TEMP_DATA"
                                                        + " WHERE OUTPUT_ID IN ("
                                                        + " SELECT MAX(OUTPUT_ID)"
                                                        + " FROM TEMP_DATA"
                                                        + " )";

        public const string ValidateLRSGeometryQuery = "SELECT "
                                                       + "SDO_LRS.VALIDATE_LRS_GEOMETRY ("
                                                       + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                       + ") from dual";

        public const string OffsetGeometryQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                  + "SDO_LRS.OFFSET_GEOM_SEGMENT("
                                                  + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'), {1}, {2}, {3}, {4})"
                                                  + ") from dual";

        public const string GetPolygonToLineQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                + "SDO_UTIL.POLYGONTOLINE("
                                                + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                + ")) from dual";

        public const string GetRemoveDuplicateVerticesQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                + "SDO_UTIL.REMOVE_DUPLICATE_VERTICES("
                                                + "SDO_UTIL.FROM_WKTGEOMETRY('{0}')"
                                                + ", {1})) from dual";

        public const string GetExtractQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                               + "SDO_UTIL.EXTRACT("
                                               + "SDO_UTIL.FROM_WKTGEOMETRY('{0}'), {1}, {2})"
                                               + ") from dual";

        public const string GetExtractPoint3DQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                   + "SDO_UTIL.EXTRACT("                                                   
                                                   + "SDO_GEOMETRY(3001, NULL, NULL, "
                                                   + "SDO_ELEM_INFO_ARRAY(1, 1, 1), "
                                                   + "SDO_ORDINATE_ARRAY({0})"
                                                   + "), {1}, {2})"
                                                   + ") from dual";

        public const string GetExtractPoint2DQuery = "SELECT SDO_UTIL.TO_WKTGEOMETRY("
                                                   + "SDO_UTIL.EXTRACT("
                                                   + "SDO_GEOMETRY(2001, NULL, NULL, "
                                                   + "SDO_ELEM_INFO_ARRAY(1, 1, 1), "
                                                   + "SDO_ORDINATE_ARRAY({0})"
                                                   + "), {1}, {2})"
                                                   + ") from dual";

        #endregion Oracle Queries

        public const string DimensionParse = @"([-\dNULL\.\s]+[\,\)])";
        public const string DimensionGroup = @"(?<content>[-\dNULL\.\s]+)(?<suffix>[\,\)])";
        public const string DimensionMatch = @"((?<x>[\d\.\-]+)\s+(?<y>[\d\.\-]+)\s+(?<z>([\d\.\-]+)|(null)|(NULL))\s+(?<m>([\d\.]+)|(null)|(NULL)))"
                                             + @"|((?<x>[\d\.\-]+)\s+(?<y>[\d\.\-]+)\s+(?<z>([\d\.\-]+)|(null)|(NULL)))"
                                             + @"|((?<x>[\d\.\-]+)\s+(?<y>[\d\.\-]+\s?))";
    }
}