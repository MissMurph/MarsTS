using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public abstract class PrefabRegistry<T> : PrefabRegistry where T : MonoBehaviour {
		
		protected Dictionary<string, T> registeredClasses;

		public override Type RegistryType {
			get { 
				return typeof(T); 
			}
		}

		protected override void Awake () {
			base.Awake();

			registeredClasses = new Dictionary<string, T>();
		}

		protected override void Register (string key, GameObject prefab) {
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

	public abstract class PrefabRegistry : MonoBehaviour {
		protected Dictionary<string, GameObject> registeredPrefabs;

		[SerializeField]
		protected GameObject[] prefabsToRegister;

		public abstract Type RegistryType {
			get;
		}

		protected virtual void Awake () {
			registeredPrefabs = new Dictionary<string, GameObject>();

			foreach (GameObject prefab in prefabsToRegister) {
				Register(prefab.name, prefab);
			}
		}

		protected abstract void Register (string key, GameObject prefab);
	}
}