using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class StopSerializer : MonoBehaviour, ICommandSerializer {
		public string Key { get { return commandKey; } }

		[SerializeField]
		private string commandKey;

		public ISerializedCommand Reader () {
			return new SerializedStopCommandlet {
				Key = Key
			};
		}

		public ISerializedCommand Writer (Commandlet _data) {
			return new SerializedStopCommandlet {
				Key = Key,
				Id = _data.Id,
				Faction = _data.Commander.ID
			};
		}
	}

	public struct SerializedStopCommandlet : ISerializedCommand 
	{
		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		//This is pretty much empty as Stop is such a simple command
		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			
		}
	}
}