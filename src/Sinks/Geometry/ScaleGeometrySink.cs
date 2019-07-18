//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that scales the measure values of input geometry.
    /// </summary>
    internal class ScaleGeometrySink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;
        private readonly double _startMeasure, _endMeasure, _newStartMeasure, _newEndMeasure, _shiftMeasure;
        private bool _isPoint;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and translates its measure.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="startMeasure"></param>
        /// <param name="endMeasure"></param>
        /// <param name="newStartMeasure"></param>
        /// <param name="newEndMeasure"></param>
        /// <param name="shiftMeasure"></param>
        public ScaleGeometrySink(SqlGeometryBuilder target, double startMeasure, double endMeasure, double newStartMeasure, double newEndMeasure, double shiftMeasure)
        {
            _target = target;
            _startMeasure = startMeasure;
            _endMeasure = endMeasure;
            _newStartMeasure = newStartMeasure;
            _newEndMeasure = newEndMeasure;
            _shiftMeasure = shiftMeasure;
        }

        // Just pass through target.
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        // Just pass through target.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.Point)
                _isPoint = true;

            _target.BeginGeometry(type);
        }

        // Just add the points with updated measure.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (_isPoint)
                _target.BeginFigure(x, y, z, _newEndMeasure + _shiftMeasure);
            else
                _target.BeginFigure(x, y, z, GetUpdatedMeasure(m));
        }

        // Just add the points with updated measure.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _target.AddLine(x, y, z, GetUpdatedMeasure(m));
        }

        // Just pass through target.
        public void EndFigure()
        {
            _target.EndFigure();
        }

        // Just pass through target.
        public void EndGeometry()
        {
            _target.EndGeometry();
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        private double GetUpdatedMeasure(double? m)
        {
            var mValue = m ?? 0.0;
            return ((mValue - _startMeasure) * ((_newEndMeasure - _newStartMeasure) / (_endMeasure - _startMeasure))) +
                   _newStartMeasure + _shiftMeasure;
        }
    }
}
