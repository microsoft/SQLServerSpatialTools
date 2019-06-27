//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;

namespace SQLSpatialTools.KMLProcessor
{
	/// <summary>
	/// Instance of this class will be thrown for every exception which occurs in the KML processor.
	/// </summary>
	public class KMLException : Exception
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="message">Exception message</param>
		public KMLException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Constructor. Constructs the KMLException using the given exception message and the inner exception
		/// </summary>
		/// <param name="message">Exception message</param>
		/// <param name="innerException">Inner exception</param>
		public KMLException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		#endregion
	}
}
