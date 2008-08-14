//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// References: http://mathworld.wolfram.com/MercatorProjection.html
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace SQLSpatialTools
{
	internal sealed class MercatorProjection : Projection
	{
		private const double MaxLatitudeDeg = 89.5;
		private readonly double MaxY = Math.Log(Math.Tan(Math.PI / 4 + Util.ToRadians(MaxLatitudeDeg) / 2));

		// longitude0: Reference longitude
		//
		public MercatorProjection(Dictionary<String,double> parameters)
			: base(parameters)
		{
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			x = longitude;
			y = MathX.Clamp(MaxY, Math.Log(Math.Tan(Math.PI / 4 + latitude / 2))); // protect againt +-Infinity
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			longitude = x;
			
			if (y >= MaxY)
			{
				latitude = Math.PI / 2;
			}
			else if (y <= -MaxY)
			{
				latitude = -Math.PI / 2;
			}
			else
			{
				latitude = Math.Atan(Math.Exp(y)) * 2 - Math.PI / 2;
			}
		}
	}
}