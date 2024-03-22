using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandRegistry : MonoBehaviour {

		private static CommandRegistry instance;

        private Dictionary<string, CommandFactory> registered;

		private void Awake () {
			instance = this;
			registered = new Dictionary<string, CommandFactory>();

			foreach (CommandFactory entry in GetComponentsInChildren<CommandFactory>()) {
				registered.Add(entry.Name, entry);
			}
		}

		public static T Get<T> (string key) where T : CommandFactory  {
			if (instance.registered.TryGetValue(key, out CommandFactory entry)) {
				if (entry is T) return entry as T;
			}

			throw new ArgumentException("Command " + key + " of type " + typeof(T) + " not found");
		}

		public static CommandFactory Get (string key) {
			if (instance.registered.TryGetValue(key, out CommandFactory entry)) {
				return entry;
			}

			throw new ArgumentException("Command " + key + " not found");
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}