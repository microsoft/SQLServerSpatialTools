// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /// <summary>
    /// This class is used for generating a new geography object where additional points are inserted
    /// along every line in such a way that the angle between two consecutive points does not
    /// exceed a prescribed angle.  The points are generated between the unit vectors that correspond
    /// to the line's start and end along the great-circle arc on the unit sphere .  This follows the
    /// definition of geodetic lines in SQL Server.
    /// </summary>
    public class DensifyGeographySink : IGeographySink
    {
        // Minimum angle. If the user specifies a smaller angle, the angle will be set to this minimum.
        public static readonly double MinAngle = 0.000001;

        // Maximum angular difference in degrees between two consecutive points in the "densified" line.
        private readonly double _angle;

        // Previous point added.
        private Vector3 _startPoint;

        private readonly IGeographySink _sink;

        // Constructor.
		public DensifyGeographySink(IGeographySink sink, double angle)
        {
            if (sink == null)
                throw new ArgumentNullException("sink");
            _sink = sink;

            if (angle < MinAngle)
                _angle = MinAngle;
            else
                 _angle = angle;
        }

        #region IGeographySink Members

        public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            // Transforming from geodetic coordinates to a unit vector.
			Vector3 endPoint = Util.SphericalDegToCartesian(latitude, longitude);

            double angle = endPoint.Angle(_startPoint);
            if (angle > MinAngle)
            {
                // _startPoint and endPoint are the unit vectors that correspond to the input
                // start and end points.  In their 3D space we operate in a local coordinate system
                // where _startPoint is the x axis and the xy plane contains endPoint. Every
                // point is now generated from the previous one by a fixed rotation in the local
                // xy plane, and converted back to geodetic coordinates.

                // Construct the local z and y axes.
                Vector3 zAxis = (_startPoint + endPoint).CrossProduct(_startPoint - endPoint).Unitize();
                Vector3 yAxis = (_startPoint).CrossProduct(zAxis);

                // Calculating how many points we need.
                int count = Convert.ToInt32(Math.Ceiling(angle / Util.ToRadians(_angle)));

                // Scaling the angle so that points are equaly placed.
                double exactAngle = angle / count;

                double cosine = Math.Cos(exactAngle);
                double sine = Math.Sin(exactAngle);

                // Setting the first x and y points in our local coordinate system.
                double x = cosine;
                double y = sine;

                for (int i = 0; i < count - 1; i++)
                {
                    Vector3 newPoint = (_startPoint * x + yAxis * y).Unitize();

                    // Adding the point.
                    _sink.AddLine(Util.LatitudeDeg(newPoint), Util.LongitudeDeg(newPoint), null, null);

                    // Rotating to get next point.
                    double r = x * cosine - y * sine;
                    y = x * sine + y * cosine;
                    x = r;
                }
            }
            _sink.AddLine(latitude, longitude, z, m);

            // Remembering last point we added.
            _startPoint = endPoint;
        }

        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            // Starting the figure, remembering the vector that corresponds to the first point.
			_startPoint = Util.SphericalDegToCartesian(latitude, longitude);
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