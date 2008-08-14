//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class Projector : IGeographySink
	{
		private readonly SqlProjection _projection;
		private readonly IGeometrySink _sink;

		public Projector(SqlProjection projection, IGeometrySink sink)
		{
			_projection = projection;
			_sink = sink;
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			_sink.BeginGeometry((OpenGisGeometryType)type);
		}

		public void EndGeography()
		{
			_sink.EndGeometry();
		}

		public void BeginFigure(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
			double x, y;
			_projection.ProjectPoint(latitude, longitude, out x, out y);
			_sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double latitude, double longitude, Nullable<double> z, Nullable<double> m)
		{
			double x, y;
			_projection.ProjectPoint(latitude, longitude, out x, out y);
			_sink.AddLine(x, y, z, m);
		}

		public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}
	}
}