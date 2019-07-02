//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geography
{
	public class GeographyEmptyShapeFilter : IGeographySink110
	{
		private readonly IGeographySink110 _sink;
		private readonly Queue<OpenGisGeographyType> _types = new Queue<OpenGisGeographyType>();
		private bool _root = true;

		public GeographyEmptyShapeFilter(IGeographySink110 sink)
		{
			_sink = sink;
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (_root)
			{
				_root = false;
				_sink.BeginGeography(type);
			}
			else
			{
				_types.Enqueue(type);
			}
		}

		public void EndGeography()
		{
			if (_types.Count > 0)
				_types.Dequeue();
			else
				_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			while (_types.Count > 0)
				_sink.BeginGeography(_types.Dequeue());
			_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			_sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			_sink.EndFigure();
		}
	}

	public class GeographyPointFilter : IGeographySink110
	{
		private readonly IGeographySink110 _sink;
		private int _depth;
		private bool _root = true;

		public GeographyPointFilter(IGeographySink110 sink)
		{
			_sink = sink;
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (type == OpenGisGeographyType.Point || type == OpenGisGeographyType.MultiPoint)
			{
				if (_root)
				{
					_root = false;
					_sink.BeginGeography(OpenGisGeographyType.GeometryCollection);
					_sink.EndGeography();
				}
				_depth++;
			}
			else
			{
				_root = false;
				_sink.BeginGeography(type);
			}
		}

		public void EndGeography()
		{
			if (_depth > 0)
				_depth--;
			else
				_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (_depth == 0)
				_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (_depth == 0)
				_sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (_depth == 0)
				_sink.EndFigure();
		}
	}

	public class GeographyShortLineStringFilter : IGeographySink110
	{
		private readonly IGeographySink110 _sink;
		private readonly double _tolerance;
		private int _srid;
		private bool _insideLineString;
		private readonly List<Vertex> _figure = new List<Vertex>();

		public GeographyShortLineStringFilter(IGeographySink110 sink, double tolerance)
		{
			_sink = sink;
			_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			_srid = srid;
			_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			_sink.BeginGeography(type);
			_insideLineString = type == OpenGisGeographyType.LineString;
		}

		public void EndGeography()
		{
			_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (_insideLineString)
			{
				_figure.Clear();
				_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				_sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (_insideLineString)
			{
				_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				_sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (_insideLineString)
			{
				if (!IsShortLineString())
				{
					PopulateFigure(_sink);
				}
			}
			else
			{
				_sink.EndFigure();
			}
		}

		private bool IsShortLineString()
		{
			try
			{
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(_srid);
				b.BeginGeography(OpenGisGeographyType.LineString);
				PopulateFigure(b);
				b.EndGeography();
				SqlGeography g = b.ConstructedGeography;
				return g.STLength().Value < _tolerance;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }
			return false;
		}

		private void PopulateFigure(IGeographySink110 sink)
		{
			_figure[0].BeginFigure(sink);
			for (int i = 1; i < _figure.Count; i++)
				_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}

	public class GeographyThinRingFilter : IGeographySink110
	{
		private readonly IGeographySink110 _sink;
		private readonly double _tolerance;
		private bool _insidePolygon;
		private int _srid;
		private readonly List<Vertex> _figure = new List<Vertex>();

		public GeographyThinRingFilter(IGeographySink110 sink, double tolerance)
		{
			_sink = sink;
			_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			_srid = srid;
			_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			_sink.BeginGeography(type);
			_insidePolygon = type == OpenGisGeographyType.Polygon;
		}

		public void EndGeography()
		{
			_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (_insidePolygon)
			{
				_figure.Clear();
				_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				_sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (_insidePolygon)
			{
				_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				_sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (_insidePolygon)
			{
				if (!IsThinRing())
				{
					PopulateFigure(_sink, false);
				}
			}
			else
			{
				_sink.EndFigure();
			}
		}

		private bool IsThinRing()
		{
			SqlGeography poly = RingToPolygon(true);
			if (poly == null)
			{
				// ring was not valid, try with different orientation
				poly = RingToPolygon(false);
				if (poly == null)
				{
					// if both orientations are invalid, we are dealing with very thin ring
					// so just return true
					return true;
				}
			}
			return poly.STArea().Value < _tolerance * poly.STLength().Value;
		}

		private SqlGeography RingToPolygon(bool reverse)
		{
			try
			{
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(_srid);
				b.BeginGeography(OpenGisGeographyType.Polygon);
				PopulateFigure(b, reverse);
				b.EndGeography();
				return b.ConstructedGeography;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }
			return null;
		}

		private void PopulateFigure(IGeographySink110 sink, bool reverse)
		{
			if (reverse)
			{
				_figure[_figure.Count - 1].BeginFigure(sink);
				for (int i = _figure.Count - 2; i >= 0; i--)
					_figure[i].AddLine(sink);
			}
			else
			{
				_figure[0].BeginFigure(sink);
				for (int i = 1; i < _figure.Count; i++)
					_figure[i].AddLine(sink);
			}
			sink.EndFigure();
		}
	}
}