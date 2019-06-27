//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Utility;
using SQLSpatialTools.Sinks.Geometry;

namespace SQLSpatialTools.Types.SQL
{
	[Serializable]
	[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = -1, IsFixedLength = false)]
	public sealed class AffineTransform : INullable, IBinarySerialize
	{
        private double _ax, _bx, _cx;
        private double _ay, _by, _cy;

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Translate(double cx, double cy)
		{
            var transform = new AffineTransform {_ax = 1, _by = 1, _cx = cx, _cy = cy};
            return transform;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Rotate(double angleDeg)
		{
			var angle = SpatialUtil.ToRadians(angleDeg);
            var transform = new AffineTransform {_ax = Math.Cos(angle), _ay = Math.Sin(angle)};
            transform._bx = -transform._ay;
			transform._by = transform._ax;
			return transform;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static AffineTransform Scale(double sx, double sy)
		{
            var transform = new AffineTransform {_ax = sx, _by = sy};
            return transform;
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeometry Apply(SqlGeometry geometry)
		{
			var builder = new SqlGeometryBuilder();
			geometry.Populate(new GeometryTransformer(builder, this));
			return builder.ConstructedGeometry;
		}

		public double GetX(double x, double y)
		{
			return _ax * x + _bx * y + _cx;
		}

		public double GetY(double x, double y)
		{
			return _ay * x + _by * y + _cy;
		}

		public void Read(BinaryReader r)
		{
			_ax = r.ReadDouble();
			_bx = r.ReadDouble();
			_cx = r.ReadDouble();
			_ay = r.ReadDouble();
			_by = r.ReadDouble();
			_cy = r.ReadDouble();
		}

		public void Write(BinaryWriter binaryWriter)
		{
			binaryWriter.Write(_ax);
			binaryWriter.Write(_bx);
			binaryWriter.Write(_cx);
			binaryWriter.Write(_ay);
			binaryWriter.Write(_by);
			binaryWriter.Write(_cy);
		}

		public bool IsNull => false;

        public static AffineTransform Null
		{
			[SqlMethod(IsDeterministic = true, IsPrecise = true)]
			get => null;
        }

        [SqlMethod(IsDeterministic = true, IsPrecise = false)]
        public static AffineTransform Parse(SqlString str)
        {
            var transform = new AffineTransform();

            var args = str.ToString().Split(' ');
            transform._ax = double.Parse(args[0]);
            transform._bx = double.Parse(args[1]);
            transform._cx = double.Parse(args[2]);
			transform._ay = double.Parse(args[3]);
			transform._by = double.Parse(args[4]);
			transform._cy = double.Parse(args[5]);
            
            return transform;
        }

        [return: SqlFacet(MaxSize = -1)]
        [SqlMethod(IsDeterministic = true, IsPrecise = false)]
        public override string ToString()
        {
            return _ax + " " + _bx + " " + _cx + " " + _ay + " " + _by + " " + _cy;
        }
	}
}