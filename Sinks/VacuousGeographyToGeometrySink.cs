// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	/**
	 * This class implements a completely trivial conversion from geography to geometry, simply taking each
	 * point (lat,long) --> (y, x).  The class takes a target geometry sink, as well as the target SRID to
	 * assign to the results.
	 */
	public class VacuousGeographyToGeometrySink : IGeographySink
	{
		private readonly IGeometrySink _target;
		private readonly int _targetSrid;

		public VacuousGeographyToGeometrySink(int targetSrid, IGeometrySink target)
		{
			_target = target;
			_targetSrid = targetSrid;
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			_target.AddLine(longitude, latitude, z, m);
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			_target.BeginFigure(longitude, latitude, z, m);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			// Convert geography to geometry types...
			_target.BeginGeometry((OpenGisGeometryType) type);
		}

		public void EndFigure()
		{
			_target.EndFigure();
		}

		public void EndGeography()
		{
			_target.EndGeometry();
		}

		public void SetSrid(int srid)
		{
			_target.SetSrid(_targetSrid);
		}
	}
}
