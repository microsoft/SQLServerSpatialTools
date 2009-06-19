// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace SQLSpatialTools
{
	// This class contains functions that can be registered in SQL Server.
	public class Functions
	{
		const double THRESHOLD = .01;  // 1 cm tolerance in most SRIDs

		// Make our ShiftGeometrySink into a function call by hooking it into a simple pipeline.
		public static SqlGeometry ShiftGeometry(SqlGeometry g, double xShift, double yShift)
		{
			// create a sink that will create a geometry instance
			SqlGeometryBuilder b = new SqlGeometryBuilder();

			// create a sink to do the shift and plug it in to the builder
			ShiftGeometrySink s = new ShiftGeometrySink(xShift, yShift, b);

			// plug our sink into the geometry instance and run the pipeline
			g.Populate(s);

			// the end of our pipeline is now populated with the shifted geometry instance
			return b.ConstructedGeometry;
		}

		// Make our LocateAlongGeographySink into a function call.  This function just hooks up
		// and runs a pipeline using the sink.
		public static SqlGeography LocateAlongGeog(SqlGeography g, double distance)
		{
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			LocateAlongGeographySink p = new LocateAlongGeographySink(distance, b);
			g.Populate(p);
			return b.ConstructedGeography;
		}

		// Make our LocateAlongGeometrySink into a function call.  This function just hooks up
		// and runs a pipeline using the sink.
		public static SqlGeometry LocateAlongGeom(SqlGeometry g, double distance)
		{
			SqlGeometryBuilder b = new SqlGeometryBuilder();
			LocateAlongGeometrySink p = new LocateAlongGeometrySink(distance, b);
			g.Populate(p);
			return b.ConstructedGeometry;
		}

		// Find the point that is the given distance from the start point in the direction of the end point.
		// The distance must be less than the distance between these two points.
		public static SqlGeography InterpolateBetweenGeog(SqlGeography start, SqlGeography end, double distance)
		{
			// We need to check a few prequisites.

			// We only operate on points.

			if (start.STGeometryType().Value != "Point")
			{
				throw new ArgumentException("Start value must be a point.");
			}

			if (end.STGeometryType().Value != "Point")
			{
				throw new ArgumentException("Start value must be a point.");
			}

			// The SRIDs also have to match
			int srid = start.STSrid.Value;
			if (srid != end.STSrid.Value)
			{
				throw new ArgumentException("The start and end SRIDs must match.");
			}

			// Finally, the distance has to fall between these points.
			if (distance > start.STDistance(end))
			{
				throw new ArgumentException("The distance value provided exceeds the distance between the two points.");
			}
			else if (distance < 0)
			{
				throw new ArgumentException("The distance must be positive.");
			}

			// We'll just do this by binary search---surely this could be more efficient, but this is 
			// relatively easy.
			//
			// Note that we can't just take the take the linear combination of end vectors because we
			// aren't working on a sphere.

			// We are going to do our binary search using 3D Cartesian values, however
			Vector3 startCart = Util.GeographicToCartesian(start);
			Vector3 endCart = Util.GeographicToCartesian(end);
			Vector3 currentCart;

			SqlGeography current;
			double currentDistance;

			// Keep refining until we slip below the THRESHOLD value.
			do
			{
				currentCart = (startCart + endCart) / 2;
				current = Util.CartesianToGeographic(currentCart, srid);
				currentDistance = start.STDistance(current).Value;
				if (distance <= currentDistance) endCart = currentCart;
				else startCart = currentCart;
			} while (Math.Abs(currentDistance - distance) > THRESHOLD);

			return current;
		}

		// Find the point that is the given distance from the start point in the direction of the end point.
		// The distance must be less than the distance between these two points.
		public static SqlGeometry InterpolateBetweenGeom(SqlGeometry start, SqlGeometry end, double distance)
		{
			// We need to check a few prequisites.

			// We only operate on points.

			if (start.STGeometryType().Value != "Point")
			{
				throw new ArgumentException("Start value must be a point.");
			}

			if (end.STGeometryType().Value != "Point")
			{
				throw new ArgumentException("Start value must be a point.");
			}

			// The SRIDs also have to match
			int srid = start.STSrid.Value;
			if (srid != end.STSrid.Value)
			{
				throw new ArgumentException("The start and end SRIDs must match.");
			}

			// Finally, the distance has to fall between these points.
			double length = start.STDistance(end).Value;
			if (distance > start.STDistance(end))
			{
				throw new ArgumentException("The distance value provided exceeds the distance between the two points.");
			}
			else if (distance < 0)
			{
				throw new ArgumentException("The distance must be positive.");
			}

			// Since we're working on a Cartesian plane, this is now pretty simple.
			double f = distance/length;  // The fraction of the way from start to end.
			double newX = (start.STX.Value * (1-f)) + (end.STX.Value * f);
			double newY = (start.STY.Value * (1-f)) + (end.STY.Value * f);
			return SqlGeometry.Point(newX, newY, srid);
		}
		
		// This function is used for generating a new geography object where additional points are inserted
		// along every line in such a way that the angle between two consecutive points does not
		// exceed a prescribed angle. The points are generated between the unit vectors that correspond
		// to the line's start and end along the great-circle arc on the unit sphere. This follows the
		// definition of geodetic lines in SQL Server.
		public static SqlGeography DensifyGeography(SqlGeography g, double maxAngle)
		{
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			g.Populate(new DensifyGeographySink(b, maxAngle));
			return b.ConstructedGeography;
		}

		// This implements a completely trivial conversion from geometry to geography, simply taking each
		// point (x,y) --> (long, lat).  The result is assigned the given SRID.
		public static SqlGeography VacuousGeometryToGeography(SqlGeometry toConvert, int targetSrid)
		{
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			toConvert.Populate(new VacuousGeometryToGeographySink(targetSrid, b));
			return b.ConstructedGeography;
		}

		// This implements a completely trivial conversion from geography to geometry, simply taking each
		// point (lat,long) --> (y, x).  The result is assigned the given SRID.
		public static SqlGeometry VacuousGeographyToGeometry(SqlGeography toConvert, int targetSrid)
		{
			SqlGeometryBuilder b = new SqlGeometryBuilder();
			toConvert.Populate(new VacuousGeographyToGeometrySink(targetSrid, b));
			return b.ConstructedGeometry;
		}

		// Computes ConvexHull of input geography and returns a polygon (unless all input points are colinear).
		//
		public static SqlGeography ConvexHullGeography(SqlGeography geography)
		{
			if (geography.IsNull || geography.STIsEmpty().Value) return geography;

			SqlGeography center = geography.EnvelopeCenter();
			SqlProjection gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);
			SqlGeometry geometry = gnomonicProjection.Project(geography);
			return gnomonicProjection.Unproject(geometry.MakeValid().STConvexHull());
		}

		// Computes ConvexHull of input WKT and returns a polygon (unless all input points are colinear).
		// This function does not require its input to be a valid geography. This function does require
		// that the WKT coordinate values are longitude/latitude values, in that order and that a valid
		// geography SRID value is supplied.
		//
		public static SqlGeography ConvexHullGeographyFromText(string inputWKT, int srid)
		{
			SqlGeometry geometry = SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid);
			SqlGeographyBuilder geographyBuilder = new SqlGeographyBuilder();
			geometry.Populate(new GeometryToPointGeographySink(geographyBuilder));
			return ConvexHullGeography(geographyBuilder.ConstructedGeography);
		}

		// Check if an input geometry can represent a valid geography without throwing an exception.
		// This function requires that the geometry be in longitude/latitude coordinates and that
		// those coordinates are in correct order in the geometry instance (i.e. latitude/longitude
		// not longitude/latitude). This function will return false (0) if the input geometry is not
		// in the correct latitude/longitude format, including a valid geography SRID.
		//
		public static bool IsValidGeographyFromGeometry(SqlGeometry geometry)
		{
			if (geometry.IsNull) return false;

			try
			{
				SqlGeographyBuilder builder = new SqlGeographyBuilder();
				geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, builder));
				SqlGeography geography = builder.ConstructedGeography;
				return true;
			}
			catch (FormatException)
			{
				// Syntax error
				return false;
			}
			catch (ArgumentException)
			{
				// Semantical (Geometrical) error
				return false;
			}
		}

		// Check if an input WKT can represent a valid geography. This function requires that
		// the WTK coordinate values are longitude/latitude values, in that order and that a valid
		// geography SRID value is supplied.  This function will not throw an exception even in
		// edge conditions (i.e. longitude/latitude coordinates are reversed to latitude/longitude).
		//
		public static bool IsValidGeographyFromText(string inputWKT, int srid)
		{
			try
			{
				// If parse succeeds then our input is valid
				SqlGeography.STGeomFromText(new SqlChars(inputWKT), srid);
				return true;
			}
			catch (FormatException)
			{
				// Syntax error
				return false;
			}
			catch (ArgumentException)
			{
				// Semantical (Geometrical) error
				return false;
			}
		}

		// Convert an input geometry instance to a valid geography instance.
		// This function requires that the WKT coordinate values are longitude/latitude values,
		// in that order and that a valid geography SRID value is supplied.
		//
		public static SqlGeography MakeValidGeographyFromGeometry(SqlGeometry geometry)
		{
			if (geometry.IsNull) return SqlGeography.Null;
			if (geometry.STIsEmpty().Value) return CreateEmptyGeography(geometry.STSrid.Value);

			// Extract vertices from our input to be able to compute geography EnvelopeCenter
			SqlGeographyBuilder pointSetBuilder = new SqlGeographyBuilder();
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
			SqlProjection gnomonicProjection = SqlProjection.Gnomonic(center.Long.Value, center.Lat.Value);

			// Project, run geometry MakeValid and unproject
			SqlGeometryBuilder geometryBuilder = new SqlGeometryBuilder();
			geometry.Populate(new VacuousGeometryToGeographySink(geometry.STSrid.Value, new Projector(gnomonicProjection, geometryBuilder)));
			SqlGeometry outGeometry = MakeValidForGeography(geometryBuilder.ConstructedGeometry);

			try
			{
				return gnomonicProjection.Unproject(outGeometry);
			}
			catch (ArgumentException)
			{
				// Try iteratively to reduce the object to remove very close vertices.
				for (double tollerance = 1e-4; tollerance <= 1e6; tollerance *= 2)
				{
					try
					{
						return gnomonicProjection.Unproject(outGeometry.Reduce(tollerance));
					}
					catch (ArgumentException)
					{
						// keep trying
					}
				}
				return SqlGeography.Null;
			}
		}

		private static SqlGeography CreateEmptyGeography(int srid)
		{
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			b.SetSrid(srid);
			b.BeginGeography(OpenGisGeographyType.GeometryCollection);
			b.EndGeography();
			return b.ConstructedGeography;
		}

		private static SqlGeometry MakeValidForGeography(SqlGeometry geometry)
		{
			// Note: This function relies on an undocumented feature of the planar Union and MakeValid
			// that polygon rings in their result will always be oriented using the same rule that
			// is used in geography. But, it is not good practice to rely on such fact in production code.

			if (geometry.STIsValid().Value && !geometry.STIsEmpty().Value)
				return geometry.STUnion(geometry.STPointN(1));
	
			return geometry.MakeValid();
		}

		// Convert an input WKT to a valid geography instance.
		// This function requires that the WKT coordinate values are longitude/latitude values,
		// in that order and that a valid geography SRID value is supplied.
		//
		public static SqlGeography MakeValidGeographyFromText(string inputWKT, int srid)
		{
			return MakeValidGeographyFromGeometry(SqlGeometry.STGeomFromText(new SqlChars(inputWKT), srid));
		}
	}
}