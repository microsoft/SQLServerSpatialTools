//------------------------------------------------------------------------------
// Copyright (c) 2019 Microsoft Corporation. All rights reserved.
//------------------------------------------------------------------------------

using System.Data.SqlTypes;

namespace SQLSpatialTools.Utility
{
    internal static class SQLTypeConversions
    {
        public class Numeric
        {
            private readonly int _value;

            private Numeric(int value)
            {
                _value = value;
            }

            public static implicit operator int(Numeric v)
            {
                return v._value;
            }

            public static implicit operator Numeric(SqlInt32 value)
            {
                return new Numeric((int)value);
            }
        }
    }
}