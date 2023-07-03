using MarsTS.Players;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units.Cache {

	public class UnitCache : MonoBehaviour {

		private static UnitCache instance;

		private Dictionary<string, Dictionary<int, Unit>> instanceMap;

		public static int Count {
			get {
				int output = 0;

				foreach (Dictionary<int, Unit> map in instance.instanceMap.Values) {
					output += map.Count;
				}

				return output;
			}
		}

		public Unit this[string name] {
			get {
				string[] split = name.Split(':');
				string type = split[0];
				string id = split[1];

				Dictionary<int, Unit> map = instance.GetMap(type);

				return map[int.Parse(id)];
			}
		}

		[SerializeField]
		private Unit[] startingUnits;

		private void Awake () {
			instance = this;
			instanceMap = new Dictionary<string, Dictionary<int, Unit>>();

			foreach (Unit unit in startingUnits) {
				int id = RegisterUnit(unit);
				unit.Init(id, Player.Main);
			}
		}

		public static Unit CreateInstance (GameObject prefab, Player owner, Vector3 position) {
			GameObject newInstance = Instantiate(prefab, position, Quaternion.Euler(0, 0, 0));

			if (newInstance.TryGetComponent(out Unit unit) ) {
				int id = instance.RegisterUnit(unit);
				unit.Init(id, owner);
				return unit;
			}
			else {
				Destroy(newInstance);
				throw new ArgumentException("Prefab " + prefab.name + " not a Unit! Cannot construct, destroying prefab");
			}
		}

		public static bool TryGet (string name, out Unit unit) {
			string[] split = name.Split(':');

			if (!(split.Length > 1)) {
				Debug.LogWarning("Registered instance " + name + " not found!");
				unit = null;
				return false;
			}

			string type = split[0];
			string id = split[1];

			if (instance.instanceMap.TryGetValue(type, out Dictionary<int, Unit> idMap) && idMap.TryGetValue(int.Parse(id), out Unit found)) {
				unit = found;
				return true;
			}
			else {
				Debug.LogWarning("Registered instance " + name + " not found!");
				unit = null;
				return false;
			}
		}

		//Returns -1 for an unsuccessful register
		private int RegisterUnit (Unit unit) {
			Dictionary<int, Unit> map = GetMap(unit.Type());
			int index = Count + 1;
			return map.TryAdd(index, unit) ? index : -1;
		}

		private Dictionary<int, Unit> GetMap (string key) {
			Dictionary<int, Unit> map = instanceMap.GetValueOrDefault(key, new Dictionary<int, Unit>());
			if (!instanceMap.ContainsKey(key)) instanceMap.Add(key, map);
			return map;
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}