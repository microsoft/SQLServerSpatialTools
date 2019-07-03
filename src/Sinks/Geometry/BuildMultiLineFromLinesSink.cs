//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that builds Multiline from Line String
    /// </summary>
    internal class BuildMultiLineFromLinesSink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;
        private bool _isFirstPoint;
        private int _linesCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildMultiLineFromLinesSink"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="linesCount">The lines count.</param>
        public BuildMultiLineFromLinesSink(SqlGeometryBuilder target, int linesCount)
        {
            _target = target;
            _linesCount = linesCount;
            _isFirstPoint = true;
        }

        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.Point)
                return;

            if (type != OpenGisGeometryType.LineString)
                SpatialExtensions.ThrowException(ErrorMessage.LineStringCompatible);

            if (_isFirstPoint)
            {
                _isFirstPoint = false;
                _target.BeginGeometry(OpenGisGeometryType.MultiLineString);
            }
            _target.BeginGeometry(type);
            _linesCount--;
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _target.BeginFigure(x, y, z, m);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            _target.AddLine(x, y, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void EndFigure()
        {
            _target.EndFigure();
        }

        public void EndGeometry()
        {
            _target.EndGeometry();

            // end of multi line
            if (_linesCount == 0)
                _target.EndGeometry();
        }
    }
}
