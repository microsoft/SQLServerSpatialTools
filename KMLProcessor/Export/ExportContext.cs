using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Types;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class will keep track of the context where some action will be executed
	/// during the KML export process. This class will keep track of the number of figures previously 
	/// started in a spatial object, it will also keep track of the spatial object nesting level and the
	/// type of each opened/started spatial object
	/// <typeparam name="T">Type of the spatial object: OpenGisGeographyType or OpenGisGeometryType</typeparam>
	/// </summary>
	internal class ExportContext<T>
	{
		#region Private Data

		/// <summary>
		/// This list will simulate the stack of spatial object types. When a spatial object instance
		/// is visited, its type will be stored on the end of this list, and the visit will
		/// proceed on its child objects. So this list will store the types of all
		/// parents of the currently visited spatial object. When a visit to a spatial object is finished,
		/// the type of that object will be removed from the end of this list.
		/// </summary>
		private List<T> m_Type = new List<T>();

		/// <summary>
		/// This structure is equivalent to the m_Type structure, except it will store the information
		/// about the number of figures which are visited in each spatial object.
		/// </summary>
		private List<int> m_Figures = new List<int>();

		/// <summary>
		/// Represents the depth in the spatial object tree. It will show the distance from the currently 
		/// spatial object to the root spatial object.
		/// </summary>
		private int m_Depth = 0;

		#endregion

		#region Public Methods

		/// <summary>
		/// This method should be called when a visit to the spatial object is started.
		/// The context will be updated with the information about the given spatial object type.
		/// </summary>
		/// <param name="type">Type of spatial object</param>
		public void BeginSpatialObject(T type)
		{
			m_Depth += 1;		// Increment the tree depth

			// Add information about this spatial object in the stack of visited spatial objects
			if (m_Depth > m_Type.Count)
			{
				m_Type.Add(type);
				m_Figures.Add(0);
			}
			else
			{
				m_Type[m_Depth - 1] = type;
				m_Figures[m_Depth - 1] = 0;
			}
		}

		/// <summary>
		/// This method should be called when the visit to spatial object is finished
		/// </summary>
		public void EndSpatialObject()
		{
			m_Depth -= 1;
		}

		/// <summary>
		/// This method should be called when new figure visit is started
		/// </summary>
		public void BeginFigure()
		{
			m_Figures[m_Depth - 1] += 1;
		}

		/// <summary>
		/// This method should be called when a figure visit is finished
		/// </summary>
		public void EndFigure()
		{
			// Nothing to be done. We calculates the total number of figures inside the spatial object,
			// so we don't need to decrease the counter
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The type of spatial object which is currently being processed
		/// </summary>
		public T Type
		{
			get
			{
				if (m_Depth == 0 || m_Depth > m_Type.Count)
					throw new KMLException("Type tree is invalid!");

				return m_Type[m_Depth - 1];
			}
		}

		/// <summary>
		/// True if the figure which is currently being processed is the first figure in 
		/// the current spatial object
		/// </summary>
		public bool IsFirstFigure
		{
			get
			{
				return Figures == 1;
			}
		}

		/// <summary>
		/// The number of visited figures in the current spatial object
		/// </summary>
		public int Figures
		{
			get
			{
				if (m_Depth == 0 || m_Depth > m_Figures.Count)
					throw new KMLException("Figure tree is invalid!");

				return m_Figures[m_Depth - 1];
			}
		}

		#endregion
	}
}
