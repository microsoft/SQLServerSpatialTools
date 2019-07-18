//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Types;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Sinks.Geography
{
    /// <summary>
    /// This class is used for generating a new geography object where additional points are inserted
    /// along every line in such a way that the angle between two consecutive points does not
    /// exceed a prescribed angle.  The points are generated between the unit vectors that correspond
    /// to the line's start and end along the great-circle arc on the unit sphere .  This follows the
    /// definition of geodetic lines in SQL Server.
    /// </summary>
    public class DensifyGeographySink : IGeographySink110
    {
        // Minimum angle. If the user specifies a smaller angle, the angle will be set to this minimum.
        private const double MinAngle = 0.000001;

        // Maximum angular difference in degrees between two consecutive points in the "densified" line.
        private readonly double _angle;

        // Previous point added.
        private Vector3 _startPoint;

        private readonly IGeographySink110 _sink;

        // Constructor.
		public DensifyGeographySink(IGeographySink110 sink, double angle)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink), "sink");

            _angle = angle < MinAngle ? MinAngle : angle;
        }

        #region IGeographySink Members

        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // Transforming from geodetic coordinates to a unit vector.
			var endPoint = SpatialUtil.SphericalDegToCartesian(latitude, longitude);

            var angle = endPoint.Angle(_startPoint);
            if (angle > MinAngle)
            {
                // _startPoint and endPoint are the unit vectors that correspond to the input
                // start and end points.  In their 3D space we operate in a local coordinate system
                // where _startPoint is the x axis and the xy plane contains endPoint. Every
                // point is now generated from the previous one by a fixed rotation in the local
                // xy plane, and converted back to geodetic coordinates.

                // Construct the local z and y axes.
                var zAxis = (_startPoint + endPoint).CrossProduct(_startPoint - endPoint).Unitize();
                var yAxis = (_startPoint).CrossProduct(zAxis);

                // Calculating how many points we need.
                var count = Convert.ToInt32(Math.Ceiling(angle / SpatialUtil.ToRadians(_angle)));

                // Scaling the angle so that points are equally placed.
                var exactAngle = angle / count;

                var cosine = Math.Cos(exactAngle);
                var sine = Math.Sin(exactAngle);

                // Setting the first x and y points in our local coordinate system.
                var x = cosine;
                var y = sine;

                for (var i = 0; i < count - 1; i++)
                {
                    var newPoint = (_startPoint * x + yAxis * y).Unitize();

                    // Adding the point.
                    _sink.AddLine(SpatialUtil.LatitudeDeg(newPoint), SpatialUtil.LongitudeDeg(newPoint), null, null);

                    // Rotating to get next point.
                    var r = x * cosine - y * sine;
                    y = x * sine + y * cosine;
                    x = r;
                }
            }
            _sink.AddLine(latitude, longitude, z, m);

            // Remembering last point we added.
            _startPoint = endPoint;
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Starting the figure, remembering the vector that corresponds to the first point.
			_startPoint = SpatialUtil.SphericalDegToCartesian(latitude, longitude);
            _sink.BeginFigure(latitude, longitude, z, m);
        }

        public void BeginGeography(OpenGisGeographyType type)
        {
            _sink.BeginGeography(type);
        }

        public void EndFigure()
        {
            _sink.EndFigure();
        }

        public void EndGeography()
        {
            _sink.EndGeography();
        }

        public void SetSrid(int srid)
        {
            _sink.SetSrid(srid);
        }

        #endregion
    }   
}