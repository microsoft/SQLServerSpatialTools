// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

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

	}
}