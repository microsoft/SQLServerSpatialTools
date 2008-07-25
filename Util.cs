// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
    /**
     * This class contains functions that are meant to be used internally in this library.
     */
    class Util
    {

        // Convert a SqlGeography to an X,Y,Z vector.
        public static double[] GeographicToCartesian(SqlGeography point)
        {
            double[] rval = new double[3];
            double lat = point.Lat.Value * (Math.PI / 180);
            double lon = point.Long.Value * (Math.PI / 180);
            rval[0] = Math.Cos(lat) * Math.Cos(lon);
            rval[1] = Math.Cos(lat) * Math.Sin(lon);
            rval[2] = Math.Sin(lat);
            return rval;
        }

        // Convert an X,Y,Z vector to a SqlGeography
        public static SqlGeography CartesianToGeographic(double[] point, int srid)
        {
            double r = Math.Sqrt(point[0] * point[0] + point[1] * point[1] + point[2] * point[2]);
            double lat = 90 - (Math.Acos(point[2] / r) * (180 / Math.PI));
            double lon = Math.Atan2(point[1], point[0]) * (180 / Math.PI);
            return SqlGeography.Point(lat, lon, srid);
        }

        // Find a vector that represents the spherical midpoint between two vectors.  Really,
        // this just means we average them.
        public static double[] SphericalMidpoint(double[] aVector, double[] bVector)
        {
            double[] midPoint = new double[3];
            midPoint[0] = (aVector[0] + bVector[0]) / 2;
            midPoint[1] = (aVector[1] + bVector[1]) / 2;
            midPoint[2] = (aVector[2] + bVector[2]) / 2;
            return midPoint;
        }
    }
}
