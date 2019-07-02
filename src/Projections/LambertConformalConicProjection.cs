//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// References: http://mathworld.wolfram.com/LambertConformalConicProjection.html
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SQLSpatialTools.Projections
{
	internal sealed class LambertConformalConicProjection : Projection
	{
		// Angles are in degrees.
		// longitude0:
		// latitude: latitude for the origin of Cartesian coordinates
		// fi1: standard parallel
		// fi2: standard parallel
		//
		public LambertConformalConicProjection(IDictionary<string, double> parameters)
			: base(parameters)
		{
			var latitudeRadian = InputLatitude("latitude0");
			var fi1Radian = InputLatitude("fi1");
			var fi2Radian = InputLatitude("fi2");
			
			if (Math.Abs(fi1Radian - fi2Radian) <= MathX.Tolerance)
			{
				throw new ArgumentException(Resource.Fi1AndFi2MustBeDifferent);
			}

			// a != 0 and b != 0 because fi1 != fi2
			var a = Math.Log(Math.Cos(fi1Radian) / Math.Cos(fi2Radian));
			var b = Math.Log(Math.Tan(Math.PI / 4 + fi2Radian / 2) / Math.Tan(Math.PI / 4 + fi1Radian / 2));
			Debug.Assert(!double.IsNaN(a) && !double.IsInfinity(a) && Math.Abs(a) > 0, a.ToString(CultureInfo.InvariantCulture));
			Debug.Assert(!double.IsNaN(b) && !double.IsInfinity(b) && Math.Abs(b) > 0, b.ToString(CultureInfo.InvariantCulture));

			_n = a / b;
			_nInv = b / a;

			// tan_fi1 > 0 because fi1 is in range (-Pi/2, Pi/2)
			var tanFi1 = Math.Tan(Math.PI / 4 + fi1Radian / 2);
			Debug.Assert(!double.IsInfinity(tanFi1) && tanFi1 > 0, tanFi1.ToString(CultureInfo.InvariantCulture));
			_f = Math.Cos(fi1Radian) * Math.Pow(tanFi1, _n) * _nInv;
			Debug.Assert(Math.Abs(_f) > MathX.Tolerance);

			// tan_latitude > 0 because latitude is in range (-Pi/2, Pi/2)
			var tanLatitude = Math.Tan(Math.PI / 4 + latitudeRadian / 2);
			Debug.Assert(!double.IsInfinity(tanLatitude) && tanLatitude > 0, tanLatitude.ToString(CultureInfo.InvariantCulture));
			_ro0 = _f * Math.Pow(tanLatitude, -_n);
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			var a = Math.Tan(Math.PI / 4 + latitude / 2);
			var ro = _f * (a <= MathX.Tolerance ? 0 : Math.Pow(a, -_n));
			var theta = _n * longitude;

			x = ro * Math.Sin(theta);
			y = _ro0 - ro * Math.Cos(theta);
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			var ro = Math.Sign(_n) * Math.Sqrt(x * x + MathX.Square(_ro0 - y));
			// If ro is zero or very small then latitude will be +-Pi/2 depending on the sign of f.
			latitude = 2 * Math.Atan(Math.Pow(_f / ro, _nInv)) - Math.PI / 2;
			longitude = MathX.Clamp(Math.PI, Math.Atan2(x, _ro0 - y) * _nInv);
		}

		private readonly double _n;
		private readonly double _nInv; // = 1 / n
		private readonly double _f;
		private readonly double _ro0;
	}
}