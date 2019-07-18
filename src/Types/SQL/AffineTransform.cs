//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	[Serializable]
	[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = -1, IsFixedLength = false)]
	public sealed class AffineTransform : INullable, IBinarySerialize
	{
		public double ax, bx, cx;
		public double ay, by, cy;

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Translate(double dx, double dy)
		{
			AffineTransform t = new AffineTransform();
			t.ax = 1;
			t.by = 1;
			t.cx = dx;
			t.cy = dy;
			return t;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Rotate(double angleDeg)
		{
			double angle = Util.ToRadians(angleDeg);
			AffineTransform t = new AffineTransform();
			t.ax = Math.Cos(angle);
			t.ay = Math.Sin(angle);
			t.bx = -t.ay;
			t.by = t.ax;
			return t;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Scale(double sx, double sy)
		{
			AffineTransform t = new AffineTransform();
			t.ax = sx;
			t.by = sy;
			return t;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeometry Apply(SqlGeometry geometry)
		{
			SqlGeometryBuilder builder = new SqlGeometryBuilder();
			geometry.Populate(new GeometryTransformer(builder, this));
			return builder.ConstructedGeometry;
		}

		public double GetX(double x, double y)
		{
			return ax * x + bx * y + cx;
		}

		public double GetY(double x, double y)
		{
			return ay * x + by * y + cy;
		}

		public void Read(BinaryReader r)
		{
			ax = r.ReadDouble();
			bx = r.ReadDouble();
			cx = r.ReadDouble();
			ay = r.ReadDouble();
			by = r.ReadDouble();
			cy = r.ReadDouble();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(ax);
			w.Write(bx);
			w.Write(cx);
			w.Write(ay);
			w.Write(by);
			w.Write(cy);
		}

		public bool IsNull
		{
			get { return false; }
		}

		public static AffineTransform Null
		{
			[SqlMethod(IsDeterministic = true, IsPrecise = true)]
			get
			{
				return null;
			}
		}

        [SqlMethod(IsDeterministic = true, IsPrecise = false)]
        public static AffineTransform Parse(SqlString str)
        {
            AffineTransform tranform = new AffineTransform();

            string[] args = str.ToString().Split(' ');
            tranform.ax = Double.Parse(args[0]);
            tranform.bx = Double.Parse(args[1]);
            tranform.cx = Double.Parse(args[2]);
			tranform.ay = Double.Parse(args[3]);
			tranform.by = Double.Parse(args[4]);
			tranform.cy = Double.Parse(args[5]);
            
            return tranform;
        }

        [return: SqlFacet(MaxSize = -1)]
        [SqlMethod(IsDeterministic = true, IsPrecise = false)]
        public override string ToString()
        {
            return ax + " " + bx + " " + cx + " " + ay + " " + by + " " + cy;
        }
	}
}