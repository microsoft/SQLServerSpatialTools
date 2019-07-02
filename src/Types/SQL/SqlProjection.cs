//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using SQLSpatialTools.Projections;
using SQLSpatialTools.Sinks.Geography;
using SQLSpatialTools.Sinks.Geometry;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.Types.SQL
{
	[Serializable]
	[SqlUserDefinedType(Format.UserDefined, IsByteOrdered = false, MaxByteSize = -1, IsFixedLength = false)]
	public sealed class SqlProjection : INullable, IBinarySerialize
	{
		private Projection _projection;

		public SqlProjection()
		{
		}

		internal SqlProjection(Projection projection)
		{
			Debug.Assert(projection != null);
			_projection = projection;
		}

		public static SqlProjection Null
		{
			[SqlMethod(IsDeterministic = true, IsPrecise = true)]
			get => new SqlProjection();
        }

		// TODO use WKT projection description format
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Parse(SqlString s)
		{
			if (s.Value.Equals("NULL"))
				return Null;

            var a = s.Value.Split(' ');
            var cons = Type.GetType(a[0])?.GetConstructor(new[] { typeof(Dictionary<string, double>) });
            return cons != null ? new SqlProjection((Projection) cons.Invoke(new object[] {Projection.ParseParameters(a[1])})) : Null;
        }

		// TODO use WKT projection description format
		[return: SqlFacet(MaxSize = -1)]
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public override string ToString()
		{
			if (_projection == null)
			{
				return "NULL";
			}
			else
			{
				return _projection.GetType().FullName + " " + _projection.Parameters;
			}
		}

		public bool IsNull
		{
			[SqlMethod(IsDeterministic = true, IsPrecise = true)]
			get => _projection == null;
        }

		public void Read(BinaryReader r)
		{
            if (r == null) return;

            var name = r.ReadString();
            if (string.IsNullOrEmpty(name))
            {
                _projection = null;
            }
            else
            {
                var cons = Type.GetType(name)?.GetConstructor(new[] { typeof(Dictionary<string, double>) });
                if (cons != null)
                    _projection = (Projection) cons.Invoke(new object[] {Projection.ParseParameters(r.ReadString())});
            }
        }

		public void Write(BinaryWriter w)
        {
            if (w == null) return;
            if (_projection == null)
            {
                w.Write("");
            }
            else
            {
                w.Write(_projection.GetType().FullName ?? throw new InvalidOperationException());
                w.Write(_projection.Parameters);
            }
        }

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection AlbersEqualArea(double longitude0, double latitude0, double parallel1, double parallel2)
		{
            var parameters = new Dictionary<string, double>
            {
                ["longitude0"] = longitude0,
                ["latitude0"] = latitude0,
                ["parallel1"] = parallel1,
                ["parallel2"] = parallel2
            };
            return new SqlProjection(new AlbersEqualAreaProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Equirectangular(double longitude0, double parallel)
		{
            var parameters = new Dictionary<string, double> {["longitude0"] = longitude0, ["parallel"] = parallel};
            return new SqlProjection(new EquirectangularProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection LambertConformalConic(double longitude0, double latitude, double fi1, double fi2)
		{
            var parameters = new Dictionary<string, double>
            {
                ["longitude0"] = longitude0, ["latitude0"] = latitude, ["fi1"] = fi1, ["fi2"] = fi2
            };
            return new SqlProjection(new LambertConformalConicProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Mercator(double longitude0)
		{
            var parameters = new Dictionary<string, double> {["longitude0"] = longitude0};
            return new SqlProjection(new MercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection ObliqueMercator(double longitude0, double fi1, double lambda1, double fi2, double lambda2)
		{
            var parameters = new Dictionary<string, double>
            {
                ["longitude0"] = longitude0,
                ["fi1"] = fi1,
                ["lambda1"] = lambda1,
                ["fi2"] = fi2,
                ["lambda2"] = lambda2
            };
            return new SqlProjection(new ObliqueMercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection TransverseMercator(double longitude0)
		{
            var parameters = new Dictionary<string, double> {["longitude0"] = longitude0};
            return new SqlProjection(new TransverseMercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Gnomonic(double longitude, double latitude)
		{
            var parameters = new Dictionary<string, double>
            {
                ["longitude0"] = 0, ["longitude1"] = longitude, ["latitude1"] = latitude
            };
            return new SqlProjection(new GnomonicProjection(parameters));
		}

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ThrowIfArgumentNull(INullable argument, string name)
		{
			if (argument == null || argument.IsNull)
			{
				throw new ArgumentNullException(name);
			}
		}

		// INSTANCE METHODS AND FIELDS

		// Projects geography onto geometry.
		// Returns valid projected SqlGeometry object.
		// Constructed geometry will have the same SRID as geography.
		//
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeometry Project(SqlGeography geography)
		{
			ThrowIfArgumentNull(geography, "geography");
			var builder = new SqlGeometryBuilder();
			geography.Populate(new Projector(this, builder));
			return builder.ConstructedGeometry;
		}

		// Unprojects geometry producing geography. 
		// Returns a valid unprojected SqlGeography object.
		//
		// SRID taken from the geometry object may not be valid for geography,
		// in which case a new SRID must be given.
		//
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeography UnprojectWithSRID(SqlGeometry geometry, SqlInt32 newSrid)
		{
			ThrowIfArgumentNull(geometry, "geometry");
			ThrowIfArgumentNull(newSrid, "newSrid");
			var builder = new SqlGeographyBuilder();
			geometry.Populate(new UnProjector(this, builder, newSrid.Value));
			return builder.ConstructedGeography;
		}

		// Unprojects geometry producing geography.
		// This method assumes that the SRID of geometry object is valid for geography.
		// Returns a valid unprojected SqlGeography object.
		//
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeography Unproject(SqlGeometry geometry)
		{
			ThrowIfArgumentNull(geometry, "geometry");
			var builder = new SqlGeographyBuilder();
			geometry.Populate(new UnProjector(this, builder, geometry.STSrid.Value)); // NOTE srid will be reused
			return builder.ConstructedGeography;
		}

		// Longitude and latitude are in degrees.
		// Input Latitude must be in range [-90, 90].
		// Input Longitude must be in range [-15069, 15069].
		//
		public void ProjectPoint(double latitudeDeg, double longitudeDeg, out double x, out double y)
		{
			var latitude = MathX.InputLat(latitudeDeg, 90, "latitude");
			var longitude = MathX.NormalizeLongitudeRad(MathX.InputLong(longitudeDeg, 15069, "longitude") - _projection.CentralLongitudeRad);

			_projection.Project(latitude, longitude, out x, out y);

			if (double.IsNaN(x))
			{
				throw new ArgumentOutOfRangeException(nameof(x));
			}
			if (double.IsNaN(y))
			{
				throw new ArgumentOutOfRangeException(nameof(y));
			}
		}

		// Longitude and latitude are in degrees.
		// Output Latitude will be in range [-90, 90].
		// Output Longitude will be in range [-180, 180].
		//
		public void UnprojectPoint(double x, double y, out double latitudeDeg, out double longitudeDeg)
		{
			if (double.IsNaN(x))
			{
				throw new ArgumentException(Resource.InputCoordinateIsNaN, nameof(x));
			}
			if (double.IsNaN(y))
			{
				throw new ArgumentException(Resource.InputCoordinateIsNaN, nameof(y));
			}

            _projection.Unproject(x, y, out var latitude, out var longitude);

			if (double.IsNaN(latitude) || latitude < -Math.PI / 2 || latitude > Math.PI / 2)
			{
				throw new ArgumentOutOfRangeException(nameof(latitude), string.Format(CultureInfo.InvariantCulture, Resource.OutputLatitudeIsOutOfRange, latitude));
			}
			if (double.IsNaN(longitude) || longitude < -Math.PI || longitude > Math.PI)
			{
				throw new ArgumentOutOfRangeException(nameof(longitude), string.Format(CultureInfo.InvariantCulture, Resource.OutputLongitudeIsOutOfRange, longitude));
			}

			latitudeDeg = MathX.Clamp(90, SpatialUtil.ToDegrees(latitude));
			longitudeDeg = MathX.NormalizeLongitudeDeg(SpatialUtil.ToDegrees(longitude + _projection.CentralLongitudeRad));
		}
	}
}