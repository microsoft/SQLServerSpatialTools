//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that reverses the input geometry.
    /// </summary>
    internal class ReverseLinearGeometrySink : IGeometrySink110
    {
        private SqlGeometryBuilder _target;
        private LRSMultiLine _lines;
        private LRSLine _currentLine;
        private bool _isMultiLine;
        private int _lineCounter;
        private int _srid;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and reverse it accordingly.
        /// </summary>
        /// <param name="target"></param>
        public ReverseLinearGeometrySink(SqlGeometryBuilder target)
        {
            _target = target;
            _isMultiLine = false;
            _lineCounter = 0;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            _lines = new LRSMultiLine(srid);
            _srid = srid;
        }

        // Check for types and begin geometry accordingly.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            // check if the type is of the supported types
            if (type == OpenGisGeometryType.MultiLineString)
                _isMultiLine = true;

            if (type == OpenGisGeometryType.LineString)
                _lineCounter++;
        }

        // Just add the points to the current line segment.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _currentLine = new LRSLine(_srid);
            _currentLine.AddPoint(x, y, z, m);
        }

        // Just add the points to the current line segment.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _currentLine.AddPoint(x, y, z, m);
        }

        // Reverse the points at the end of figure.
        public void EndFigure()
        {
            _currentLine.ReversePoints();
        }

        // This is where real work is done.
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!_isMultiLine)
                _lines.AddLine(_currentLine);

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (_lineCounter == 0 || !_isMultiLine)
            {
                // reverse the line before constructing the geometry
                _lines.ReversLines();
                _lines.ToSqlGeometry(ref _target);
            }
            else
            {
                _lines.AddLine(_currentLine);
                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                _lineCounter--;
            }
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }
    }
}
