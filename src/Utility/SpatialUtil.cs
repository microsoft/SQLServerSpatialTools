//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Utility
{
    /// <summary>
    /// This class contains functions that are meant to be used internally in this library.
    /// </summary>
    internal static class SpatialUtil
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
				throw new ArgumentOutOfRangeException(nameof(latitudeDeg),"|latitudeDeg| > 90");

			var latitudeRad = ToRadians(latitudeDeg);
			var longitudeRad = ToRadians(longitudeDeg);
			var r = Math.Cos(latitudeRad);
			return new Vector3(r * Math.Cos(longitudeRad), r * Math.Sin(longitudeRad), Math.Sin(latitudeRad));
		}

		// Convert a Lat/Long in radians to an X,Y,Z vector.
		public static Vector3 SphericalRadToCartesian(double latitudeRad, double longitudeRad)
		{
			if (Math.Abs(latitudeRad) > Math.PI / 2)
				throw new ArgumentOutOfRangeException(nameof(latitudeRad), "|latitudeRad| > PI / 2");

			var r = Math.Cos(latitudeRad);
			return new Vector3(r * Math.Cos(longitudeRad), r * Math.Sin(longitudeRad), Math.Sin(latitudeRad));
		}

		// Returns longitude in radians given the vector.
		public static double Longitude(Vector3 p)
		{ 
			return Math.Atan2(p.Y, p.X); 
		}

		// Returns longitude in degrees given the vector.
		public static double LongitudeDeg(Vector3 p)
		{ 
			return ToDegrees(Longitude(p)); 
		}

		// Returns latitude in radians given the vector.
		public static double Latitude(Vector3 p)
		{
			return Math.Atan2(p.Z, Math.Sqrt(p.X * p.X + p.Y * p.Y));
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