//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that populate measures for each point in a geometry .
    /// </summary>
    internal class PopulateGeometryMeasuresSink : IGeometrySink110
    {
        private SqlGeometry _lastPoint;
        private SqlGeometry _thisPoint;

        private LRSMultiLine _lines;
        private LRSLine _currentLine;

        private readonly double _startMeasure;
        private readonly double _endMeasure;
        private readonly double _totalLength;

        private bool _isMultiLine;
        private int _lineCounter;
        private int _srid;                     // The _srid we are working in.
        private double _currentLength;
        private double _currentPointM;
        private SqlGeometry _target;    // Where we place our result.

        /// <summary>
        /// Gets the constructed geometry.
        /// </summary>
        /// <returns></returns>
        public SqlGeometry GetConstructedGeom()
        {
            return _target;
        }

        // We target another builder, to which we will send a point representing the point we find.
        // We also take a distance, which is the point along the input linestring we will travel.
        // Note that we only operate on LineString instances: anything else will throw an exception.
        public PopulateGeometryMeasuresSink(double startMeasure, double endMeasure, double length)
        {
            _startMeasure = startMeasure;
            _endMeasure = endMeasure;
            _totalLength = length;
            _isMultiLine = false;
            _lineCounter = 0;
            _currentPointM = startMeasure;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            _lines = new LRSMultiLine(srid);
            _srid = srid;
        }

        // Start geometry and check if the type is of the supported types
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
                _isMultiLine = true;
            else if (type == OpenGisGeometryType.LineString)
                _lineCounter++;
        }


        // This operates on LineStrings, multi linestring
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _currentLine = new LRSLine(_srid);
            _currentLine.AddPoint(x, y, null, _currentPointM);

            // Memorize the starting point.
            _lastPoint = SqlGeometry.Point(x, y, _srid);
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _thisPoint = SqlGeometry.Point(x, y, _srid);
            _currentLength += _lastPoint.STDistance(_thisPoint).Value;
            _currentLine.AddPoint(x, y, null, GetCurrentMeasure());

            // reset the last point with the current point.
            _lastPoint = _thisPoint;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
            
        }

        // When we end, we'll make all of our output calls to our target.
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!_isMultiLine)
                _lines.AddLine(_currentLine);

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (_lineCounter == 0 || !_isMultiLine)
            {
                _target = _lines.ToSqlGeometry();
            }
            else
            {
                _lines.AddLine(_currentLine);
                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                _lineCounter--;
            }
        }

        private double GetCurrentMeasure()
        {
            _currentPointM = _startMeasure + (_currentLength / _totalLength) * (_endMeasure - _startMeasure);
            return _currentPointM;
        }
    }
}
