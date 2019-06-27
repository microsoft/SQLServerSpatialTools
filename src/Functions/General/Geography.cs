//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Sinks.Geography;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Types.SQL;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Functions.General
{
    /// <summary>
    /// This class contains General Geography type functions that can be registered in SQL Server.
    /// </summary>
    public static class Geography
    {
        /// <summary>
        /// Make our LocateAlongGeographySink into a function call. 
        /// This function just hooks up and runs a pipeline using the sink.
        /// </summary>
        /// <param name="geography">Sql Geography</param>
        /// <param name="distance">Distance at which the point to be located</param>
        /// <returns></returns>
        public static SqlGeography LocatePointAlongGeog(SqlGeography geography, double distance)
        {
            var geogBuilder = new SqlGeographyBuilder();
            var geogSink = new LocateAlongGeographySink(distance, geogBuilder);
            geography.Populate(geogSink);
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Find the point that is the given distance from the start point in the direction of the end point.
        /// The distance must be less than the distance between these two points.
        /// </summary>
        /// <param name="start">Start Geography Point</param>
        /// <param name="end">End Geography Point</param>
        /// <param name="distance">Distance at which the point to be located</param>
        /// <returns></returns>
        public static SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)
        {
            // We need to check a few prerequisite.
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
            if (distance > length)
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBeBetweenTwoPoints);
            }

            if (distance < 0)
            {
                throw new ArgumentException(ErrorMessage.DistanceMustBePositive);
            }

            // We'll just do this by binary search---surely this could be more efficient, 
            // but this is relatively easy.
            //
            // Note that we can't just take the take the linear combination of end vectors because we
            // aren't working on a sphere.

            // We are going to do our binary search using 3D Cartesian values, however
            var startCart = SpatialUtil.GeographicToCartesian(start);
            var endCart = SpatialUtil.GeographicToCartesian(end);

            SqlGeography current;
            double currentDistance;

            // Keep refining until we slip below the THRESHOLD value.
            do
            {
                var currentCart = (startCart + endCart) / 2;
                current = SpatialUtil.CartesianToGeographic(currentCart, srid);
                currentDistance = start.STDistance(current).Value;

                if (distance <= currentDistance)
                    endCart = currentCart;
                else
                    startCart = currentCart;
            } while (Math.Abs(currentDistance - distance) > Constants.Tolerance);

            return current;
        }

        /// <summary>
        /// This function is used for generating a new geography object where additional points are inserted
        /// along every line in such a way that the angle between two consecutive points does not
        /// exceed a prescribed angle. The points are generated between the unit vectors that correspond
        /// to the line's start and end along the great-circle arc on the unit sphere. This follows the
        /// definition of geodetic lines in SQL Server.
        /// </summary>
        /// <param name="geography">Input Sql geography</param>
        /// <param name="maxAngle">Max Angle</param>
        /// <returns></returns>
        public static SqlGeography DensifyGeography(SqlGeography geography, double maxAngle)
        {
            var geogBuilder = new SqlGeographyBuilder();
            geography.Populate(new DensifyGeographySink(geogBuilder, maxAngle));
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Performs a complete trivial conversion from geography to geometry, simply taking each
        ///point (lat,long) -> (y, x).  The result is assigned the given SRID.
        /// </summary>
        /// <param name="toConvert"></param>
        /// <param name="targetSrid"></param>
        /// <returns></returns>
        public static SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)
        {
            var geomBuilder = new SqlGeometryBuilder();
            toConvert.Populate(new VacuousGeographyToGeometrySink(targetSrid, geomBuilder));
            return geomBuilder.ConstructedGeometry;
        }

        /// <summary>
        /// Computes ConvexHull of input geography and returns a polygon (unless all input points are collinear).
        /// </summary>
        /// <param name="geography">Input Sql Geography</param>
        /// <returns></returns>
        public static SqlGeography ConvexHullGeography(SqlGeography geography)
        {
            if (geography.IsNull || geography.STIsEmpty().Value) return geography;

            var center = geography.EnvelopeCenter();
            var gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);
            var geometry = gnomonicProjection.Project(geography);
            return gnomonicProjection.Unproject(geometry.MakeValid().STConvexHull());
        }

        /// <summary>
        /// Computes ConvexHull of input WKT and returns a polygon (unless all input points are collinear).
        /// This function does not require its input to be a valid geography. This function does require
        /// that the WKT coordinate values are longitude/latitude values, in that order and that a valid
        /// geography SRID value is supplied.
        /// </summary>
        /// <param name="inputWKT">Input Well Known Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)
        {
            var geometry = SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid);
            var geographyBuilder = new SqlGeographyBuilder();
            geometry.Populate(new GeometryToPointGeographySink(geographyBuilder));
            return ConvexHullGeography(geographyBuilder.ConstructedGeography);
        }

        /// <summary>
        /// Check if an input geometry can represent a valid geography without throwing an exception.
        /// This function requires that the geometry be in longitude/latitude coordinates and that
        /// those coordinates are in correct order in the geometry instance (i.e. latitude/longitude
        /// not longitude/latitude). This function will return false (0) if the input geometry is not
        /// in the correct latitude/longitude format, including a valid geography SRID.
        /// </summary>
        /// <param name="geometry">Input Sql Geometry</param>
        /// <returns></returns>
        public static bool IsValidGeographyFromGeometry(SqlGeometry geometry)
        {
            if (geometry.IsNull) return false;

            try
            {
                var geogBuilder = new SqlGeographyBuilder();
                geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, geogBuilder));
                // ReSharper disable once UnusedVariable
                var geography = geogBuilder.ConstructedGeography;
                return true;
            }
            catch (FormatException)
            {
                // Syntax error
                return false;
            }
            catch (ArgumentException)
            {
                // Semantic (Geometrical) error
                return false;
            }
        }

        /// <summary>
        /// Check if an input WKT can represent a valid geography. This function requires that
        /// the WTK coordinate values are longitude/latitude values, in that order and that a valid
        /// geography SRID value is supplied.  This function will not throw an exception even in
        /// edge conditions (i.e. longitude/latitude coordinates are reversed to latitude/longitude).
        /// </summary>
        /// <param name="inputWKT">Input Well Known Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static bool IsValidGeographyFromText(string inputWKT, int srid)
        {
            try
            {
                // If parse succeeds then our input is valid
                inputWKT.GetGeog(srid);
                return true;
            }
            catch (FormatException)
            {
                // Syntax error
                return false;
            }
            catch (ArgumentException)
            {
                // Semantic (Geometrical) error
                return false;
            }
        }

        /// <summary>
        /// Convert an input geometry instance to a valid geography instance.
        /// This function requires that the WKT coordinate values are longitude/latitude values,
        /// in that order and that a valid geography SRID value is supplied.
        /// </summary>
        /// <param name="geometry">Input Sql Geometry</param>
        /// <returns></returns>
        public static SqlGeography MakeValidGeographyFromGeometry(SqlGeometry geometry)
        {
            if (geometry.IsNull) return SqlGeography.Null;
            if (geometry.STIsEmpty().Value) return CreateEmptyGeography(geometry.STSrid.Value);

            // Extract vertices from our input to be able to compute geography EnvelopeCenter
            var pointSetBuilder = new SqlGeographyBuilder();
            geometry.Populate(new GeometryToPointGeographySink(pointSetBuilder));
            SqlGeography center;
            try
            {
                center = pointSetBuilder.ConstructedGeography.EnvelopeCenter();
            }
            catch (ArgumentException)
            {
                // Input is larger than a hemisphere.
                return SqlGeography.Null;
            }

            // Construct Gnomonic projection centered on input geography
            var gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);

            // Project, run geometry MakeValid and unproject
            var geometryBuilder = new SqlGeometryBuilder();
            geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, new Projector(gnomonicProjection, geometryBuilder)));
            var outGeometry = Geometry.MakeValidForGeography(geometryBuilder.ConstructedGeometry);

            try
            {
                return gnomonicProjection.Unproject(outGeometry);
            }
            catch (ArgumentException)
            {
                // Try iteratively to reduce the object to remove very close vertices.
                for (var tolerance = 1e-4; tolerance <= 1e6; tolerance *= 2)
                {
                    try
                    {
                        return gnomonicProjection.Unproject(outGeometry.Reduce(tolerance));
                    }
                    catch (ArgumentException)
                    {
                        // keep trying
                    }
                }
                return SqlGeography.Null;
            }
        }

        /// <summary>
        /// Constructs an empty Sql Geography
        /// </summary>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        private static SqlGeography CreateEmptyGeography(int srid)
        {
            var geogBuilder = new SqlGeographyBuilder();
            geogBuilder.SetSrid(srid);
            geogBuilder.BeginGeography(OpenGisGeographyType.GeometryCollection);
            geogBuilder.EndGeography();
            return geogBuilder.ConstructedGeography;
        }

        /// <summary>
        /// Convert an input WKT to a valid geography instance.
        /// This function requires that the WKT coordinate values are longitude/latitude values,
        /// in that order and that a valid geography SRID value is supplied.
        /// </summary>
        /// <param name="inputWKT">Input Well Know Text</param>
        /// <param name="srid">Spatial Reference Identifier</param>
        /// <returns></returns>
        public static SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)
        {
            return MakeValidGeographyFromGeometry(inputWKT.GetGeom(srid));
        }

        // Selectively filter unwanted artifacts in input object:
        //	- empty shapes (if [filterEmptyShapes] is true)
        //	- points (if [filterPoints] is true)
        //	- line strings shorter than provided tolerance (if lineString.STLength < [lineStringTolerance])
        //	- polygon rings thinner than provided tolerance (if ring.STArea < ring.STLength * [ringTolerance])
        //	- general behavior: Returned spatial objects will always to the simplest OGC construction
        //
        public static SqlGeography FilterArtifactsGeography(SqlGeography geography, bool filterEmptyShapes, bool filterPoints, double lineStringTolerance, double ringTolerance)
        {
            if (geography == null || geography.IsNull)
                return geography;

            var geogBuilder = new SqlGeographyBuilder();
            IGeographySink110 filter = geogBuilder;

            if (filterEmptyShapes)
                filter = new GeographyEmptyShapeFilter(filter);
            if (ringTolerance > 0)
                filter = new GeographyThinRingFilter(filter, ringTolerance);
            if (lineStringTolerance > 0)
                filter = new GeographyShortLineStringFilter(filter, lineStringTolerance);
            if (filterPoints)
                filter = new GeographyPointFilter(filter);

            geography.Populate(filter);
            geography = geogBuilder.ConstructedGeography;

            if (geography == null || geography.IsNull)
                return geography;

            // Strip collections with single element
            while (geography.STNumGeometries().Value == 1 && geography.InstanceOf("GEOMETRYCOLLECTION").Value)
                geography = geography.STGeometryN(1);

            return geography;
        }

    }
}
