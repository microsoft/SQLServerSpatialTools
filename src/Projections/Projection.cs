//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//
// Purpose: Abstract base class of all projections.
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SQLSpatialTools.Projections
{
	internal abstract class Projection
	{
		private readonly Dictionary<string, double> _parameters;

        public double CentralLongitudeRad { get; }

        public string Parameters
		{
			get
			{
				var a = new StringBuilder();
				foreach (var key in _parameters.Keys)
                {
                    var value = _parameters[key].ToString("R", CultureInfo.InvariantCulture);
                    a.AppendFormat(a.Length > 0 ? ";{0}={1}" : "{0}={1}", key, value);
                }
				return a.ToString();
			}
		}

		public static Dictionary<string, double> ParseParameters(string parameters)
        {
            return parameters.Split(';')
                .Select(pair => pair.Split('='))
                .ToDictionary(a => a[0],
                    a => double.Parse(a[1],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture));
        }

        protected Projection(IDictionary<string, double> parameters)
		{
			Debug.Assert(parameters != null);
			_parameters = new Dictionary<string,double>(parameters);
			CentralLongitudeRad = InputLongitude("longitude0");
		}

		// Used for validation and conversion of projection input parameters.
		//
		// Returns latitude converted to radians in interval (-Pi/2, Pi/2).
		// Throws ArgumentOutOfRangeException if latitude is NaN or not in range [-89.9, 89.9].
		//
		// Param name: name of latitude argument in case if the exception is thrown
		//
		protected internal double InputLatitude(string name)
		{
			return InputLatitude(name, 89.9);
		}

		protected internal double InputLatitude(string name, double max)
		{
			return MathX.InputLat(_parameters[name], max, name);
		}

		// Used for validation and conversion of projection input parameters.
		//
		// Returns longitude converted to radians in range [-Pi, Pi).
		// Throws ArgumentOutOfRangeException if longitude is NaN or Infinity.
		//
		// Param name: name of longitude argument in case if the exception is thrown
		//
		protected internal double InputLongitude(string name)
		{
			return InputLongitude(name, 360);
		}

		protected internal double InputLongitude(string name, double max)
		{
			return MathX.InputLong(_parameters[name], max, name);
		}

		// Longitude and latitude are in radians.
		// Latitude is assumed to be in range [-Pi/2, Pi/2].
		// Longitude is assumed to be in range [-Pi, Pi).
		protected internal abstract void Project(double latitude, double longitude, out double x, out double y);

		// Longitude and latitude are in radians.
		// Latitude must be in range [-Pi/2, Pi/2].
		// Longitude must be in range [-Pi, Pi).
		protected internal abstract void Unproject(double x, double y, out double latitude, out double longitude);
	}
}