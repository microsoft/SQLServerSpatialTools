//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that multiplies the measure values of input geometry.
    /// </summary>
    internal class MultiplyMeasureGeometrySink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;
        private readonly double _scaleMeasure;

        /// <summary>
        /// Loop through each geometry types LINESTRING and MULTILINESTRING and scales the measure by given magnitude.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scaleMeasure"></param>
        public MultiplyMeasureGeometrySink(SqlGeometryBuilder target, double scaleMeasure)
        {
            _target = target;
            _scaleMeasure = scaleMeasure;
        }

        // Just pass through target.
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        // Just pass through target.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            _target.BeginGeometry(type);
        }

        // Just add the points with updated measure.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
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

        private double? GetUpdatedMeasure(double? m)
        {
            double? measure = null;
            if (m.HasValue)
                measure = (double)m * _scaleMeasure;
            return measure;
        }
    }
}
