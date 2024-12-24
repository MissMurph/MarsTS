using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Commands {

	public class BoolSerializer : MonoBehaviour, ICommandSerializer
	{
		public string Key => _commandKey;

		[SerializeField]
		private string _commandKey;

		public ISerializedCommand Reader () {
			return new SerializedBoolCommandlet {
				Key = Key
			};
		}

		public ISerializedCommand Writer (Commandlet data)
		{
			var superType = data as Commandlet<bool>;
			
			return new SerializedBoolCommandlet {
				Key = Key,
				Id = data.Id,
				Faction = data.Commander.Id,
				Status = superType.Target,
			};
		}
	}

	public struct SerializedBoolCommandlet : ISerializedCommand 
	{
		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		public bool Status;
		
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Status);
		}
	}
}