// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /**
     * This class implements a geometry sink that will shift an input geometry by a given amount in the x and
     * y directions.  It directs its output to another sink, and can therefore be used in a pipeline if desired.
     */
    public class ShiftGeometrySink : IGeometrySink110
    {
        private readonly IGeometrySink110 _target;  // the target sink
		private readonly double _xShift;         // How much to shift in the x direction.
		private readonly double _yShift;         // How much to shift in the y direction.

        // We take an amount to shift in the x and y directions, as well as a target sink, to which
        // we will pipe our result.
        public ShiftGeometrySink(double xShift, double yShift, IGeometrySink110 target)
        {
            _target = target;
            _xShift = xShift;
            _yShift = yShift;
        }

        // Just pass through without change.
        public void SetSrid(int srid)
        {
            _target.SetSrid(srid);
        }

        // Just pass through without change.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            _target.BeginGeometry(type);
        }

        // Each BeginFigure call will just move the start point by the required amount.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _target.BeginFigure(x + _xShift, y + _yShift, z, m);
        }

        // Each AddLine call will just move the endpoint by the required amount.
        public void AddLine(double x, double y, double? z, double? m)
        {
            _target.AddLine(x + _xShift, y + _yShift, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new System.Exception("AddCircularArc is not implemented yet in this class");
        }

        // Just pass through without change.
        public void EndFigure()
        {
            _target.EndFigure();
        }

        // Just pass through without change.
        public void EndGeometry()
        {
            _target.EndGeometry();
        }
    }
}
