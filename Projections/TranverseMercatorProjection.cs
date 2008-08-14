//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// References: http://mathworld.wolfram.com/MercatorProjection.html
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace SQLSpatialTools
{
	internal sealed class TranverseMercatorProjection : Projection
	{
		private const double MaxX = 10;

		// longitude0: Reference longitude.
		//
		public TranverseMercatorProjection(Dictionary<string, double> parameters)
			: base(parameters)
		{
		}

		// x will be in range [-MaxX, MaxX]
		// y will be in range [-Pi, Pi]
		protected internal override void Project(double latitude, double longitude, out double x, out double y)
        {
			// North pole
			if (latitude >= Math.PI / 2 - MathX.Tolerance)
			{
				x = 0;
				y = Math.PI / 2;
				return;
			}

			// South pole
			if (latitude <= -Math.PI / 2 + MathX.Tolerance)
			{
				x = 0;
				y = -Math.PI / 2;
				return;
			}
			
			if (Math.Abs(latitude) <= MathX.Tolerance)
			{
				// East of India
				if (Math.Abs(longitude - Math.PI / 2) <= MathX.Tolerance)
				{
					x = MaxX;
					y = 0;
					return;
				}
				// West of South America
				if (Math.Abs(longitude + Math.PI / 2) <= MathX.Tolerance)
				{
					x = -MaxX;
					y = 0;
					return;
				}
			}

			double b = Math.Cos(latitude) * Math.Sin(longitude);
			x = MathX.Clamp(MaxX, Math.Log((1 + b) / (1 - b)) / 2);
			y = Math.Atan2(Math.Tan(latitude), Math.Cos(longitude));
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			if (x >= MaxX)
			{
				latitude = 0;
				longitude = Math.PI / 2;
			}
			else if (x <= -MaxX)
			{
				latitude = 0;
				longitude = -Math.PI / 2;
			}
			else
			{
				// 1 <= cosh(x) <= cosh(MaxX)
				latitude = Math.Asin(Math.Sin(y) / Math.Cosh(x));
				// In case of x=0 and y=+-Pi/2: latitude will be +-90 and longtude will be 0
				longitude = MathX.Atan2(Math.Sinh(x), Math.Cos(y));
			}
		}
	}
}