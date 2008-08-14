// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	/**
	 * This class implements a completely trivial conversion from geometry to geography, simply taking each
	 * point (x, y) --> (long, lat).  The class takes a target geography sink, as well as the target SRID to
	 * assign to the results.
	 */
	public class VacuousGeometryToGeographySink : IGeometrySink
	{
		private readonly IGeographySink _target;
		private readonly int _targetSrid;

		public VacuousGeometryToGeographySink(int targetSrid, IGeographySink target)
		{
			_target = target;
			_targetSrid = targetSrid;
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			_target.AddLine(y, x, z, m);
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_target.BeginFigure(y, x, z, m);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			// Convert geography to geometry types...
			_target.BeginGeography((OpenGisGeographyType) type);
		}

		public void EndFigure()
		{
			_target.EndFigure();
		}

		public void EndGeometry()
		{
			_target.EndGeography();
		}

		public void SetSrid(int srid)
		{
			_target.SetSrid(_targetSrid);
		}
	}
}
