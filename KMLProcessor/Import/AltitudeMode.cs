using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SqlServer.SpatialToolbox.KMLProcessor
{
	/// <summary>
	/// Altitude modes allowed in KML
	/// </summary>
	public enum AltitudeMode
	{
		clampToGround = 0,
		relativeToGround = 1,
		absolute = 2,
		clampToSeaFloor = 3,
		relativeToSeaFloor = 4
	}
}
