using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

    public class WorldObjectRegistry : PrefabRegistry<ISelectable> {

        private static WorldObjectRegistry instance;

		public override string Key => "world_object";

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

		public override ISelectable GetRegistryEntry (string key) {
			if (registeredClasses.TryGetValue(key, out ISelectable component)) {
				return component;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public static ISelectable Selectable (string key) {
			return instance.GetRegistryEntry(key);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}