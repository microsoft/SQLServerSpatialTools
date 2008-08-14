// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /**
     * This class contains functions that are meant to be used internally in this library.
     */
    class Util
    {
        // Convert a SqlGeography to an X,Y,Z vector.
        public static Vector3 GeographicToCartesian(SqlGeography point)
        {
            return SphericalToCartesian(point.Lat.Value, point.Long.Value);
        }

        // Convert an X,Y,Z vector to a SqlGeography
        public static SqlGeography CartesianToGeographic(Vector3 point, int srid)
        {
            return SqlGeography.Point(LatitudeDeg(point), LongitudeDeg(point), srid);
        }

        // Convert a Lat/Long in degrees to an X,Y,Z vector.
		public static Vector3 SphericalToCartesian(double latitudeDeg, double longitudeDeg)
		{
			double latitude = ToRadians(latitudeDeg);
			double longitude = ToRadians(longitudeDeg);
			double r = Math.Cos(latitude);
			return new Vector3(r * Math.Cos(longitude), r * Math.Sin(longitude), Math.Sin(latitude));
		}

		// Returns longitude in radians given the vector.
		public static double Longitude(Vector3 p)
		{ 
			return Math.Atan2(p.y, p.x); 
		}

		// Returns longitude in degrees given the vector.
		public static double LongitudeDeg(Vector3 p)
		{ 
			return ToDegrees(Longitude(p)); 
		}

		// Returns latitude in radians given the vector.
		public static double Latitude(Vector3 p)
		{
			return Math.Asin(p.z); 
		}

		// Returns latitude in degrees given the vector.
		public static double LatitudeDeg(Vector3 p)
		{
			return ToDegrees(Latitude(p));
		}

		// Converts degrees to radians.
		public static double ToRadians(double a)
		{
			return a / 180 * Math.PI;
		}

		// Converts radians to degrees.
		public static double ToDegrees(double a)
		{
			return a * 180 / Math.PI;
		}
    }
}