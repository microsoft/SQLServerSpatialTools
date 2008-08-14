//------------------------------------------------------------------------------
// Copyright (c) 2008 Microsoft Corporation.
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;

namespace SQLSpatialTools
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[SqlUserDefinedAggregate(
		Format.UserDefined,
		IsInvariantToDuplicates = true,
		IsInvariantToNulls = true,
		IsInvariantToOrder = true,
		IsNullIfEmpty = false,
		MaxByteSize = -1)]
	public class GeographyUnionAggregate : IBinarySerialize
	{
		private SqlGeography aggregate;
		
		public void Init()
		{
			aggregate = new SqlGeography();
		}

		public void Accumulate(SqlGeography geography, double bufferDistance)
		{
			aggregate = aggregate.STUnion(geography.STBuffer(bufferDistance));
		}

		public void Merge(GeographyUnionAggregate group)
		{
			aggregate = aggregate.STUnion(group.aggregate);
		}

		public SqlGeography Terminate()
		{
			return aggregate;
		}

		public void Read(BinaryReader r)
		{
			aggregate = new SqlGeography();
			aggregate.Read(r);
		}

		public void Write(BinaryWriter w)
		{
			aggregate.Write(w);
		}
	}
}