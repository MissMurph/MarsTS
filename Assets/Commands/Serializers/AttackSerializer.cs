using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class AttackSerializer : MonoBehaviour, ICommandSerializer {

		public string Key { get { return commandKey; } }

		[SerializeField]
		private string commandKey;

		public ISerializedCommand Reader () {
			return new SerializedMoveCommandlet {
				Key = Key
			};
		}

		public ISerializedCommand Writer (Commandlet _data) {
			AttackCommandlet superType = _data as AttackCommandlet;

			if (EntityCache.TryGet(superType.Target.GameObject.name, out NetworkObject targetNetworking)) {
				return new SerializedAttackCommandlet {
					Key = Key,
					Faction = superType.Commander.Id,
					Id = superType.Id,
					targetUnit = targetNetworking
				};
			}

			return null;
		}
	}

	public struct SerializedAttackCommandlet : ISerializedCommand {

		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		public NetworkObjectReference targetUnit;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref targetUnit);
		}
	}
}