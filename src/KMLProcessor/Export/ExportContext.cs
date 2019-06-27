//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SQLSpatialTools.KMLProcessor.Export
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
		private readonly List<T> _type = new List<T>();

		/// <summary>
		/// This structure is equivalent to the m_Type structure, except it will store the information
		/// about the number of figures which are visited in each spatial object.
		/// </summary>
		private readonly List<int> _figures = new List<int>();

		/// <summary>
		/// Represents the depth in the spatial object tree. It will show the distance from the currently 
		/// spatial object to the root spatial object.
		/// </summary>
		private int _depth;

		#endregion

		#region Public Methods

		/// <summary>
		/// This method should be called when a visit to the spatial object is started.
		/// The context will be updated with the information about the given spatial object type.
		/// </summary>
		/// <param name="type">Type of spatial object</param>
		public void BeginSpatialObject(T type)
		{
			_depth += 1;		// Increment the tree depth

			// Add information about this spatial object in the stack of visited spatial objects
			if (_depth > _type.Count)
			{
				_type.Add(type);
				_figures.Add(0);
			}
			else
			{
				_type[_depth - 1] = type;
				_figures[_depth - 1] = 0;
			}
		}

		/// <summary>
		/// This method should be called when the visit to spatial object is finished
		/// </summary>
		public void EndSpatialObject()
		{
			_depth -= 1;
		}

		/// <summary>
		/// This method should be called when new figure visit is started
		/// </summary>
		public void BeginFigure()
		{
			_figures[_depth - 1] += 1;
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
				if (_depth == 0 || _depth > _type.Count)
					throw new KMLException("Type tree is invalid!");

				return _type[_depth - 1];
			}
		}

		/// <summary>
		/// True if the figure which is currently being processed is the first figure in 
		/// the current spatial object
		/// </summary>
		public bool IsFirstFigure => Figures == 1;

        /// <summary>
		/// The number of visited figures in the current spatial object
		/// </summary>
		public int Figures
		{
			get
			{
				if (_depth == 0 || _depth > _figures.Count)
					throw new KMLException("Figure tree is invalid!");

				return _figures[_depth - 1];
			}
		}

		#endregion
	}
}
