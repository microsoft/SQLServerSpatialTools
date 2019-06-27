//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that builds LRS multiline.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    internal class BuildLRSMultiLineSink : IGeometrySink110
    {
        private int _srid;
        private readonly bool _doUpdateM;
        private readonly double _offsetM;
        private LRSLine _currentLine;
        public LRSMultiLine MultiLine;

        public BuildLRSMultiLineSink(bool doUpdateM, double? offsetM)
        {
            _doUpdateM = doUpdateM;
            _offsetM = offsetM ?? 0;
        }

        // Initialize MultiLine and sets srid.
        public void SetSrid(int srid)
        {
            MultiLine = new LRSMultiLine(srid);
            _srid = srid;
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.LineString)
                _currentLine = new LRSLine(_srid);
        }

        // Just add the points to the current line.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            var currentM = _doUpdateM ? m + _offsetM : m;
            _currentLine.AddPoint(x, y, z, currentM);
        }

        // Just add the points to the current line.
        public void AddLine(double x, double y, double? z, double? m)
        {
            var currentM = _doUpdateM ? m + _offsetM : m;
            _currentLine.AddPoint(x, y, z, currentM);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // Add the current line to the MULTILINESTRING collection
        public void EndFigure()
        {
            MultiLine.AddLine(_currentLine);
        }

        // This is a NO-OP
        public void EndGeometry()
        {
        }
    }
}
