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
            return SphericalDegToCartesian(point.Lat.Value, point.Long.Value);
        }

        // Convert an X,Y,Z vector to a SqlGeography
        public static SqlGeography CartesianToGeographic(Vector3 point, int srid)
        {
            return SqlGeography.Point(LatitudeDeg(point), LongitudeDeg(point), srid);
        }

        // Convert a Lat/Long in degrees to an X,Y,Z vector.
		public static Vector3 SphericalDegToCartesian(double latitudeDeg, double longitudeDeg)
		{
			if (Math.Abs(latitudeDeg) > 90)
				throw new ArgumentOutOfRangeException("|latitudeDeg| > 90");

			double latitudeRad = ToRadians(latitudeDeg);
			double longitudeRad = ToRadians(longitudeDeg);
			double r = Math.Cos(latitudeRad);
			return new Vector3(r * Math.Cos(longitudeRad), r * Math.Sin(longitudeRad), Math.Sin(latitudeRad));
		}

		// Convert a Lat/Long in radians to an X,Y,Z vector.
		public static Vector3 SphericalRadToCartesian(double latitudeRad, double longitudeRad)
		{
			if (Math.Abs(latitudeRad) > Math.PI / 2)
				throw new ArgumentOutOfRangeException("|latitudeRad| > PI / 2");

			double r = Math.Cos(latitudeRad);
			return new Vector3(r * Math.Cos(longitudeRad), r * Math.Sin(longitudeRad), Math.Sin(latitudeRad));
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
			return Math.Atan2(p.z, Math.Sqrt(p.x * p.x + p.y * p.y));
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