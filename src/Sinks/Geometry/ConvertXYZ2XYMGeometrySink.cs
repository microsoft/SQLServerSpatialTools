//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// Converts Z co-ordinate as measure.
    /// </summary>
    internal class ConvertXYZ2XYMGeometrySink : SqlGeometryBuilder
    {
        public override void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            if (m1.HasValue || m2.HasValue)
                throw new ArgumentException(ErrorMessage.WKT3DOnly);

            base.AddCircularArc(x1, y1, null, z1, x2, y2, null, z2);
        }

        public override void AddLine(double x, double y, double? z, double? m)
        {
            if (m.HasValue)
                throw new ArgumentException(ErrorMessage.WKT3DOnly);

            base.AddLine(x, y, null, z);
        }

        public override void BeginFigure(double x, double y, double? z, double? m)
        {
            if (m.HasValue)
                throw new ArgumentException(ErrorMessage.WKT3DOnly);

            base.BeginFigure(x, y, null, z);
        }
    }
}