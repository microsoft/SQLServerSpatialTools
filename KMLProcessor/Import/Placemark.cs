using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the information about a placemark extracted from the KML file
	/// </summary>
	public class Placemark
	{
		#region Public Properties

		/// <summary>
		/// Identifier
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
		/// Name
		/// </summary>
		public string Name
		{
			get { return m_Name; }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					m_Name = "";
				}
				else
				{
					m_Name = value;
				}
			}
		}
		/// <summary>
		/// Data member for the property Name
		/// </summary>
		protected string m_Name = "";

		/// <summary>
		/// Description
		/// </summary>
		public string Description
		{
			get { return m_Description; }
			set
			{
				if (string.IsNullOrEmpty(value))
					m_Description = "";
				else
					m_Description = value;
			}
		}
		/// <summary>
		/// Data member for the Description property
		/// </summary>
		protected string m_Description = "";

		/// <summary>
		/// Address
		/// </summary>
		public string Address
		{
			get { return m_Address; }
			set
			{
				if (string.IsNullOrEmpty(value))
					m_Address = "";
				else
					m_Address = value;
			}
		}
		/// <summary>
		/// Data member for the Address property
		/// </summary>
		protected string m_Address = "";

		/// <summary>
		/// The geography instance which describes this placemark
		/// </summary>
		public Geography Geography
		{
			get { return m_Geography; }
			set { m_Geography = value; }
		}
		/// <summary>
		/// Data member for the Geography property
		/// </summary>
		protected Geography m_Geography;

		/// <summary>
		/// The geography instance where this placemark looks at
		/// </summary>
		public Geography LookAt
		{
			get { return m_LookAt; }
			set { m_LookAt = value; }
		}
		/// <summary>
		/// Data member for the LookAt property
		/// </summary>
		protected Geography m_LookAt;

		#endregion
	}
}
