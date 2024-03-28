using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public struct SerializedCommandWrapper : INetworkSerializable {

		public ISerializedCommand commandletData;

		public string Key;
		public int Faction;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			//Debug.Log("Serializer Running on " + (NetworkManager.Singleton.IsServer) + " and data is not null: " + (commandletData != null));

			if (serializer.IsWriter) {
				Key = commandletData.Key;
				Faction = commandletData.Faction;
			}

			serializer.SerializeValue(ref Key);
			serializer.SerializeValue(ref Faction);

			if (serializer.IsReader) {
				commandletData = Serializers.Read(Key);
			}
			
			commandletData.NetworkSerialize(serializer);
		}
	}
}