using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public abstract class PrefabRegistry<T> : MonoBehaviour where T : MonoBehaviour {

		protected Dictionary<string, GameObject> registeredPrefabs;
		protected Dictionary<string, T> registeredClasses;

		[SerializeField]
		protected GameObject[] prefabsToRegister;

		protected virtual void Awake () {
			registeredPrefabs = new Dictionary<string, GameObject>();
			registeredClasses = new Dictionary<string, T>();

			foreach (GameObject prefab in prefabsToRegister) {
				Register(prefab.name, prefab);
			}
		}

		protected virtual void Register (string key, GameObject prefab) {
			if (registeredPrefabs.ContainsKey(key)) {
				throw new ArgumentException("Prefab of type " + typeof(T) + " with key " + key + " already registered!");
			}
			else {
				T component = prefab.GetComponent<T>();
				registeredPrefabs.Add(key, prefab);
				registeredClasses.Add(key, component);
			}
		}
	}
}