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

		public override bool GetRegistryEntry<T> (string key, out T output) {
			if (registeredClasses.TryGetValue(key, out Building entry) && entry is T superType) {
				output = superType;
				return true;
			}
			else throw new ArgumentException("Building " + key + " not registered!");
		}

		public static Building Building (string key) {
			instance.GetRegistryEntry<Building>(key, out Building output);
			return output;
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}