using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    class ConvertXVZ2XYM : SqlGeometryBuilder
    {

        public override void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            if (m1.HasValue || m2.HasValue)
                throw new ArgumentException("Input WKT should only have three dimensions!");

            base.AddCircularArc(x1, y1, null, z1, x2, y2, null, z2);
        }

        public override void AddLine(double x, double y, double? z, double? m)
        {
            if (m.HasValue)
                throw new ArgumentException("Input WKT should only have three dimensions!");

            base.AddLine(x, y, null, z);
        }

        public override void BeginFigure(double x, double y, double? z, double? m)
        {
            if (m.HasValue)
                throw new ArgumentException("Input WKT should only have three dimensions!");

            base.BeginFigure(x, y, null, z);
        }

        public override void BeginGeometry(OpenGisGeometryType @type)
        {
            base.BeginGeometry(@type);
        }

        public override void EndFigure()
        {
            base.EndFigure();
        }

        public override void EndGeometry()
        {
            base.EndGeometry();
        }

        public override void SetSrid(int srid)
        {
            base.SetSrid(srid);
        }
    }
}