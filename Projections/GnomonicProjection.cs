//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
//
// References: http://mathworld.wolfram.com/GnomonicProjection.html
//
// Note: The gnomonic projction is the only projection that maps SqlGeography
//       Polygons and LineString exactly to their SqlGeometry counterparts.
//
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

namespace SQLSpatialTools
{
	// EPSG Code:    None
	// OGC WKT Name: Gnomonic
	internal sealed class GnommonicProjection : Projection
	{
		// Input argument: the center of the projection
		public GnommonicProjection(Dictionary<String, double> parameters)
			: base(parameters)
		{
			_center = Util.SphericalRadToCartesian(InputLatitude("latitude1"), InputLongitude("longitude1"));

			// This projection is designed for numerical computations rather than cartography.
			// The choice of coordinate basis for the tangent plane - which affects the 
			// orientation of the projection in the xy plane - is optimized for accuracy rather
			// than good looks. The first basis vector is obtained by dropping the smallest coordinate,
			// switching the other two, and flipping the sign of one of them. The second one is
			// obtained by cross product.

			double[] center = { _center.x, _center.y, _center.z };
			double[] vector = new double[3];

			int k = GetMinEntry(center);
			int j = (k + 2) % 3;
			int i = (j + 2) % 3;

			vector[i] = -center[j];
			vector[j] = center[i];
			vector[k] = 0;

			_xAxis = new Vector3(vector[0], vector[1], vector[2]).Unitize();

			_yAxis = _center.CrossProduct(_xAxis);
		}

		private static int GetMinEntry(double[] values)
		{
			int i = 0;
			if (Math.Abs(values[1]) < Math.Abs(values[0]))
				i = 1;
			if (Math.Abs(values[2]) < Math.Abs(values[i]))
				i = 2;
			return i;
		}

		protected internal override void Project(double latitude, double longitude, out double x, out double y)
		{
			Vector3 vector = Util.SphericalRadToCartesian(latitude, longitude);
			double r = vector * _center;

			if (r < _tolerance)
			{
				throw new ArgumentOutOfRangeException("Input point is too far away from the center of projection.");
			}
			vector = vector / r;

			x = vector * _xAxis;
			y = vector * _yAxis;
		}

		protected internal override void Unproject(double x, double y, out double latitude, out double longitude)
		{
			Vector3 vector = _center + _xAxis * x + _yAxis * y;
			latitude = Util.Latitude(vector);
			longitude = Util.Longitude(vector);
		}

		private readonly Vector3 _center;
		private readonly Vector3 _xAxis;
		private readonly Vector3 _yAxis;
		private static readonly double _tolerance = 1e-8;
	}
}