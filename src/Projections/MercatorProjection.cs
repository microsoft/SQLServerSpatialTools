//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// References: http://mathworld.wolfram.com/MercatorProjection.html
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Projections
{
	internal sealed class MercatorProjection : Projection
	{
		private const double MaxLatitudeDeg = 89.5;
		private readonly double _maxY = Math.Log(Math.Tan(Math.PI / 4 + SpatialUtil.ToRadians(MaxLatitudeDeg) / 2));

		// longitude0: Reference longitude
		//
		public MercatorProjection(IDictionary<string, double> parameters)
			: base(parameters)
		{
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			x = longitude;
			y = MathX.Clamp(_maxY, Math.Log(Math.Tan(Math.PI / 4 + latitude / 2))); // protect against +-Infinity
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			longitude = x;
			
			if (y >= _maxY)
			{
				latitude = Math.PI / 2;
			}
			else if (y <= -_maxY)
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