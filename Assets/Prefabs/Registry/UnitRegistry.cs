using MarsTS.Prefabs;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs {

	public class UnitRegistry : PrefabRegistry<Unit> {

		private static UnitRegistry instance;

		protected override void Awake () {
			base.Awake();

			instance = this;
		}

		public static GameObject Prefab (string key) {
			if (instance.registeredPrefabs.TryGetValue(key, out GameObject prefab)) {
				return prefab;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		public static Unit Unit (string key) {
			if (instance.registeredClasses.TryGetValue(key, out Unit component)) {
				return component;
			}
			else throw new ArgumentException("Unit " + key + " not registered!");
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}