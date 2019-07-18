//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
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
			get
			{
				return new SqlProjection();
			}
		}

		// TODO use WKT projection description format
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Parse(SqlString s)
		{
			if (s.Value.Equals("NULL"))
			{
				return SqlProjection.Null;
			}
			else
			{
				string[] a = s.Value.Split(' ');
				ConstructorInfo cons = Type.GetType(a[0]).GetConstructor(new Type[] { typeof(Dictionary<String, double>) });
				return new SqlProjection((Projection)cons.Invoke(new object[] { Projection.ParseParameters(a[1]) }));
			}
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
			get { return _projection == null; }
		}

		public void Read(BinaryReader r)
		{
			if (r != null)
			{
				string name = r.ReadString();
				if (name.Equals(""))
				{
					_projection = null;
				}
				else
				{
					ConstructorInfo cons = Type.GetType(name).GetConstructor(new Type[] { typeof(Dictionary<String, double>) });
					_projection = (Projection)cons.Invoke(new object[] { Projection.ParseParameters(r.ReadString()) });
				}
			}
		}

		public void Write(BinaryWriter w)
		{
			if (w != null)
			{
				if (_projection == null)
				{
					w.Write("");
				}
				else
				{
					w.Write(_projection.GetType().FullName);
					w.Write(_projection.Parameters);
				}
			}
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection AlbersEqualArea(double longitude0, double latitude0, double parallel1, double parallel2)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			parameters["latitude0"] = latitude0;
			parameters["parallel1"] = parallel1;
			parameters["parallel2"] = parallel2;
			return new SqlProjection(new AlbersEqualAreaProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Equirectangular(double longitude0, double parallel)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			parameters["parallel"] = parallel;
			return new SqlProjection(new EquirectangularProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection LambertConformalConic(double longitude0, double latitude, double fi1, double fi2)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			parameters["latitude0"] = latitude;
			parameters["fi1"] = fi1;
			parameters["fi2"] = fi2;
			return new SqlProjection(new LambertConformalConicProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Mercator(double longitude0)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			return new SqlProjection(new MercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection ObliqueMercator(double longitude0, double fi1, double lambda1, double fi2, double lambda2)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			parameters["fi1"] = fi1;
			parameters["lambda1"] = lambda1;
			parameters["fi2"] = fi2;
			parameters["lambda2"] = lambda2;
			return new SqlProjection(new ObliqueMercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection TranverseMercator(double longitude0)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = longitude0;
			return new SqlProjection(new TranverseMercatorProjection(parameters));
		}

		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public static SqlProjection Gnomonic(double longitude, double latitude)
		{
			Dictionary<String, double> parameters = new Dictionary<string, double>();
			parameters["longitude0"] = 0;
			parameters["longitude1"] = longitude;
			parameters["latitude1"] = latitude;
			return new SqlProjection(new GnommonicProjection(parameters));
		}

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
			SqlGeometryBuilder builder = new SqlGeometryBuilder();
			geography.Populate(new Projector(this, builder));
			return builder.ConstructedGeometry;
		}

		// Uprojects geometry producing geography. 
		// Returns a valid uprojected SqlGeography object.
		//
		// SRID taken from the geometry object may not be valid for geography,
		// in which case a new SRID must be given.
		//
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeography UnprojectWithSRID(SqlGeometry geometry, SqlInt32 newSrid)
		{
			ThrowIfArgumentNull(geometry, "geometry");
			ThrowIfArgumentNull(newSrid, "newSrid");
			SqlGeographyBuilder builder = new SqlGeographyBuilder();
			geometry.Populate(new Unprojector(this, builder, newSrid.Value));
			return builder.ConstructedGeography;
		}

		// Uprojects geometry producing geography.
		// This method assumes that the SRID of geometry object is valid for geography.
		// Returns a valid uprojected SqlGeography object.
		//
		[SqlMethod(IsDeterministic = true, IsPrecise = false)]
		public SqlGeography Unproject(SqlGeometry geometry)
		{
			ThrowIfArgumentNull(geometry, "geometry");
			SqlGeographyBuilder builder = new SqlGeographyBuilder();
			geometry.Populate(new Unprojector(this, builder, geometry.STSrid.Value)); // NOTE srid will be reused
			return builder.ConstructedGeography;
		}

		// Longitude and latitude are in degrees.
		// Input Latitude must be in range [-90, 90].
		// Input Longitude must be in range [-15069, 15069].
		//
		public void ProjectPoint(double latitudeDeg, double longitudeDeg, out double x, out double y)
		{
			double latitude = MathX.InputLat(latitudeDeg, 90, "latitude");
			double longitude = MathX.NormalizeLongitudeRad(MathX.InputLong(longitudeDeg, 15069, "longitude") - _projection.CentralLongitudeRad);

			_projection.Project(latitude, longitude, out x, out y);

			if (Double.IsNaN(x))
			{
				throw new ArgumentOutOfRangeException("x");
			}
			if (Double.IsNaN(y))
			{
				throw new ArgumentOutOfRangeException("y");
			}
		}

		// Longitude and latitude are in degrees.
		// Output Latitude will be in range [-90, 90].
		// Output Longitude will be in range [-180, 180].
		//
		public void UnprojectPoint(double x, double y, out double latitudeDeg, out double longitudeDeg)
		{
			if (Double.IsNaN(x))
			{
				throw new ArgumentException(Resource.InputCoordinateIsNaN, "x");
			}
			if (Double.IsNaN(y))
			{
				throw new ArgumentException(Resource.InputCoordinateIsNaN, "y");
			}

			double latitude, longitude;
			_projection.Unproject(x, y, out latitude, out longitude);

			if (Double.IsNaN(latitude) || latitude < -Math.PI / 2 || latitude > Math.PI / 2)
			{
				throw new ArgumentOutOfRangeException("latitude", String.Format(CultureInfo.InvariantCulture, Resource.OutputLatitudeIsOutOfRange, latitude));
			}
			if (Double.IsNaN(longitude) || longitude < -Math.PI || longitude > Math.PI)
			{
				throw new ArgumentOutOfRangeException("longitude", String.Format(CultureInfo.InvariantCulture, Resource.OutputLongitudeIsOutOfRange, longitude));
			}

			latitudeDeg = MathX.Clamp(90, Util.ToDegrees(latitude));
			longitudeDeg = MathX.NormalizeLongitudeDeg(Util.ToDegrees(longitude + _projection.CentralLongitudeRad));
		}
	}
}