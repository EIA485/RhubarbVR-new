﻿using System.Runtime.CompilerServices;

using MessagePack;

using SharedModels;
using SharedModels.GameSpecific;

using StereoKit;

namespace RhuEngine.DataStructure
{
	[MessagePackObject]
	public class DataNode<T> : IDataNode
	{
		public DataNode(T def = default) {
			Value = def;
		}

		public DataNode() {
			Value = default;
		}
		[Key(0)]
		public T Value { get; set; }

		public byte[] GetByteArray() {
			return Serializer.Save(this);
		}

		public void SetByteArray(byte[] arrBytes) {
			Value = Serializer.Read<DataNode<T>>(arrBytes).Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(DataNode<T> data) => data.Value;
	}
}
