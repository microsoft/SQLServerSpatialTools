//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class GeometryTransformer : IGeometrySink
	{
		readonly IGeometrySink _sink;
		readonly AffineTransform _transform;

		public GeometryTransformer(IGeometrySink sink, AffineTransform transform)
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