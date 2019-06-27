//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a completely trivial conversion from geometry to geography, simply taking each
    /// point(x, y) --> (long, lat).  The class takes a target geography sink, as well as the target SRID to
    /// assign to the results.
    /// </summary>
    public class VacuousGeometryToGeographySink : IGeometrySink110
	{
		private readonly IGeographySink110 _target;
		private readonly int _targetSrid;

		public VacuousGeometryToGeographySink(int targetSrid, IGeographySink110 target)
		{
			_target = target;
			_targetSrid = targetSrid;
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			_target.AddLine(y, x, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
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
