using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// This class contains the utility methods
	/// </summary>
	public class Utilities
	{
		/// <summary>
		/// This method shifts the given value into the range [-upperBorder, upperBorder]
		/// </summary>
		/// <param name="value">The value to be shifted</param>
		/// <param name="phase">The range's upper bound</param>
		/// <returns>The shifted value</returns>
		public static double ShiftInRange(double value, double upperBorder)
		{
			upperBorder = Math.Abs(upperBorder);

			if (upperBorder == 0)
				throw new KMLException("The range has to be greater then 0.");

			double rangeSize = 2 * upperBorder;

			if (value > upperBorder)
			{
				int k = (int)((value + upperBorder) / rangeSize);
				value = value - k * rangeSize;
			}
			else if (value < -1 * upperBorder)
			{
				int k = (int)((value - upperBorder) / rangeSize);
				value = value - k * rangeSize;
			}

			return value;
		}
	}
}
