using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public class Registry : MonoBehaviour {
        
        private static Registry instance;

		private Dictionary<Type, PrefabRegistry> registries;

		private void Awake () {
			instance = this;

			registries = new Dictionary<Type, PrefabRegistry>();

			foreach (PrefabRegistry registryComp in GetComponentsInChildren<PrefabRegistry>()) {
				registries[registryComp.RegistryType] = registryComp;
			}
		}



		private void OnDestroy () {
			instance = null;
		}
	}
}