using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the information about the context where a geography instance is found.
	/// </summary>
	public class GeographyContext
	{
		/// <summary>
		/// Geography instance Id
		/// </summary>
		public string Id
		{
			get { return m_Id; }
			set { m_Id = value; }
		}
		/// <summary>
		/// Data member for the Id property
		/// </summary>
		protected string m_Id = null;

		/// <summary>
		/// Geography instance context. It contains the information about the parent and 
		/// the siblings of this geography instance.
		/// It also contains the information about this geography instance.
		/// </summary>
		public string Context
		{
			get { return m_Context; }
			set { m_Context = value; }
		}
		/// <summary>
		/// Data member for the Context property
		/// </summary>
		protected string m_Context = null;

		/// <summary>
		/// The extracted geography instance
		/// </summary>
		public SqlGeography Shape
		{
			get { return m_Shape; }
			set { m_Shape = value; }
		}
		/// <summary>
		/// Data member for the Shape property
		/// </summary>
		protected SqlGeography m_Shape = null;
	}
}
