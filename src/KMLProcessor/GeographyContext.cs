//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using Microsoft.SqlServer.Types;

namespace SQLSpatialTools.KMLProcessor
{
	/// <summary>
	/// This class contains the information about the context where a geography instance is found.
	/// </summary>
	public class GeographyContext
	{
		/// <summary>
		/// Geography instance Id
		/// </summary>
		public string Id { get; set; }

        /// <summary>
		/// Geography instance context. It contains the information about the parent and 
		/// the siblings of this geography instance.
		/// It also contains the information about this geography instance.
		/// </summary>
		public string Context { get; set; }

		/// <summary>
		/// The extracted geography instance
		/// </summary>
		public SqlGeography Shape { get; set; }
	}
}
