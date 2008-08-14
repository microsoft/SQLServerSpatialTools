//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// Purpose: Misc math functions.
//------------------------------------------------------------------------------
using System;
using System.Globalization;

namespace SQLSpatialTools
{
	internal static class MathX
	{
		public static readonly double Tolerance = 1e-14;

		public static double Atan2(double y, double x, string name)
		{
			if (Math.Abs(y) <= Tolerance && Math.Abs(x) <= Tolerance)
			{
				throw new ArgumentException(name);
			}
			return Atan2(y, x);
		}

		public static double Atan2(double y, double x)
		{
			double a = Math.Atan2(y, x);
			return a == Math.PI ? -Math.PI : a;
		}

		public static double Clamp(double limit, double a)
		{
			if (a > limit)
				return limit;
			if (a < -limit)
				return -limit;
			return a;
		}

		public static double InputLat(double latDeg, double max, string name)
		{
			if (Double.IsNaN(latDeg) || latDeg < -max || latDeg > max)
			{
				throw new ArgumentOutOfRangeException(name, String.Format(CultureInfo.InvariantCulture, Resource.InputLatitudeIsOutOfRange, latDeg, max));
			}
			return Clamp(Math.PI / 2, Util.ToRadians(latDeg));
		}

		public static double InputLong(double longDeg, double max, string name)
		{
			if (Double.IsNaN(longDeg) || longDeg < -max || longDeg > max)
			{
				throw new ArgumentOutOfRangeException("longitude", String.Format(CultureInfo.InvariantCulture, Resource.InputLongitudeIsOutOfRange, longDeg, max));
			}
			return NormalizeLongitudeRad(Util.ToRadians(longDeg));
		}

		// Remainder(2, 1.5) == 0.5
		// Remainder(2, -1.5) == 0.5
		// Remainder(-2, 1.5) == 1
		// Remainder(-2, -1.5) == 1
		// def: x = y * integer + rem, 0 <= rem < x
		public static double Remainder(double x, double y)
		{
			return x - Math.Floor(x / y) * y;
		}

		// Returns longitude in range [-180, 180).
		// Has the same meaning as:
		//   while(longitude >= 180) longitude -= 360
		//   while(longitude < -180) longitude += 360
		//
		public static double NormalizeLongitudeDeg(double longitude)
		{
			return -180 <= longitude && longitude < 180 ? longitude : Remainder(longitude + 180, 360) - 180;
		}

		// Returns longitude in range [-Pi, Pi).
		// Has the same meaning as:
		//   while(longitude >= Pi) longitude -= Pi * 2
		//   while(longitude < -Pi) longitude += Pi * 2
		//
		public static double NormalizeLongitudeRad(double longitude)
		{
			return -Math.PI <= longitude && longitude < Math.PI ? longitude : Remainder(longitude + Math.PI, Math.PI * 2) - Math.PI;
		}

		public static double Square(double a)
		{
			return a * a;
		}
	}
}