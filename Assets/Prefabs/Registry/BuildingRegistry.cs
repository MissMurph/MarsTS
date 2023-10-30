using MarsTS.Buildings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public class BuildingRegistry : PrefabRegistry<Building> {

		private static BuildingRegistry instance;

		protected override void Awake () {
			base.Awake();

			instance = this;
		}

		public static GameObject Prefab (string key) {
			if (instance.registeredPrefabs.TryGetValue(key, out GameObject prefab)) {
				return prefab;
			}
			else throw new ArgumentException("Building " + key + " not registered!");
		}

		public static Building Building (string key) {
			if (instance.registeredClasses.TryGetValue(key, out Building component)) {
				return component;
			}
			else throw new ArgumentException("Building " + key + " not registered!");
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}