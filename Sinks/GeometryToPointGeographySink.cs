//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	//
	// Sink which extracts points from a geometry instance and forwards them to a geography sink
	//
	public sealed class GeometryToPointGeographySink : IGeometrySink
	{
		private readonly IGeographySink _sink;
		private int _count;

		public GeometryToPointGeographySink(IGeographySink sink)
		{
			_sink = sink;
			_count = 0;
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (_count == 0)
			{
				_sink.BeginGeography(OpenGisGeographyType.MultiPoint);
			}
			_count++;
		}

		public void EndGeometry()
		{
			_count--;
			if (_count == 0)
			{
				_sink.EndGeography();
			}
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			_sink.BeginGeography(OpenGisGeographyType.Point);
			_sink.BeginFigure(y, x, z, m);
			_sink.EndFigure();
			_sink.EndGeography();
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			BeginFigure(x, y, z, m);
		}

		public void EndFigure()
		{
			// we ignore these calls
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}
	}
}