using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class AttackableSerializer : MonoBehaviour, ICommandSerializer {

		public string Key => commandKey;

		[SerializeField]
		private string commandKey;

		public ISerializedCommand Reader () {
			return new SerializedAttackableCommandlet {
				SerializerKey = Key
			};
		}

		public ISerializedCommand Writer (Commandlet data) {
			if (data is null) {
				Debug.LogError($"Commandlet cannot be serialized by {typeof(AttackableSerializer)}:{Key} because Data is null!");
				return null;
			}
			
			AttackableCommandlet superType = data as AttackableCommandlet;

			return new SerializedAttackableCommandlet {
				Name = data.Name,
				SerializerKey = Key,
				Faction = superType.Commander.Id,
				Id = superType.Id,
				TargetUnit = superType.Target.GameObject.name
			};
		}
	}

	public struct SerializedAttackableCommandlet : ISerializedCommand {
		public string Name { get; set; }
		public string SerializerKey { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		public string TargetUnit;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref TargetUnit);
		}
	}
}