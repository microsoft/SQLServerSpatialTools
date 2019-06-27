//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor
{
	/// <summary>
	/// This class will propagate all method calls to the given target sink, 
    /// except the method call to the SetSrid method.
	/// </summary>
	public class FilterSetSridGeographySink : IGeographySink110
	{
		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="targetSink">Target sink</param>
		public FilterSetSridGeographySink(IGeographySink110 targetSink)
		{
			_targetSink = targetSink;
		}

		#endregion

		#region Private Data

		/// <summary>
		/// Target sink to propagate all method calls to it, except the method call to the SetSrid method
		/// </summary>
		private readonly IGeographySink110 _targetSink;

		#endregion

		#region IGeographySink Members

		public void AddLine(double latitude, double longitude, double? z, double? m)
        {
            _targetSink?.AddLine(latitude, longitude, z, m);
        }

        public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
        {
            throw new Exception("AddCircularArc is not implemented yet in this class");
        }

        public void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            _targetSink?.BeginFigure(latitude, longitude, z, m);
        }

		public void BeginGeography(OpenGisGeographyType type)
        {
            _targetSink?.BeginGeography(type);
        }

		public void EndFigure()
        {
            _targetSink?.EndFigure();
        }

		public void EndGeography()
        {
            _targetSink?.EndGeography();
        }

		/// <summary>
		/// This method call will not be propagated to the target sink
		/// </summary>
		/// <param name="srid"></param>
		public void SetSrid(int srid)
		{
		}

		#endregion
	}
}
