//------------------------------------------------------------------------------
// Copyright (c) 2010 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	struct Vertex
	{
		double x;
		double y;
		double? z;
		double? m;

		public Vertex(double x_, double y_, double? z_, double? m_)
		{
			x = x_;
			y = y_;
			z = z_;
			m = m_;
		}

		public void BeginFigure(IGeometrySink sink) { sink.BeginFigure(x, y, z, m); }
		public void AddLine(IGeometrySink sink) { sink.AddLine(x, y, z, m); }

		public void BeginFigure(IGeographySink sink) { sink.BeginFigure(x, y, z, m); }
		public void AddLine(IGeographySink sink) { sink.AddLine(x, y, z, m); }
	}

	public class GeometryEmptyShapeFilter : IGeometrySink
	{
		private IGeometrySink m_sink;
		private Queue<OpenGisGeometryType> m_types = new Queue<OpenGisGeometryType>();
		private bool m_root = true;

		public GeometryEmptyShapeFilter(IGeometrySink sink)
		{
			m_sink = sink;
		}

		public void SetSrid(int srid)
		{
			m_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (m_root)
			{
				m_root = false;
				m_sink.BeginGeometry(type);
			}
			else
			{
				m_types.Enqueue(type);
			}
		}

		public void EndGeometry()
		{
			if (m_types.Count > 0)
				m_types.Dequeue();
			else
				m_sink.EndGeometry();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			while (m_types.Count > 0)
				m_sink.BeginGeometry(m_types.Dequeue());
			m_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			m_sink.AddLine(latitude, longitude, z, m);
		}

		public void EndFigure()
		{
			m_sink.EndFigure();
		}
	}

	public class GeometryPointFilter : IGeometrySink
	{
		private IGeometrySink m_sink;
		private int m_depth;
		private bool m_root = true;

		public GeometryPointFilter(IGeometrySink sink)
		{
			m_sink = sink;
		}

		public void SetSrid(int srid)
		{
			m_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			if (type == OpenGisGeometryType.Point || type == OpenGisGeometryType.MultiPoint)
			{
				if (m_root)
				{
					m_root = false;
					m_sink.BeginGeometry(OpenGisGeometryType.GeometryCollection);
					m_sink.EndGeometry();
				}
				m_depth++;
			}
			else
			{
				m_sink.BeginGeometry(type);
			}
		}

		public void EndGeometry()
		{
			if (m_depth > 0)
				m_depth--;
			else
				m_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (m_depth == 0)
				m_sink.BeginFigure(x, y, z, m);
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			m_sink.AddLine(x, y, z, m);
		}

		public void EndFigure()
		{
			if (m_depth == 0)
				m_sink.EndFigure();
		}
	}

	public class GeometryShortLineStringFilter : IGeometrySink
	{
		private IGeometrySink m_sink;
		private double m_tolerance;
		private int m_srid;
		private bool m_insideLineString;
		private List<Vertex> m_figure = new List<Vertex>();

		public GeometryShortLineStringFilter(IGeometrySink sink, double tolerance)
		{
			m_sink = sink;
			m_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			m_srid = srid;
			m_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			m_sink.BeginGeometry(type);
			m_insideLineString = type == OpenGisGeometryType.LineString;
		}

		public void EndGeometry()
		{
			m_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (m_insideLineString)
			{
				m_figure.Clear();
				m_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				m_sink.BeginFigure(x, y, z, m);
			}
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (m_insideLineString)
			{
				m_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				m_sink.AddLine(x, y, z, m);
			}
		}

		public void EndFigure()
		{
			if (m_insideLineString)
			{
				if (!IsShortLineString())
				{
					PopulateFigure(m_sink);
				}
			}
			else
			{
				m_sink.EndFigure();
			}
		}

		private bool IsShortLineString()
		{
			try
			{ 
				SqlGeometryBuilder b = new SqlGeometryBuilder();
				b.SetSrid(m_srid);
				b.BeginGeometry(OpenGisGeometryType.LineString);
				PopulateFigure(b);
				b.EndGeometry();
				return b.ConstructedGeometry.STLength().Value < m_tolerance;
			}
			catch (ArgumentException) { }
			catch (FormatException) { }
			return true;
		}

		private void PopulateFigure(IGeometrySink sink)
		{
			m_figure[0].BeginFigure(sink);
			for (int i = 1; i < m_figure.Count; i++)
				m_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}

	public class GeometryThinRingFilter : IGeometrySink
	{
		private IGeometrySink m_sink;
		private double m_tolerance;
		private bool m_insidePolygon;
		private int m_srid;
		private List<Vertex> m_figure = new List<Vertex>();

		public GeometryThinRingFilter(IGeometrySink sink, double tolerance)
		{
			m_sink = sink;
			m_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			m_srid = srid;
			m_sink.SetSrid(srid);
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			m_sink.BeginGeometry(type);
			m_insidePolygon = type == OpenGisGeometryType.Polygon;
		}

		public void EndGeometry()
		{
			m_sink.EndGeometry();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (m_insidePolygon)
			{
				m_figure.Clear();
				m_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				m_sink.BeginFigure(x, y, z, m);
			}
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (m_insidePolygon)
			{
				m_figure.Add(new Vertex(x, y, z, m));
			}
			else
			{
				m_sink.AddLine(x, y, z, m);
			}
		}

		public void EndFigure()
		{
			if (m_insidePolygon)
			{
				if (!IsThinRing())
				{
					PopulateFigure(m_sink);
				}
			}
			else
			{
				m_sink.EndFigure();
			}
		}

		private bool IsThinRing()
		{
			try
			{
				SqlGeometryBuilder b = new SqlGeometryBuilder();
				b.SetSrid(m_srid);
				b.BeginGeometry(OpenGisGeometryType.Polygon);
				PopulateFigure(b);
				b.EndGeometry();
				SqlGeometry poly = b.ConstructedGeometry.MakeValid();
				return poly.STArea().Value < m_tolerance * poly.STLength().Value;
			}
			catch (ArgumentException) { }
			catch (FormatException) { }
			return true;
		}

		private void PopulateFigure(IGeometrySink sink)
		{
			m_figure[0].BeginFigure(sink);
			for (int i = 1; i < m_figure.Count; i++)
				m_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}
}