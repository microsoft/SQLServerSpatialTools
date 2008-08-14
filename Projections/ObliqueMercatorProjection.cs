//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
// 
// References: http://mathworld.wolfram.com/MercatorProjection.html
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace SQLSpatialTools
{
	internal sealed class ObliqueMercatorProjection : Projection
	{
        private const double MaxY = 5;

		// Angles are in degrees.
		// longitude0:
		// fi1:
		// lambda1:
		// fi2:
		// lambda2:
		//
		public ObliqueMercatorProjection(Dictionary<string, double> parameters)
			: base(parameters)
		{
			double fi1_rad = InputLatitude("fi1");
			double fi2_rad = InputLatitude("fi2");
			
			double lambda1_rad = InputLongitude("lambda1");
			double lambda2_rad = InputLongitude("lambda2");

			double cos_fi1 = Math.Cos(fi1_rad);
			double sin_fi1 = Math.Sin(fi1_rad);

			double cos_fi2 = Math.Cos(fi2_rad);
			double sin_fi2 = Math.Sin(fi2_rad);
			
			double a = cos_fi1 * sin_fi2 * Math.Cos(lambda1_rad);
			double b = sin_fi1 * cos_fi2 * Math.Cos(lambda2_rad);
			double c = sin_fi1 * cos_fi2 * Math.Sin(lambda2_rad);
			double d = cos_fi1 * sin_fi2 * Math.Sin(lambda1_rad);
			double lambda_p = Math.Atan2(a - b, c - d);

			double fi_p = Math.Atan2(-Math.Cos(lambda_p - lambda1_rad), Math.Tan(fi1_rad));
			_cos_fi_p = Math.Cos(fi_p);
			_sin_fi_p = Math.Sin(fi_p);
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			double sin_lat = Math.Sin(latitude);
			double cos_lat = Math.Cos(latitude);

			double sin_long = Math.Sin(longitude);
			double cos_long = Math.Cos(longitude);

			double a = _sin_fi_p * sin_lat - _cos_fi_p * cos_lat * sin_long; // sin_fi_p
			if (Double.IsNaN(a) || Math.Abs(a) > 1)
			{
				throw new ArgumentOutOfRangeException("latitude, longitude");
			}

			x = MathX.Atan2(sin_lat / cos_lat * _cos_fi_p + sin_long * _sin_fi_p, cos_long, "latitude, longitude");
			y = MathX.Clamp(MaxY, Math.Log((1 + a) / (1 - a)) / 2); // protect againt +-Infinity
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			double sin_x = Math.Sin(x);
			double cos_x = Math.Cos(x);

			double sinh_y = Math.Sinh(y);
			double cosh_y = Math.Cosh(y);

			// cosh_y >= 1 by definition
			double a = (_sin_fi_p * sinh_y + _cos_fi_p * sin_x) / cosh_y;
			if (Math.Abs(a) > 1)
			{
				throw new ArgumentOutOfRangeException("x, y");
			}

			latitude = Math.Asin(a);
			longitude = MathX.Atan2(_sin_fi_p * sin_x - _cos_fi_p * sinh_y, cos_x, "x, y");
		}

		private readonly double _cos_fi_p; // = cos(fi_p)
		private readonly double _sin_fi_p; // = sin(fi_p)
	}
}