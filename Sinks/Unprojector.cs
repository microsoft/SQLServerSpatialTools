//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public sealed class Unprojector : IGeometrySink
	{
		private readonly SqlProjection _projection;
		private readonly IGeographySink _sink;

		public Unprojector(SqlProjection projection, IGeographySink sink, int newSrid)
		{
			_projection = projection;
			_sink = sink;
			_sink.SetSrid(newSrid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeography((OpenGisGeographyType)type);
		}

		public void EndGeometry()
		{
			_sink.EndGeography();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			double latitude, longitude;
			_projection.UnprojectPoint(x, y, out latitude, out longitude);
			_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			double latitude, longitude;
			_projection.UnprojectPoint(x, y, out latitude, out longitude);
			_sink.AddLine(latitude, longitude, z, m);
		}

		public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			// Input argument not used since a new srid is defined in the constructor.
		}
	}
}