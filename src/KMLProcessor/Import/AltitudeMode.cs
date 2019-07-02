//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

namespace SQLSpatialTools.KMLProcessor.Import
{
    /// <summary>
    /// Altitude modes allowed in KML
    /// </summary>
    public enum AltitudeMode
    {
        ClampToGround = 0,
        RelativeToGround = 1,
        Absolute = 2,
        ClampToSeaFloor = 3,
        RelativeToSeaFloor = 4
    }
}
