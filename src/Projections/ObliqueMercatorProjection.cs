//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// References: http://mathworld.wolfram.com/MercatorProjection.html
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SQLSpatialTools.Projections
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
		public ObliqueMercatorProjection(IDictionary<string, double> parameters)
			: base(parameters)
		{
			var fi1Rad = InputLatitude("fi1");
			var fi2Rad = InputLatitude("fi2");
			
			var lambda1Rad = InputLongitude("lambda1");
			var lambda2Rad = InputLongitude("lambda2");

			var cosFi1 = Math.Cos(fi1Rad);
			var sinFi1 = Math.Sin(fi1Rad);

			var cosFi2 = Math.Cos(fi2Rad);
			var sinFi2 = Math.Sin(fi2Rad);
			
			var a = cosFi1 * sinFi2 * Math.Cos(lambda1Rad);
			var b = sinFi1 * cosFi2 * Math.Cos(lambda2Rad);
			var c = sinFi1 * cosFi2 * Math.Sin(lambda2Rad);
			var d = cosFi1 * sinFi2 * Math.Sin(lambda1Rad);
			var lambdaP = Math.Atan2(a - b, c - d);

			var fiP = Math.Atan2(-Math.Cos(lambdaP - lambda1Rad), Math.Tan(fi1Rad));
			_cosFiP = Math.Cos(fiP);
			_sinFiP = Math.Sin(fiP);
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			var sinLatitude = Math.Sin(latitude);
			var cosLatitude = Math.Cos(latitude);

			var sinLongitude = Math.Sin(longitude);
			var cosLongitude = Math.Cos(longitude);

			var a = _sinFiP * sinLatitude - _cosFiP * cosLatitude * sinLongitude; // sin_fi_p
			if (double.IsNaN(a) || Math.Abs(a) > 1)
			{
				throw new ArgumentOutOfRangeException(string.Join(",", new[] { nameof(latitude), nameof(longitude) }));
			}

			x = MathX.Atan2(sinLatitude / cosLatitude * _cosFiP + sinLongitude * _sinFiP, cosLongitude, "latitude, longitude");
			y = MathX.Clamp(MaxY, Math.Log((1 + a) / (1 - a)) / 2); // protect against +-Infinity
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			var sinX = Math.Sin(x);
			var cosX = Math.Cos(x);

			var sinhY = Math.Sinh(y);
			var coshY = Math.Cosh(y);

			// cosh_y >= 1 by definition
			var a = (_sinFiP * sinhY + _cosFiP * sinX) / coshY;
			if (Math.Abs(a) > 1)
			{
				throw new ArgumentOutOfRangeException(string.Join(",",new []{nameof(x), nameof(y)}));
			}

			latitude = Math.Asin(a);
			longitude = MathX.Atan2(_sinFiP * sinX - _cosFiP * sinhY, cosX, "x, y");
		}

		private readonly double _cosFiP; // = cos(fi_p)
		private readonly double _sinFiP; // = sin(fi_p)
	}
}