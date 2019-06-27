//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using System;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that resets M value to null.
    /// </summary>
    internal class ResetMGeometrySink : IGeometrySink110
    {
        private readonly SqlGeometryBuilder _target;    // Where we place our result.

        public ResetMGeometrySink(SqlGeometryBuilder target)
        {
            _target = target;
        }

        /// <summary>
        /// Save the SRID for later 
        /// </summary>
        /// <param name="srid">Spatial Reference Identifier</param>
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        /// <summary>
        /// Start the geometry
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            _target.BeginGeometry(type);
        }

        /// <summary>
        /// Start the figure.  
        /// Note that since we only operate on LineStrings, Multilinestring and Point these should only be executed.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _target.BeginFigure(x, y, z, null);
        }

        public void AddLine(double x, double y, double? z, double? m)
        {
            _target.AddLine(x, y, z, null);
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
        }
    }
}