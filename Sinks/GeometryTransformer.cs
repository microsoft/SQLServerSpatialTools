//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class GeometryTransformer : IGeometrySink110
	{
		readonly IGeometrySink110 _sink;
		readonly AffineTransform _transform;

		public GeometryTransformer(IGeometrySink110 sink, AffineTransform transform)
		{
			_sink = sink;
			_transform = transform;
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeometry(type);
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_sink.BeginFigure(_transform.GetX(x, y), _transform.GetY(x, y), z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			_sink.AddLine(_transform.GetX(x, y), _transform.GetY(x, y), z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void EndGeometry()
		{
			_sink.EndGeometry();
		}
	}
}