using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ratworx.MarsTS.Commands {

    public class CommandSerializers : MonoBehaviour {

        private static CommandSerializers instance;

        private Dictionary<string, ICommandSerializer> registered;

		private void Awake () {
			instance = this;
			registered = new Dictionary<string, ICommandSerializer>();
		}

		private void Start () {
			foreach (ICommandSerializer foundSerializer in GetComponentsInChildren<ICommandSerializer>()) {
				registered[foundSerializer.Key] = foundSerializer;
			}
		}

		public static ICommandSerializer GetSerializer (string key) {
			if (instance == null) return null;
			
			if (instance.registered.TryGetValue(key, out ICommandSerializer foundSerializer)) {
				return foundSerializer;
			}

			throw new ArgumentException("Serializer " + key + " not found, has it been registered?");
		}

		public static ISerializedCommand Read (string key) {
			if (instance == null) return null;

			if (instance.registered.TryGetValue(key, out ICommandSerializer foundSerializer)) {
				return foundSerializer.Reader();
			}

			throw new ArgumentException("Serializer " + key + " not found, has it been registered?");
		}

		public static ISerializedCommand Write (Commandlet data) {
			if (instance == null) return null;

			//Debug.Log(data.Key);

			if (instance.registered.TryGetValue(data.SerializerKey, out ICommandSerializer foundSerializer)) {
				return foundSerializer.Writer(data);
			}

			throw new ArgumentException("Serializer " + data.SerializerKey + " not found, has it been registered?");
		}

		public static ISerializedCommand Write(string key, Commandlet data)
		{
			if (instance == null) return null;

			if (instance.registered.TryGetValue(key, out ICommandSerializer foundSerializer)) {
				return foundSerializer.Writer(data);
			}
			
			throw new ArgumentException("Serializer " + data.SerializerKey + " not found, has it been registered?");
		}

		public void OnDestroy () {
			instance = null;
		}
	}
}