using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class will propagate all method calls to the given target sink, 
    /// except the method call to the SetSrid method.
	/// </summary>
	public class FilterSetSridGeographySink : IGeographySink
	{
		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="targetSink">Target sink</param>
		public FilterSetSridGeographySink(IGeographySink targetSink)
		{
			m_TargetSink = targetSink;
		}

		#endregion

		#region Private Data

		/// <summary>
		/// Target sink to propagate all method calls to it, except the method call to the SetSrid method
		/// </summary>
		private IGeographySink m_TargetSink;

		#endregion

		#region IGeographySink Members

		public void AddLine(double latitude, double longitude, double? z, double? m)
		{
			if (m_TargetSink != null)
				m_TargetSink.AddLine(latitude, longitude, z, m);
		}

		public void BeginFigure(double latitude, double longitude, double? z, double? m)
		{
			if (m_TargetSink != null)
				m_TargetSink.BeginFigure(latitude, longitude, z, m);
		}

		public void BeginGeography(OpenGisGeographyType type)
		{
			if (m_TargetSink != null)
				m_TargetSink.BeginGeography(type);
		}

		public void EndFigure()
		{
			if (m_TargetSink != null)
				m_TargetSink.EndFigure();
		}

		public void EndGeography()
		{
			if (m_TargetSink != null)
				m_TargetSink.EndGeography();
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
