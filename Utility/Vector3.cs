// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace SQLSpatialTools
{
    internal class Vector3
    {
        // Fields.
        public readonly double x, y, z;

        // Constructors.
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // Addition.
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        // Subtraction.
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        // Multiplication.
        public static Vector3 operator *(Vector3 vector, double a)
        {
            return new Vector3(vector.x * a, vector.y * a, vector.z * a);
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
            return b.x * a.x + b.y * a.y + b.z * a.z;
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
            return new Vector3(y * a.z - z * a.y, z * a.x - x * a.z, x * a.y - y * a.x);
        }

        // Angle in radians between vectors a and b.
        public double Angle(Vector3 a)
        {
            return 2 * Math.Asin(this.Distance(a) / (2 * a.VectorLength()));
        }

        // Angle in degrees between vectors a and b.
        public double AngleInDegrees(Vector3 a)
        {
            return Util.ToDegrees(this.Angle(a));
        }   
    }
}