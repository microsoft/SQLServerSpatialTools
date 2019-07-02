//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// References: http://mathworld.wolfram.com/AlbersEqual-AreaConicProjection.html
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SQLSpatialTools.Projections
{
	// EPSG Code:    9822
	// OGC WKT Name: Albers_Conic_Equal_Area
	internal sealed class AlbersEqualAreaProjection : Projection
	{
		// longitude0, latitude0, parallel1, parallel2
		public AlbersEqualAreaProjection(IDictionary<string, double> parameters)
			: base(parameters)
		{
            var latitude0Rad = InputLatitude("latitude0");
			var parallel1Rad = InputLatitude("parallel1");
			var parallel2Rad = InputLatitude("parallel2");

			var cosParallel1 = Math.Cos(parallel1Rad);
			var sinParallel1 = Math.Sin(parallel1Rad);

			_n2 = sinParallel1 + Math.Sin(parallel2Rad);
			if (Math.Abs(_n2) <= MathX.Tolerance)
			{
				throw new ArgumentOutOfRangeException();
			}

			_n = _n2 / 2;
			_nHalf = _n2 / 4;
			var invN2 = 1 / _n2;
			_invN = 2 / _n2;

			_c = MathX.Square(cosParallel1) + sinParallel1 * _n2;
			_cOverN2 = _c * invN2;

			var a = _c - Math.Sin(latitude0Rad) * _n2;
			if (a < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			_ro0 = Math.Sqrt(a) * _invN;
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			var a = _c - Math.Sin(latitude) * _n2;
			if (a < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(latitude));
			}

			var ro = Math.Sqrt(a) * _invN;
	
			var theta = longitude * _n;
			x = ro * Math.Sin(theta);
			y = _ro0 - ro * Math.Cos(theta);
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			var ros = x * x + (_ro0 - y) * (_ro0 - y);
			var a = _cOverN2 - ros * _nHalf;
			if (double.IsNaN(a) || Math.Abs(a) > 1 + MathX.Tolerance)
			{
				throw new ArgumentOutOfRangeException(paramName:string.Join(",", new []{nameof(x),nameof(y)}));
			}

			latitude = Math.Asin(MathX.Clamp(1, a));
			longitude = MathX.Clamp(Math.PI, MathX.Atan2(x, _ro0 - y, "x, y") * _invN);
		}

		private readonly double _c;
		private readonly double _cOverN2;
		private readonly double _ro0;
		private readonly double _n2;
		private readonly double _n;
		private readonly double _nHalf;
        private readonly double _invN;
	}
}