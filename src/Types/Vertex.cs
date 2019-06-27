//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Types
{
    internal struct Vertex
	{
        private readonly double _x;
        private readonly double _y;
        private readonly double? _z;
        private readonly double? _m;

		public Vertex(double x, double y, double? z, double? m)
		{
			_x = x;
			_y = y;
			_z = z;
			_m = m;
		}

		public void BeginFigure(IGeometrySink110 sink) { sink.BeginFigure(_x, _y, _z, _m); }
		public void AddLine(IGeometrySink110 sink) { sink.AddLine(_x, _y, _z, _m); }

		public void BeginFigure(IGeographySink110 sink) { sink.BeginFigure(_x, _y, _z, _m); }
		public void AddLine(IGeographySink110 sink) { sink.AddLine(_x, _y, _z, _m); }
	}
}