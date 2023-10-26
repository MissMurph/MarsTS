using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class UnitRegistry : MonoBehaviour {

		private static UnitRegistry instance;

		private Dictionary<string, GameObject> registeredPrefabs;
		private Dictionary<string, Unit> registeredUnits;

		[SerializeField]
		private GameObject[] prefabsToRegister;

		private void Awake () {
			instance = this;
			registeredPrefabs = new Dictionary<string, GameObject>();
			registeredUnits = new Dictionary<string, Unit>();

			foreach (GameObject prefab in prefabsToRegister) {
				Register(prefab.name, prefab);
			}
		}

		private void Register (string key, GameObject prefab) {
			if (registeredPrefabs.ContainsKey(key)) {
				throw new ArgumentException("Unit " + key + " already registered!");
			}
			else {
				Unit component = prefab.GetComponent<Unit>();
				registeredPrefabs.Add(key, prefab);
				registeredUnits.Add(key, component);
			}
		}

		public static GameObject Prefab (string key) {
			if (instance.registeredPrefabs.TryGetValue(key, out GameObject prefab)) {
				return prefab;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public static Unit Unit (string key) {
			if (instance.registeredUnits.TryGetValue(key, out Unit component)) {
				return component;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}
	}
}