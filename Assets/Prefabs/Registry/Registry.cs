using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

namespace MarsTS.Prefabs {

    public class Registry : MonoBehaviour {
        
        private static Registry instance;

		private Dictionary<string, PrefabRegistry> registries;

		private void Awake () {
			instance = this;

			registries = new Dictionary<string, PrefabRegistry>();

			foreach (PrefabRegistry registryComp in GetComponentsInChildren<PrefabRegistry>()) {
				registries[registryComp.Key] = registryComp;
			}
		}

		public static T Get<T> (string key) {
			string[] split = key.Split(':');

			if (!(split.Length > 1)) {
				Debug.LogWarning("Registry Object " + key + " not found!");
				return default;
			}

			string registryType = split[0];
			string objectType = split[1];

			if (instance.registries.TryGetValue(registryType, out PrefabRegistry abstactRegistry)
				&& abstactRegistry.GetRegistryEntry<T>(objectType, out T output)) {
				return output;
			}

			Debug.LogWarning("Registry Object " + key + " not found!");
			return default;
		}

		public static GameObject Prefab (string key) {
			string[] split = key.Split(':');

			if (!(split.Length > 1)) {
				Debug.LogWarning("Registry Object " + key + " not found!");
				return default;
			}

			string registryType = split[0];
			string objectType = split[1];

			if (instance.registries.TryGetValue(registryType, out PrefabRegistry abstactRegistry)) {
				return abstactRegistry.GetPrefab(objectType);
			}

			Debug.LogWarning("Registry Object " + key + " not found!");
			return default;
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}