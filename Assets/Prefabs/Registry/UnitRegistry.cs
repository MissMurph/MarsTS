using MarsTS.Prefabs;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

	public class UnitRegistry : PrefabRegistry<Unit> {

		private static UnitRegistry instance;

		public override string Key => "unit";

		protected override void Awake () {
			base.Awake();

			instance = this;
		}

		public static GameObject Prefab (string key) {
			return instance.GetPrefab(key);
		}

		public override GameObject GetPrefab (string key) {
			if (registeredPrefabs.TryGetValue(key, out GameObject prefab)) {
				return prefab;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public override bool GetRegistryEntry<T> (string key, out T output) {
			if (registeredClasses.TryGetValue(key, out Unit component) && component is T superType) {
				output = superType;
				return true;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public static Unit Unit (string key) {
			instance.GetRegistryEntry<Unit>(key, out Unit output);
			return output;
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}