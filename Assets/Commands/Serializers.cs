using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Serializers : MonoBehaviour {

        private static Serializers instance;

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

			//Debug.Log(key);

			if (instance.registered.TryGetValue(key, out ICommandSerializer foundSerializer)) {
				return foundSerializer.Reader();
			}

			throw new ArgumentException("Serializer " + key + " not found, has it been registered?");
		}

		public static ISerializedCommand Write (Commandlet data) {
			if (instance == null) return null;

			//Debug.Log(data.Key);

			if (instance.registered.TryGetValue(data.Key, out ICommandSerializer foundSerializer)) {
				return foundSerializer.Writer(data);
			}

			throw new ArgumentException("Serializer " + data.Key + " not found, has it been registered?");
		}

		public void OnDestroy () {
			instance = null;
		}
	}
}