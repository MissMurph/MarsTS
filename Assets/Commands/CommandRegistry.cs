using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandRegistry : MonoBehaviour {

		private static CommandRegistry instance;

        public Command[] toRegister;

        private Dictionary<string, Command> registered;

		private void Awake () {
			instance = this;
			registered = new Dictionary<string, Command>();

			foreach (Command entry in toRegister) {
				registered.Add(entry.Name, entry);
			}
		}

		public static T Get<T> (string key) where T : Command  {
			if (instance.registered.TryGetValue(key, out Command entry)) {
				if (entry is T) return entry as T;
			}

			throw new ArgumentException("Command " + key + " of type " + typeof(T) + " not found");
		}

		public static Command Get (string key) {
			if (instance.registered.TryGetValue(key, out Command entry)) {
				return entry;
			}

			throw new ArgumentException("Command " + key + " not found");
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}