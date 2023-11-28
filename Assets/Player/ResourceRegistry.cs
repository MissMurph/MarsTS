using MarsTS.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class ResourceRegistry : MonoBehaviour {

		private static ResourceRegistry instance;

		[SerializeField]
		private ResourceEntry[] toRegister;

		private Dictionary<string, ResourceEntry> registered;

		private void Awake () {
			instance = this;
			registered = new Dictionary<string, ResourceEntry>();

			foreach (ResourceEntry entry in toRegister) {
				registered.Add(entry.Name, entry);
			}
		}

		public static ResourceEntry Get (string key) {
			if (instance.registered.TryGetValue(key, out ResourceEntry entry)) {
				return entry;
			}

			throw new ArgumentException("Resource " + key + " not found");
		}

		private void OnDestroy () {
			instance = null;
		}
	}

	[Serializable]
	public class ResourceEntry {
		public string Name;
		public Sprite Icon;
	}
}