//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that checks whether the geometry collection is of supported types.
    /// </summary>
    internal class GISTypeCheckGeometrySink : IGeometrySink110
    {
        private bool _isSupportedType;
        private readonly OpenGisGeometryType[] _supportedTypes;

        public GISTypeCheckGeometrySink(OpenGisGeometryType[] supportedTypes)
        {
            _isSupportedType = true;
            _supportedTypes = supportedTypes;
        }

        /// <summary>
        /// Returns true if input type is of specified supported types list.
        /// </summary>
        /// <returns></returns>
        public bool IsCompatible()
        {
            return _isSupportedType;
        }

        // This is a NOP.
        public void SetSrid(int srid)
        {
        }

        /// <summary>
        /// Loop through each geometry types in geom collection and validate if the type is either POINT, LINESTRING, MULTILINESTRING
        /// </summary>
        /// <param name="type">Geometry Type</param>
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.GeometryCollection)
                return;

            // check if the type is of the supported types
            if (_isSupportedType && !(type.Contains(_supportedTypes)))
            {
                _isSupportedType = false;
            }
        }

        // This is a NOP.
        public void BeginFigure(double x, double y, double? z, double? m)
        {

        }

        // This is a NOP.
        public void AddLine(double x, double y, double? z, double? m)
        {

        }

        // This is a NOP.
        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // This is a NOP.
        public void EndGeometry()
        {

        }
    }
}
