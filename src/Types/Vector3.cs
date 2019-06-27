//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types
{
    internal class Vector3
    {
        // Fields.
        public readonly double X, Y, Z;

        // Constructors.
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Addition.
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        // Subtraction.
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        // Multiplication.
        public static Vector3 operator *(Vector3 vector, double a)
        {
            return new Vector3(vector.X * a, vector.Y * a, vector.Z * a);
        }

        // Division.
        public static Vector3 operator /(Vector3 v, double a)
        {
            return v * (1 / a);
        }

        // Unit vector with the same direction.
        public Vector3 Unitize()
        {
            return this / VectorLength();
        }

        // Dot product.
        public static double operator *(Vector3 a, Vector3 b)
        {
            return b.X * a.X + b.Y * a.Y + b.Z * a.Z;
        }

        // The square if the length.
        public double LengthSquared()
        {
            return this * this;
        }

        // The length of the vector.
        public double VectorLength()
        {
            return Math.Sqrt(LengthSquared());
        }

        // Squared distance between vectors a and b.
        public double DistanceSquared(Vector3 a)
        {
            return (this - a) * (this - a);
        }

        // Distance between vectors a and b.
        public double Distance(Vector3 a)
        {
            return Math.Sqrt(DistanceSquared(a));
        }

        // Cross product between vectors a and b.
        public Vector3 CrossProduct(Vector3 a)
        {
            return new Vector3(Y * a.Z - Z * a.Y, Z * a.X - X * a.Z, X * a.Y - Y * a.X);
        }

        // Angle in radians between vectors a and b.
        public double Angle(Vector3 a)
        {
            return 2 * Math.Asin(Distance(a) / (2 * a.VectorLength()));
        }

        // Angle in degrees between vectors a and b.
        public double AngleInDegrees(Vector3 a)
        {
            return SpatialUtil.ToDegrees(Angle(a));
        }   
    }
}