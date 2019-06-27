//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Functions.General
{
    /// <summary>
    /// This class contains General Geometry type functions that can be registered in SQL Server.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Selectively filter unwanted artifacts in input object:
        ///	- empty shapes (if [filterEmptyShapes] is true)
        ///	- points (if [filterPoints] is true)
        ///	- line strings shorter than provided tolerance (if lineString.STLength less than lineStringTolerance)
        ///	- polygon rings thinner than provided tolerance (if ring.STArea less than ring.STLength * ringTolerance)
        ///	- general behavior: Returned spatial objects will always to the simplest OGC construction
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="filterEmptyShapes"></param>
        /// <param name="filterPoints"></param>
        /// <param name="lineStringTolerance"></param>
        /// <param name="ringTolerance"></param>
        /// <returns></returns>
        public static SqlGeometry FilterArtifactsGeometry(SqlGeometry geometry, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
        {
            if (geometry == null || geometry.IsNull)
                return geometry;

            var geomBuilder = new SqlGeometryBuilder();
            IGeometrySink110 filter = geomBuilder;

            if (filterEmptyShapes)
                filter = new GeometryEmptyShapeFilter(filter);
            if (ringTolerance > 0)
                filter = new GeometryThinRingFilter(filter, ringTolerance);
            if (lineStringTolerance > 0)
                filter = new GeometryShortLineStringFilter(filter, lineStringTolerance);
            if (filterPoints)
                filter = new GeometryPointFilter(filter);

            geometry.Populate(filter);
            geometry = geomBuilder.ConstructedGeometry;

            if (geometry == null || geometry.IsNull || !geometry.STIsValid().Value)
                return geometry;

            // Strip collections with single element
            while (geometry.STNumGeometries().Value == 1 && geometry.InstanceOf("GEOMETRYCOLLECTION").Value)
                geometry = geometry.STGeometryN(1);

            return geometry;
        }

        /// <summary>
        /// Convert Z co-ordinate of geom from XYZ to XYM
        /// </summary>
        /// <param name="wktXYM">Well Know Text with x,y,z representation</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeometry GeomFromXYMText(string wktXYM, int srid)
        {
            var res = new ConvertXYZ2XYMGeometrySink();
            var geom = wktXYM.GetGeom(srid);
            geom.Populate(res);
            return res.ConstructedGeometry;
        }

        /// <summary>
        /// Find the point that is the given distance from the start point in the direction of the end point.
        /// The distance must be less than the distance between these two points.
        /// </summary>
        /// <param name="start">Starting Geometry Point</param>
        /// <param name="end">End Geometry Point</param>
        /// <param name="distance">Distance measure of the point to locate</param>
        /// <returns></returns>
        public static SqlGeometry InterpolateBetweenGeom(SqlGeometry start, SqlGeometry end, double distance)
        {
            // We need to check a few prerequisites.
            // We only operate on points.
            if (!start.IsPoint() || !end.IsPoint())
            {
                throw new ArgumentException(ErrorMessage.PointCompatible);
            }

            // The SRIDs also have to match
            var srid = start.STSrid.Value;
            if (srid != end.STSrid.Value)
            {
                throw new ArgumentException(ErrorMessage.SRIDCompatible);
            }

            // Finally, the distance has to fall between these points.
            var length = start.STDistance(end).Value;
            if (distance > start.STDistance(end))
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBeBetweenTwoPoints);
            }

            if (distance < 0)
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBePositive);
            }

            // Since we're working on a Cartesian plane, this is now pretty simple.
            // The fraction of the way from start to end.
            var fraction = distance / length;
            var newX = (start.STX.Value * (1 - fraction)) + (end.STX.Value * fraction);
            var newY = (start.STY.Value * (1 - fraction)) + (end.STY.Value * fraction);
            return SqlGeometry.Point(newX, newY, srid);
        }

        // Make our LocateAlongGeometrySink into a function call.  This function just hooks up
        // and runs a pipeline using the sink.
        public static SqlGeometry LocatePointAlongGeom(SqlGeometry geometry, double distance)
        {
            var geometryBuilder = new SqlGeometryBuilder();
            var geometrySink = new LocateAlongGeometrySink(distance, geometryBuilder);
            geometry.Populate(geometrySink);
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Make the input geometry valid to use in geography manipulation
        /// </summary>
        /// <param name="geometry">Input geometry</param>
        /// <returns></returns>
        public static SqlGeometry MakeValidForGeography(SqlGeometry geometry)
        {
            // Note: This function relies on an undocumented feature of the planar Union and MakeValid
            // that polygon rings in their result will always be oriented using the same rule that
            // is used in geography. But, it is not good practice to rely on such fact in production code.

            if (geometry.STIsValid().Value && !geometry.STIsEmpty().Value)
                return geometry.STUnion(geometry.STPointN(1));

            return geometry.MakeValid();
        }

        /// <summary>
        /// Reverse the Line Segment.
        /// </summary>
        /// <param name="geometry">Input SqlGeometry</param>
        /// <returns></returns>
        public static SqlGeometry ReverseLinestring(SqlGeometry geometry)
        {
            if (!geometry.IsLineString())
                throw new ArgumentException(ErrorMessage.LineStringCompatible);

            var geomBuilder = new SqlGeometryBuilder();

            geomBuilder.SetSrid((int)geometry.STSrid);
            geomBuilder.BeginGeometry(OpenGisGeometryType.LineString);
            geomBuilder.BeginFigure(geometry.STEndPoint().STX.Value, geometry.STEndPoint().STY.Value);
            for (var i = (int)geometry.STNumPoints() - 1; i >= 1; i--)
            {
                geomBuilder.AddLine(
                    geometry.STPointN(i).STX.Value,
                    geometry.STPointN(i).STY.Value);
            }
            geomBuilder.EndFigure();
            geomBuilder.EndGeometry();
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Shift the input Geometry x and y co-ordinate by specified amount
        /// Make our ShiftGeometrySink into a function call by hooking it into a simple pipeline.
        /// </summary>
        /// <param name="geometry">Input Geometry</param>
        /// <param name="xShift">X value to shift</param>
        /// <param name="yShift">Y value to shift</param>
        /// <returns>Shifted Geometry</returns>
        public static SqlGeometry ShiftGeometry(SqlGeometry geometry, double xShift, double yShift)
        {
            // create a sink that will create a geometry instance
            var geometryBuilder = new SqlGeometryBuilder();

            // create a sink to do the shift and plug it in to the builder
            var geomSink = new ShiftGeometrySink(xShift, yShift, geometryBuilder);

            // plug our sink into the geometry instance and run the pipeline
            geometry.Populate(geomSink);

            // the end of our pipeline is now populated with the shifted geometry instance
            return geometryBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// This implements a completely trivial conversion from geometry to geography, simply taking each
        /// point (x,y) --> (long, lat).  The result is assigned the given SRID.
        /// </summary>
        /// <param name="toConvert">Input Geometry to convert</param>
        /// <param name="targetSrid">Target SRID</param>
        /// <returns>Converted Geography</returns>
        public static SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)
        {
            var geographyBuilder = new SqlGeographyBuilder();
            toConvert.Populate(new VacuousGeometryToGeographySink(targetSrid, geographyBuilder));
            return geographyBuilder.ConstructedGeography;
        }
    }
}