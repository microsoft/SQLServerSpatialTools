//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// References: http://mathworld.wolfram.com/AlbersEqual-AreaConicProjection.html
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace SQLSpatialTools
{
	// EPSG Code:    9822
	// OGC WKT Name: Albers_Conic_Equal_Area
	internal sealed class AlbersEqualAreaProjection : Projection
	{
		// longitude0, latitude0, parallel1, parallel2
		public AlbersEqualAreaProjection(Dictionary<String,double> parameters)
			: base(parameters)
		{
			double latitude0_rad = InputLatitude("latitude0");
			double parallel1_rad = InputLatitude("parallel1");
			double parallel2_rad = InputLatitude("parallel2");

			double cos_parallel1 = Math.Cos(parallel1_rad);
			double sin_parallel1 = Math.Sin(parallel1_rad);

			_n2 = sin_parallel1 + Math.Sin(parallel2_rad);
			if (Math.Abs(_n2) <= MathX.Tolerance)
			{
				throw new ArgumentOutOfRangeException();
			}

			_n = _n2 / 2;
			_n_half = _n2 / 4;
			_inv_n2 = 1 / _n2;
			_inv_n = 2 / _n2;

			_c = MathX.Square(cos_parallel1) + sin_parallel1 * _n2;
			_c_over_n2 = _c * _inv_n2;

			double a = _c - Math.Sin(latitude0_rad) * _n2;
			if (a < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			_ro_0 = Math.Sqrt(a) * _inv_n;
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			double a = _c - Math.Sin(latitude) * _n2;
			if (a < 0)
			{
				throw new ArgumentOutOfRangeException("latitude");
			}

			double ro = Math.Sqrt(a) * _inv_n;
	
			double tetha = longitude * _n;
			x = ro * Math.Sin(tetha);
			y = _ro_0 - ro * Math.Cos(tetha);
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			double ros = x * x + (_ro_0 - y) * (_ro_0 - y);
			double a = _c_over_n2 - ros * _n_half;
			if (Double.IsNaN(a) || Math.Abs(a) > 1 + MathX.Tolerance)
			{
				throw new ArgumentOutOfRangeException("x, y");
			}

			latitude = Math.Asin(MathX.Clamp(1, a));
			longitude = MathX.Clamp(Math.PI, MathX.Atan2(x, _ro_0 - y, "x, y") * _inv_n);
		}

		private readonly double _c;
		private readonly double _c_over_n2;
		private readonly double _ro_0;
		private readonly double _n2;
		private readonly double _n;
		private readonly double _n_half;
		private readonly double _inv_n2;
		private readonly double _inv_n;
	}
}