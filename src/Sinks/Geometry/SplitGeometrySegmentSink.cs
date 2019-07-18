//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geometry
{
    /// <summary>
    /// This class implements a geometry sink that split the geometry LINESTRING and MULTILINESTRING into two segments based on split point.
    /// </summary>
    internal class SplitGeometrySegmentSink : IGeometrySink110
    {
        private readonly double _splitPointMeasure;
        private readonly SqlGeometry _splitPoint;

        public SqlGeometry Segment1;    // Where we place our result.
        public SqlGeometry Segment2;    // Where we place our result.

        private LRSMultiLine _segment1;
        private LRSMultiLine _segment2;
        private LRSLine _currentLineForSegment1;
        private LRSLine _currentLineForSegment2;

        private int _srid, _lineCounter;
        private bool _isMultiLine;
        private bool _splitPointReached;
        private double _lastM;

        // Initialize Split Geom Sink with split point
        public SplitGeometrySegmentSink(SqlGeometry splitPoint)
        {
            _splitPoint = splitPoint;
            _isMultiLine = false;
            _lineCounter = 0;
            _splitPointMeasure = splitPoint.HasM ? splitPoint.M.Value : 0;
        }

        // Save the SRID for later
        public void SetSrid(int srid)
        {
            _segment1 = new LRSMultiLine(srid);
            _segment2 = new LRSMultiLine(srid);
            _srid = srid;
        }

        private bool IsEqualToSplitMeasure(double? currentMeasure)
        {
            return currentMeasure.EqualsTo(_splitPoint.M.Value);
        }

        // Start the geometry.
        public void BeginGeometry(OpenGisGeometryType type)
        {
            if (type == OpenGisGeometryType.MultiLineString)
                _isMultiLine = true;
            if (type == OpenGisGeometryType.LineString)
                _lineCounter++;
        }

        // Start the figure.
        public void BeginFigure(double x, double y, double? z, double? m)
        {
            _currentLineForSegment1 = new LRSLine(_srid);
            _currentLineForSegment2 = new LRSLine(_srid);

            // just add it to the second segment once split point is reached
            if (m != null && (_splitPointReached && m.Value.NotEqualsTo(_splitPointMeasure)))
            {
                _currentLineForSegment2.AddPoint(x, y, z, m);
                return;
            }

            if (m < _splitPointMeasure)
                _currentLineForSegment1.AddPoint(x, y, null, m);
            else if (m > _splitPointMeasure || IsEqualToSplitMeasure(m))
            {
                _currentLineForSegment2.AddPoint(x, y, null, m);
                _splitPointReached = IsEqualToSplitMeasure(m);
            }

            if (m != null) _lastM = (double) m;
        }

        // This is where the real work is done.
        public void AddLine(double x, double y, double? z, double? m)
        {
            // just add it to the second segment once split point is reached
            if (m != null && (_splitPointReached && m.Value.NotEqualsTo(_splitPointMeasure)))
            {
                _currentLineForSegment2.AddPoint(x, y, z, m);
                return;
            }

            // If current measure is less than split measure; then add it to the first segment.
            if (m < _splitPointMeasure)
            {
                _currentLineForSegment1.AddPoint(x, y, z, m);
            }

            // split measure in between last point measure and current point measure.
            else if (_splitPointMeasure > _lastM && _splitPointMeasure < m)
            {
                _currentLineForSegment1.AddPoint(_splitPoint);
                _currentLineForSegment2.AddPoint(_splitPoint);
                _currentLineForSegment2.AddPoint(x, y, z, m);
                _splitPointReached = true;
            }

            // if current measure is equal to split measure; then it is a shape point
            else if (m != null && (IsEqualToSplitMeasure(m) && _lastM.NotEqualsTo(m.Value)))
            {
                _currentLineForSegment1.AddPoint(x, y, z, m);
                _currentLineForSegment2.AddPoint(x, y, z, m);
                _splitPointReached = true;
            }

            // If current measure is greater than split measure; then add it to the second segment.
            else if (m > _splitPointMeasure)
            {
                _currentLineForSegment2.AddPoint(x, y, z, m);
            }

            // reassign current measure to last measure
            if (m != null) _lastM = (double) m;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        // This is a NOP.
        public void EndFigure()
        {
        }

        // add segments to target
        public void EndGeometry()
        {
            // if not multi line then add the current line to the collection.
            if (!_isMultiLine)
            {
                _segment1.AddLine(_currentLineForSegment1);
                _segment2.AddLine(_currentLineForSegment2);
            }

            // if line counter is 0 then it is multiline
            // if 1 then it is linestring 
            if (_lineCounter == 0 || !_isMultiLine)
            {
                Segment1 = _segment1.ToSqlGeometry();

                // TODO:: Messy logic to be meet as per Oracle; need to be re-factored
                // if second is a multi line
                // end measure of first line > end measure of last line
                // then consider only first line 
                // by locating the point
                if (!_segment1.IsEmpty && _segment2.IsMultiLine)
                {
                    var endSegmentEndM = _segment2.GetLastLine().GetEndPointM();
                    var startSegmentStartM = _segment2.GetFirstLine().GetEndPointM();

                    if (startSegmentStartM > endSegmentEndM)
                    {
                        var trimmedLine = _segment2.GetFirstLine();
                        var newLRSLine = new LRSLine(_srid);

                        // add points up to end segment measure
                        foreach (var point in trimmedLine)
                        {
                            if (point.M < endSegmentEndM)
                                newLRSLine.AddPoint(point);
                        }

                        // add the end point
                        if (endSegmentEndM.EqualsTo(_splitPointMeasure))
                            newLRSLine.AddPoint(_splitPoint);
                        else
                            newLRSLine.AddPoint(trimmedLine.LocatePoint(endSegmentEndM, newLRSLine.GetEndPoint()));

                        Segment2 = newLRSLine.ToSqlGeometry();
                    }
                    // if end segment measure is equal to split measure; then return the split alone for second segment
                    else if (endSegmentEndM.EqualsTo(_splitPointMeasure))
                        Segment2 = _splitPoint;
                    else
                        Segment2 = _segment2.ToSqlGeometry();
                }
                else
                    Segment2 = _segment2.ToSqlGeometry();
            }
            else
            {
                if (_currentLineForSegment1.IsLine)
                    _segment1.AddLine(_currentLineForSegment1);

                if (_currentLineForSegment2.IsLine)
                    _segment2.AddLine(_currentLineForSegment2);

                // reset the line counter so that the child line strings chaining is done and return to base multiline type
                _lineCounter--;
            }
        }
    }
}
