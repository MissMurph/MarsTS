using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public struct SerializedCommandWrapper : INetworkSerializable {

		public ISerializedCommand commandletData;

		// TODO: Convert key to bit codes
		public string Key;
		public int Id;
		public int Faction;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			if (serializer.IsWriter) {
				Key = commandletData.Key;
				Faction = commandletData.Faction;
				Id = commandletData.Id;
			}

			serializer.SerializeValue(ref Key);
			serializer.SerializeValue(ref Faction);
			serializer.SerializeValue(ref Id);

			if (serializer.IsReader) {
				commandletData = CommandSerializers.Read(Key);
			}
			
			commandletData.NetworkSerialize(serializer);
		}
	}
}