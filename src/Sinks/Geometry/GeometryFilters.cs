//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{

    public class GeometryEmptyShapeFilter : IGeometrySink110
	{
		private readonly IGeometrySink110 _sink;
		private readonly Queue<OpenGisGeometryType> _types = new Queue<OpenGisGeometryType>();
		private bool _root = true;

		public GeometryEmptyShapeFilter(IGeometrySink110 sink)
		{
			_sink = sink;
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (_root)
			{
				_root = false;
				_sink.BeginGeometry(type);
			}
			else
			{
				_types.Enqueue(type);
			}
		}

		public void EndGeometry()
		{
			if (_types.Count > 0)
				_types.Dequeue();
			else
				_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			while (_types.Count > 0)
				_sink.BeginGeometry(_types.Dequeue());
			_sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			_sink.AddLine(x, y, z, m);
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

	public class GeometryPointFilter : IGeometrySink110
	{
		private readonly IGeometrySink110 _sink;
		private int _depth;
		private bool _root = true;

		public GeometryPointFilter(IGeometrySink110 sink)
		{
			_sink = sink;
		}

		public void SetSrid(int srid)
		{
			_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (type == OpenGisGeometryType.Point || type == OpenGisGeometryType.MultiPoint)
			{
				if (_root)
				{
					_root = false;
					_sink.BeginGeometry(OpenGisGeometryType.GeometryCollection);
					_sink.EndGeometry();
				}
				_depth++;
			}
			else
			{
				_sink.BeginGeometry(type);
			}
		}

		public void EndGeometry()
		{
			if (_depth > 0)
				_depth--;
			else
				_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_depth == 0)
				_sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			_sink.AddLine(x, y, z, m);
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

	public class GeometryShortLineStringFilter : IGeometrySink110
	{
		private readonly IGeometrySink110 _sink;
		private readonly double _tolerance;
		private int _srid;
		private bool _insideLineString;
		private readonly List<Vertex> _figure = new List<Vertex>();

		public GeometryShortLineStringFilter(IGeometrySink110 sink, double tolerance)
		{
			_sink = sink;
			_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			_srid = srid;
			_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeometry(type);
			_insideLineString = type == OpenGisGeometryType.LineString;
		}

		public void EndGeometry()
		{
			_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_insideLineString)
			{
				_figure.Clear();
				_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				_sink.BeginFigure(x, y, z, m);
			}
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (_insideLineString)
			{
				_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				_sink.AddLine(x, y, z, m);
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
				SqlGeometryBuilder b = new SqlGeometryBuilder();
				b.SetSrid(_srid);
				b.BeginGeometry(OpenGisGeometryType.LineString);
				PopulateFigure(b);
				b.EndGeometry();
				return b.ConstructedGeometry.STLength().Value < _tolerance;
			}
			catch (ArgumentException) { }
			catch (FormatException) { }
			return true;
		}

		private void PopulateFigure(IGeometrySink110 sink)
		{
			_figure[0].BeginFigure(sink);
			for (int i = 1; i < _figure.Count; i++)
				_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}

	public class GeometryThinRingFilter : IGeometrySink110
	{
		private readonly IGeometrySink110 _sink;
		private readonly double _tolerance;
		private bool _insidePolygon;
		private int _srid;
		private readonly List<Vertex> _figure = new List<Vertex>();

		public GeometryThinRingFilter(IGeometrySink110 sink, double tolerance)
		{
			_sink = sink;
			_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			_srid = srid;
			_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeometry(type);
			_insidePolygon = type == OpenGisGeometryType.Polygon;
		}

		public void EndGeometry()
		{
			_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_insidePolygon)
			{
				_figure.Clear();
				_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				_sink.BeginFigure(x, y, z, m);
			}
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (_insidePolygon)
			{
				_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				_sink.AddLine(x, y, z, m);
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
					PopulateFigure(_sink);
				}
			}
			else
			{
				_sink.EndFigure();
			}
		}

		private bool IsThinRing()
		{
			try
			{
				var builder = new SqlGeometryBuilder();
				builder.SetSrid(_srid);
				builder.BeginGeometry(OpenGisGeometryType.Polygon);
				PopulateFigure(builder);
				builder.EndGeometry();
				var poly = builder.ConstructedGeometry.MakeValid();
				return poly.STArea().Value < _tolerance * poly.STLength().Value;
			}
			catch (ArgumentException) { }
			catch (FormatException) { }
			return true;
		}

		private void PopulateFigure(IGeometrySink110 sink)
		{
			_figure[0].BeginFigure(sink);
			for (int i = 1; i < _figure.Count; i++)
				_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}
}