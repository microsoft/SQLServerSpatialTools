//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.Types
{
    /// <summary>
    /// Data structure to capture MULTILINESTRING geometry type.
    /// </summary>
    internal class LRSMultiLine : IEnumerable
    {
        private readonly List<LRSLine> _lines;
        internal readonly int SRID;
        private string _wkt;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRSMultiLine"/> class.
        /// </summary>
        /// <param name="srid">The srid.</param>
        internal LRSMultiLine(int srid)
        {
            SRID = srid;
            _lines = new List<LRSLine>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsEmpty => !_lines.Any() || _lines.Count == 0;

        /// <summary>
        /// Gets the number of line segments in the MULTILINESTRING.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        internal int Count => _lines.Any() ? _lines.Count : 0;

        /// <summary>
        /// Gets a value indicating whether this instance is MULTILINESTRING.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a multi line; otherwise, <c>false</c>.
        /// </value>
        internal bool IsMultiLine => Count > 1;

        /// <summary>
        /// Gets a value indicating whether this instance is LINESTRING.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a line string; otherwise, <c>false</c>.
        /// </value>
        internal bool IsLine => Count == 1;

        /// <summary>
        /// Gets a value indicating whether this instance is a 2 POINT LINESTRING.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is a 2 point line string; otherwise, <c>false</c>.
        /// </value>
        internal bool Is2PointLine => IsLine && GetFirstLine().Count == 2;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        internal double Length { get { return _lines.Any() ? _lines.Sum(line => line.Length) : 0; } }

        #region Add Lines

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="line">The LRS line.</param>
        internal void AddLine(LRSLine line)
        {
            if (line != null && line.HasPoints && line.IsLine)
                _lines.Add(line);
        }

        /// <summary>
        /// Adds multiple lines.
        /// </summary>
        /// <param name="lineList">The line list.</param>
        internal void AddLines(List<LRSLine> lineList)
        {
            if (lineList != null && lineList.Any())
                lineList.ForEach(AddLine);
        }

        /// <summary>
        /// Adds LRS Multi lines.
        /// </summary>
        /// <param name="lrsMultiLine">The line list.</param>
        internal void Add(LRSMultiLine lrsMultiLine)
        {
            if (lrsMultiLine?._lines != null && lrsMultiLine._lines.Any())
                AddLines(lrsMultiLine._lines);
        }

        #endregion

        #region Line Manipulation

        /// <summary>
        /// Scale the existing measure of geometry by multiplying existing measure with offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void ScaleMeasure(double offsetMeasure)
        {
            _lines.ForEach(line => line.ScaleMeasure(offsetMeasure));
        }

        /// <summary>
        /// Sum the existing measure with the offsetMeasure
        /// </summary>
        /// <param name="offsetMeasure"></param>
        internal void TranslateMeasure(double offsetMeasure)
        {
            _lines.ForEach(line => line.TranslateMeasure(offsetMeasure));
        }

        /// <summary>
        /// Reverses only the LINESTRING segments order in a MULTILINESTRING
        /// </summary>
        internal void ReversLines()
        {
            _lines.Reverse();
        }

        /// <summary>
        /// Removes the collinear points.
        /// </summary>
        internal void RemoveCollinearPoints()
        {
            // First calculate the slope to remove collinear points.
            CalculateSlope();
            _lines.ForEach(line => line.RemoveCollinearPoints());
        }

        /// <summary>
        /// Calculates the slope.
        /// </summary>
        internal void CalculateSlope()
        {
            _lines.ForEach(line => line.CalculateSlope());
        }

        /// <summary>
        /// Computes the offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        internal LRSMultiLine ComputeOffset(double offset, double tolerance)
        {
            var parallelMultiLine = new LRSMultiLine(SRID);
            _lines.ForEach(line => parallelMultiLine._lines.Add(line.ComputeParallelLine(offset, tolerance)));
            return parallelMultiLine;
        }

        /// <summary>
        /// Populates the measures.
        /// </summary>
        /// <param name="startM">The start m.</param>
        /// <param name="endM">The end m.</param>
        internal void PopulateMeasures(double? startM, double? endM)
        {
            var startMeasure = startM ?? 0;
            var endMeasure = endM ?? Length;
            double currentLength = 0;

            _lines.ForEach(line =>
            {
                line.PopulateMeasures(Length, ref currentLength, startMeasure, endMeasure);
            });

        }

        /// <summary>
        /// Reverse both LINESTRING segment and its POINTS
        /// </summary>
        internal void ReverseLinesAndPoints()
        {
            ReversLines();
            _lines.ForEach(line => line.ReversePoints());
        }

        /// <summary>
        /// Gets the first LINESTRING in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSLine GetFirstLine()
        {
            return _lines.Any() ? _lines.First() : null;
        }

        /// <summary>
        /// Gets the last LINESTRING in a MULTILINESTRING.
        /// </summary>
        /// <returns></returns>
        internal LRSLine GetLastLine()
        {
            return _lines.Any() ? _lines.Last() : null;
        }

        /// <summary>
        /// Gets the start POINT in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetStartPoint()
        {
            return _lines.Any() ? _lines.First().GetStartPoint() : null;
        }

        /// <summary>
        /// Gets the start POINT measure in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal double? GetStartPointM()
        {
            return GetStartPoint()?.M;
        }

        /// <summary>
        /// Gets the end POINT in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetEndPoint()
        {
            return _lines.Any() ? _lines.Last().GetEndPoint() : null;
        }

        /// <summary>
        /// Gets the end POINT measure in a MULTILINESTRING
        /// </summary>
        /// <returns></returns>
        internal double? GetEndPointM()
        {
            return GetEndPoint()?.M;
        }

        /// <summary>
        /// Gets the POINT in a MULTILINESTRING at a specified measure
        /// </summary>
        /// <returns></returns>
        internal LRSPoint GetPointAtM(double measure)
        {
            if (!_lines.Any()) return null;

            foreach (var line in _lines)
            {
                var point = line.GetPointAtM(measure);
                if (point != null)
                    return point;
            }
            return null;
        }

        /// <summary>
        /// Removes the first.
        /// </summary>
        /// <returns></returns>
        internal void RemoveFirst()
        {
            if (!_lines.Any()) return;
            _lines.RemoveAt(0);
        }

        /// <summary>
        /// Removes the last.
        /// </summary>
        /// <returns></returns>
        internal void RemoveLast()
        {
            if (!_lines.Any()) return;
            _lines.RemoveAt(_lines.Count - 1);
        }

        #endregion

        #region Data Structure Conversion

        /// <summary>
        /// Converts to WKT format.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsEmpty)
            {
                _wkt = string.Empty;
                return "MULTILINESTRING EMPTY";
            }

            if(IsLine)
                return GetFirstLine().ToString();

            var wktBuilder = new StringBuilder();
            if (IsMultiLine)
                wktBuilder.Append("MULTILINESTRING (");

            var lineIterator = 1;

            foreach (var line in _lines)
            {
                if (line.IsLine)
                    wktBuilder.Append(line.ToString().Replace("LINESTRING ", string.Empty));

                if (lineIterator != _lines.Count)
                    wktBuilder.Append(", ");
                lineIterator++;
            }

            if (IsMultiLine)
                wktBuilder.Append(")");

            _wkt = wktBuilder.ToString();

            return _wkt;
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <returns>SqlGeometry</returns>
        internal SqlGeometry ToSqlGeometry()
        {
            var geomBuilder = new SqlGeometryBuilder();
            return ToSqlGeometry(ref geomBuilder);
        }

        /// <summary>
        /// Method returns the SqlGeometry form of the MULTILINESTRING
        /// </summary>
        /// <param name="geomBuilder">Reference SqlGeometryBuilder to be used for building Geometry.</param>
        /// <returns>SqlGeometry</returns>
        internal SqlGeometry ToSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            if (IsEmpty)
                return SqlGeometry.Null;

            return BuildSqlGeometry(ref geomBuilder) ? geomBuilder.ConstructedGeometry : null;
        }

        /// <summary>
        /// Method builds the SqlGeometry form of the MULTILINESTRING through reference GeometryBuilder.
        /// </summary>
        /// <param name="geomBuilder">Reference SqlGeometryBuilder to be used for building Geometry.</param>
        internal bool BuildSqlGeometry(ref SqlGeometryBuilder geomBuilder)
        {
            var isBuildDone = false;
            if (IsEmpty)
                return false;

            if (geomBuilder == null)
                geomBuilder = new SqlGeometryBuilder();

            if (IsMultiLine)
            {
                geomBuilder.SetSrid(SRID);
                geomBuilder.BeginGeometry(OpenGisGeometryType.MultiLineString);
                isBuildDone = true;
            }

            // ignore points
            foreach (var line in _lines.Where(line => line.IsLine).ToList())
            {
                line.BuildSqlGeometry(ref geomBuilder, IsMultiLine);
                isBuildDone = true;
            }

            if (IsMultiLine)
                geomBuilder.EndGeometry();

            return isBuildDone;
        }


        #endregion

        #region Enumeration

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public LRSEnumerator<LRSLine> GetEnumerator()
        {
            return new LRSEnumerator<LRSLine>(_lines);
        }

        /// <summary>
        /// Returns an enumerator that iterates through each LINESTRING in a MULTILINESTRING.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}