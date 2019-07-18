//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System;
using SQLSpatialTools.Utility;

namespace SQLSpatialTools.KMLProcessor
{
	/// <summary>
	/// This class contains the utility methods
	/// </summary>
	public static class Utilities
	{
        /// <summary>
        /// This method shifts the given value into the range [-upperBorder, upperBorder]
        /// </summary>
        /// <param name="value">The value to be shifted</param>
        /// <param name="upperBorder">The range's upper bound</param>
        /// <returns>The shifted value</returns>
        public static double ShiftInRange(double value, double upperBorder)
		{
			upperBorder = Math.Abs(upperBorder);

			if (upperBorder.EqualsTo(0))
				throw new KMLException("The range has to be greater then 0.");

			var rangeSize = 2 * upperBorder;

			if (value > upperBorder)
			{
				var k = (int)((value + upperBorder) / rangeSize);
				value -= k * rangeSize;
			}
			else if (value < -1 * upperBorder)
			{
				var k = (int)((value - upperBorder) / rangeSize);
				value -= k * rangeSize;
			}

			return value;
		}
	}
}
