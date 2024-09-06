using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class BoolSerializer : MonoBehaviour, ICommandSerializer {
		public string Key => commandKey;

		[SerializeField]
		private string commandKey;

		public ISerializedCommand Reader () {
			return new SerializedBoolCommandlet {
				Key = Key
			};
		}

		public ISerializedCommand Writer (Commandlet _data) {
			return new SerializedBoolCommandlet {
				Key = Key,
				Id = _data.Id,
				Faction = _data.Commander.Id
			};
		}
	}

	public struct SerializedBoolCommandlet : ISerializedCommand 
	{
		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		//This is empty as Stop is such a simple command
		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter { }
	}
}