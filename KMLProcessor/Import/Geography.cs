using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// Base class for all geography instances extracted from the KML file
	/// </summary>
	public abstract class Geography
	{
		#region Public Properties

		/// <summary>
		/// Id of the placemark which contains this geography instance
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
		/// True if point(s) which are part of this geography instance 
		/// should be connected to the ground when they are visualy represented in some visualisation tool
		/// </summary>
		public bool Extrude
		{
			get { return m_Extrude; }
			set
			{
				m_Extrude = value;
			}
		}
		/// <summary>
		/// Data member for the Extrude property
		/// </summary>
		protected bool m_Extrude = false;

		/// <summary>
		/// True if all lines in this geography instance should follow the terrain. 
		/// This flag is not applicable just for Point instance.
		/// </summary>
		public bool Tesselate
		{
			get { return m_Tesselate; }
			set
			{
				m_Tesselate = value;
			}
		}
		/// <summary>
		/// Data member for the Tesselate property
		/// </summary>
		protected bool m_Tesselate = false;

		/// <summary>
		/// True if this geography instance is valid
		/// </summary>
		public bool IsValid
		{
			get
			{
				// This implementation is base on the property that the Sql Server 2008 (10.0 Katmai)
				// will throw an exception if the geography instance is not valid. 

				try
				{
					SqlGeographyBuilder constructed = new SqlGeographyBuilder();
					constructed.SetSrid(Constants.DefaultSRID);

					Populate(constructed);

					SqlGeography g = constructed.ConstructedGeography;

					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		/// <summary>
		/// The context where this geography instance is found. It will
		/// contain the information about the parent and the siblings of this
		/// geography instance, and the information about this geography instance
		/// it self.
		/// </summary>
		public string Context
		{
			get { return m_Context; }
			set { m_Context = value; }
		}
		/// <summary>
		/// Data member for the Context property
		/// </summary>
		protected string m_Context;

		#endregion

		#region Abstract Members

		/// <summary>
		/// This method populates the given sink with the data from this geography instance
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public abstract void Populate(IGeographySink sink);

		/// <summary>
		/// SqlGeography instance well-known text.
		/// </summary>
		public abstract string WKT
		{
			get;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// This method returns the geography instance that corresponds to this KML geography
		/// </summary>
		/// <param name="makeValid">If true and this geography instance is invalid then the MakeValid
		/// function will be executed on this geography instance</param>
		/// <returns>The geography instance that corresponds to this KML geography</returns>
		public SqlGeography ToSqlGeography(bool makeValid)
		{
			if (IsValid)
			{
				SqlGeographyBuilder constructed = new SqlGeographyBuilder();
				constructed.SetSrid(Constants.DefaultSRID);
				Populate(constructed);
				return constructed.ConstructedGeography;
			}
			else if (makeValid)
			{
				SqlGeographyBuilder constructed = new SqlGeographyBuilder();
				constructed.SetSrid(Constants.DefaultSRID);
				MakeValid(constructed);
				return constructed.ConstructedGeography;
			}

			throw new KMLException("Invalid geography instance.");
		}

		/// <summary>
		/// This method returns a string representation of this object
		/// </summary>
		/// <returns>WKT for this geography instance</returns>
		public override string ToString()
		{
			return WKT;
		}

		/// <summary>
		/// This method populates the given sink with the data from this geography instance.
		/// If this geography instance is invalid and the makeValid flag is set then a valid geography instance
		/// will be constructed and the given sink will be populated with that instance.
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		/// <param name="makeValid">If true and this geography instance is invalid then the MakeValid
		/// function will be executed on this geography instance</param>
		public void Populate(
			IGeographySink sink,
			bool makeValid)
		{
			if (makeValid)
			{
				if (IsValid)
					Populate(sink);
				else
					MakeValid(sink);
			}
			else
			{
				Populate(sink);
			}
		}

		/// <summary>
		/// This method populates the given sink with the valid geography instance constructed from this geography instance.
		/// </summary>
		/// <param name="sink">Sink to be populated</param>
		public void MakeValid(IGeographySink sink)
		{
			// 1. Creates the valid geography for this WKT
			SqlGeography vg = SQLSpatialTools.Functions.MakeValidGeographyFromText(this.WKT, Constants.DefaultSRID);

			// 2. Populates the given sink with the valid geography
			vg.Populate(new FilterSetSridGeographySink(sink));
		}

		#endregion

		#region Internal Properties

		/// <summary>
		/// Id which was assigned to this geography instance when it was inserted in the database. 
		/// If the value is null then it is not stored in the database yet
		/// </summary>
		internal int? DbId
		{
			get { return m_DbId; }
			set
			{
				m_DbId = value;
			}
		}
		/// <summary>
		/// Data member for the DbId property
		/// </summary>
		private int? m_DbId;

		#endregion
	}
}

