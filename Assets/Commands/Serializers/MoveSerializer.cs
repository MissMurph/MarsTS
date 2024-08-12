using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class MoveSerializer : MonoBehaviour, ICommandSerializer {

		public string Key { get { return commandKey; } }

		[SerializeField]
		private string commandKey;

		public ISerializedCommand Reader () {
			return new SerializedMoveCommandlet {
				Key = Key
			};
		}

		public ISerializedCommand Writer (Commandlet _data) {
			MoveCommandlet superType = _data as MoveCommandlet;

			return new SerializedMoveCommandlet {
				Key = Key,
				Faction = superType.Commander.Id,
				Id = superType.Id,
				_targetPosition = superType.Target,
			};
		}
	}

	public struct SerializedMoveCommandlet : ISerializedCommand {

		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		public Vector3 _targetPosition;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref _targetPosition);
		}
	}
}