//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// References: http://mathworld.wolfram.com/LambertConformalConicProjection.html
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SQLSpatialTools
{
	internal sealed class LambertConformalConicProjection : Projection
	{
		// Angles are in degrees.
		// longitude0:
		// latitude: latitude for the origin of cartesian coordinates
		// fi1: standard parallel
		// fi2: standard parallel
		//
		public LambertConformalConicProjection(Dictionary<String,double> parameters)
			: base(parameters)
		{
			double latitude_rad = InputLatitude("latitude0");
			double fi1_rad = InputLatitude("fi1");
			double fi2_rad = InputLatitude("fi2");
			
			if (Math.Abs(fi1_rad - fi2_rad) <= MathX.Tolerance)
			{
				throw new ArgumentException(Resource.Fi1AndFi2MustBeDifferent);
			}

			// a != 0 and b != 0 because fi1 != fi2
			double a = Math.Log(Math.Cos(fi1_rad) / Math.Cos(fi2_rad));
			double b = Math.Log(Math.Tan(Math.PI / 4 + fi2_rad / 2) / Math.Tan(Math.PI / 4 + fi1_rad / 2));
			Debug.Assert(!Double.IsNaN(a) && !Double.IsInfinity(a) && Math.Abs(a) > 0, a.ToString(CultureInfo.InvariantCulture));
			Debug.Assert(!Double.IsNaN(b) && !Double.IsInfinity(b) && Math.Abs(b) > 0, b.ToString(CultureInfo.InvariantCulture));

			_n = a / b;
			_nInv = b / a;

			// tan_fi1 > 0 because fi1 is in range (-Pi/2, Pi/2)
			double tan_fi1 = Math.Tan(Math.PI / 4 + fi1_rad / 2);
			Debug.Assert(!Double.IsInfinity(tan_fi1) && tan_fi1 > 0, tan_fi1.ToString(CultureInfo.InvariantCulture));
			_f = Math.Cos(fi1_rad) * Math.Pow(tan_fi1, _n) * _nInv;
			Debug.Assert(Math.Abs(_f) > MathX.Tolerance);

			// tan_latitude > 0 because latitude is in range (-Pi/2, Pi/2)
			double tan_latitude = Math.Tan(Math.PI / 4 + latitude_rad / 2);
			Debug.Assert(!Double.IsInfinity(tan_latitude) && tan_latitude > 0, tan_latitude.ToString(CultureInfo.InvariantCulture));
			_ro_0 = _f * Math.Pow(tan_latitude, -_n);
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			double a = Math.Tan(Math.PI / 4 + latitude / 2);
			double ro = _f * (a <= MathX.Tolerance ? 0 : Math.Pow(a, -_n));
			double theta = _n * longitude;

			x = ro * Math.Sin(theta);
			y = _ro_0 - ro * Math.Cos(theta);
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			double ro = Math.Sign(_n) * Math.Sqrt(x * x + MathX.Square(_ro_0 - y));
			// If ro is zero or very small then then latitude will be +-Pi/2 depending on the sign of f.
			latitude = 2 * Math.Atan(Math.Pow(_f / ro, _nInv)) - Math.PI / 2;
			longitude = MathX.Clamp(Math.PI, Math.Atan2(x, _ro_0 - y) * _nInv);
		}

		private readonly double _n;
		private readonly double _nInv; // = 1 / n
		private readonly double _f;
		private readonly double _ro_0;
	}
}