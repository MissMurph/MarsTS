using MarsTS.Buildings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public class BuildingRegistry : PrefabRegistry<Building> {

		private static BuildingRegistry instance;

		public override string Key => "building";

		protected override void Awake () {
			base.Awake();

			instance = this;
		}

		public override GameObject GetPrefab (string key) {
			if (registeredPrefabs.TryGetValue(key, out GameObject prefab)) {
				return prefab;
			}
			else throw new ArgumentException("Building " + key + " not registered!");
		}

		public static GameObject Prefab (string key) {
			return instance.GetPrefab(key);
		}

		public override Building GetRegistryEntry (string key) {
			if (registeredClasses.TryGetValue(key, out Building entry)) {
				return entry.Get();
			}
			else throw new ArgumentException("Building " + key + " not registered!");
		}

		public static Building Building (string key) {
			return instance.GetRegistryEntry(key);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}