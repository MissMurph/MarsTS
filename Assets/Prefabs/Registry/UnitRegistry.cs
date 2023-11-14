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

		public override Unit GetRegistryEntry (string key) {
			if (registeredClasses.TryGetValue(key, out Unit component)) {
				return component.Get();
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public static Unit Unit (string key) {
			return instance.GetRegistryEntry(key);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}