//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//
// References: http://en.wikipedia.org/wiki/Equirectangular_projection
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SQLSpatialTools
{
	internal sealed class EquirectangularProjection : Projection
	{
		// Angles are in degrees.
		// longitude0:
		// parallel: standard parallels (north and south of the equator) where the scale of the projection is true
		//
		public EquirectangularProjection(Dictionary<String, double> parameters)
			: base(parameters)
		{
			double parallel_rad = InputLatitude("parallel");
			
			_scale = Math.Cos(parallel_rad);

			// scale > 0 because |parallel| <= 89.9
			Debug.Assert(_scale > 0, _scale.ToString(CultureInfo.InvariantCulture));
			_scaleInv = 1 / _scale;
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			x = longitude * _scale;
			y = latitude;
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			longitude = x * _scaleInv;
			latitude = y;
		}

		private readonly double _scale;
		private readonly double _scaleInv; // = 1 / scale
	}
}