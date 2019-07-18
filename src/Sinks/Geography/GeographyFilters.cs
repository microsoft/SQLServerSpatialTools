//------------------------------------------------------------------------------
// Copyright (c) 2010 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	public class GeographyEmptyShapeFilter : IGeographySink110
	{
		private IGeographySink110 m_sink;
		private Queue<OpenGisGeographyType> m_types = new Queue<OpenGisGeographyType>();
		private bool m_root = true;

		public GeographyEmptyShapeFilter(IGeographySink110 sink)
		{
			m_sink = sink;
		}

		public void SetSrid(int srid)
		{
			m_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (m_root)
			{
				m_root = false;
				m_sink.BeginGeography(type);
			}
			else
			{
				m_types.Enqueue(type);
			}
		}

		public void EndGeography()
		{
			if (m_types.Count > 0)
				m_types.Dequeue();
			else
				m_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			while (m_types.Count > 0)
				m_sink.BeginGeography(m_types.Dequeue());
			m_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			m_sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			m_sink.EndFigure();
		}
	}

	public class GeographyPointFilter : IGeographySink110
	{
		private IGeographySink110 m_sink;
		private int m_depth;
		private bool m_root = true;

		public GeographyPointFilter(IGeographySink110 sink)
		{
			m_sink = sink;
		}

		public void SetSrid(int srid)
		{
			m_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (type == OpenGisGeographyType.Point || type == OpenGisGeographyType.MultiPoint)
			{
				if (m_root)
				{
					m_root = false;
					m_sink.BeginGeography(OpenGisGeographyType.GeometryCollection);
					m_sink.EndGeography();
				}
				m_depth++;
			}
			else
			{
				m_root = false;
				m_sink.BeginGeography(type);
			}
		}

		public void EndGeography()
		{
			if (m_depth > 0)
				m_depth--;
			else
				m_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (m_depth == 0)
				m_sink.BeginFigure(latitude, longitude, z, m);
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (m_depth == 0)
				m_sink.AddLine(latitude, longitude, z, m);
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (m_depth == 0)
				m_sink.EndFigure();
		}
	}

	public class GeographyShortLineStringFilter : IGeographySink110
	{
		private IGeographySink110 m_sink;
		private double m_tolerance;
		private int m_srid;
		private bool m_insideLineString;
		private List<Vertex> m_figure = new List<Vertex>();

		public GeographyShortLineStringFilter(IGeographySink110 sink, double tolerance)
		{
			m_sink = sink;
			m_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			m_srid = srid;
			m_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			m_sink.BeginGeography(type);
			m_insideLineString = type == OpenGisGeographyType.LineString;
		}

		public void EndGeography()
		{
			m_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (m_insideLineString)
			{
				m_figure.Clear();
				m_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				m_sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (m_insideLineString)
			{
				m_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				m_sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
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
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(m_srid);
				b.BeginGeography(OpenGisGeographyType.LineString);
				PopulateFigure(b);
				b.EndGeography();
				SqlGeography g = b.ConstructedGeography;
				return g.STLength().Value < m_tolerance;
			}
			catch (FormatException) { }
			catch (ArgumentException) { }
			return false;
		}

		private void PopulateFigure(IGeographySink110 sink)
		{
			m_figure[0].BeginFigure(sink);
			for (int i = 1; i < m_figure.Count; i++)
				m_figure[i].AddLine(sink);
			sink.EndFigure();
		}
	}

	public class GeographyThinRingFilter : IGeographySink110
	{
		private IGeographySink110 m_sink;
		private double m_tolerance;
		private bool m_insidePolygon;
		private int m_srid;
		private List<Vertex> m_figure = new List<Vertex>();

		public GeographyThinRingFilter(IGeographySink110 sink, double tolerance)
		{
			m_sink = sink;
			m_tolerance = tolerance;
		}

		public void SetSrid(int srid)
		{
			m_srid = srid;
			m_sink.SetSrid(srid);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			m_sink.BeginGeography(type);
			m_insidePolygon = type == OpenGisGeographyType.Polygon;
		}

		public void EndGeography()
		{
			m_sink.EndGeography();
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (m_insidePolygon)
			{
				m_figure.Clear();
				m_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				m_sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (m_insidePolygon)
			{
				m_figure.Add(new Vertex(latitude, longitude, z, m));
			}
			else
			{
				m_sink.AddLine(latitude, longitude, z, m);
			}
		}

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
		{
			if (m_insidePolygon)
			{
				if (!IsThinRing())
				{
					PopulateFigure(m_sink, false);
				}
			}
			else
			{
				m_sink.EndFigure();
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
			return poly.STArea().Value < m_tolerance * poly.STLength().Value;
		}

		private SqlGeography RingToPolygon(bool reverse)
		{
			try
			{
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(m_srid);
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
				m_figure[m_figure.Count - 1].BeginFigure(sink);
				for (int i = m_figure.Count - 2; i >= 0; i--)
					m_figure[i].AddLine(sink);
			}
			else
			{
				m_figure[0].BeginFigure(sink);
				for (int i = 1; i < m_figure.Count; i++)
					m_figure[i].AddLine(sink);
			}
			sink.EndFigure();
		}
	}
}