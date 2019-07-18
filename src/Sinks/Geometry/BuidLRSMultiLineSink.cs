// Copyright (c) Microsoft Corporation.  All rights reserved.

using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using System;
using System.Text;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class implements a geometry sink that builds LRS multiline.
    /// Second segment measure is updated with offset difference.
    /// </summary>
    class BuildLRSMultiLineSink : IGeometrySink110
    {
        private LRSLine CurrentLine;
        public LRSMultiLine MultiLine;
        private int Srid;

        public  void ScaleMeasure(double offsetMeasure)
        {
            MultiLine.ScaleMeasure(offsetMeasure);
        }
        public void TranslateMeasure(double offsetMeasure)
        {
            MultiLine.TranslateMeasure(offsetMeasure);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineMergeGeometrySink"/> class.
        /// </summary>
        public BuildLRSMultiLineSink()
        {
            MultiLine = new LRSMultiLine();
        }

        public void SetSrid(int srid)
        {
            Srid = srid;
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.LineString)
            {
                CurrentLine = new LRSLine();
                CurrentLine.SetSrid(Srid);
            }
        }

        // Start the figure.  
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            CurrentLine.AddPoint(new LRSPoint(x, y, z, m));
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            CurrentLine.AddPoint(new LRSPoint(x, y, z, m));
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NO-OP
        public void EndFigure()
        {
            MultiLine.AddLine(CurrentLine);
        }

        // This is a NO-OP
        public void EndGeometry()
        {
        }
        public SqlGeometry ToSqlGeometry()
        {
            return MultiLine.ToSqlGeometry();
        }
    }
}
