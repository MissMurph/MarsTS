using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Entities {

	public class EntityCache : MonoBehaviour {

		private static EntityCache instance;

		private Dictionary<string, Dictionary<int, Entity>> instanceMap;

		public static int Count {
			get {
				int output = 0;

				foreach (Dictionary<int, Entity> map in instance.instanceMap.Values) {
					output += map.Count;
				}

				return output;
			}
		}

		public Entity this[string name] {
			get {
				string[] split = name.Split(':');
				string type = split[0];
				string id = split[1];

				Dictionary<int, Entity> map = instance.GetMap(type);

				return map[int.Parse(id)];
			}
		}

		private void Awake () {
			instance = this;
			instanceMap = new Dictionary<string, Dictionary<int, Entity>>();
		}

		private void Start () {
			EventBus.AddListener<EntityDestroyEvent>(OnEntityDestroyed);
		}

		public static bool TryGet (string name, out Entity output) {
			string[] split = name.Split(':');

			if (!(split.Length > 1)) {
				//Debug.LogWarning("Registered instance " + name + " not found!");
				output = null;
				return false;
			}

			string type = split[0];
			string id = split[1];

			if (instance.instanceMap.TryGetValue(type, out Dictionary<int, Entity> idMap) && idMap.TryGetValue(int.Parse(id), out Entity found)) {
				output = found;
				return true;
			}
			else {
				//Debug.LogWarning("Registered instance " + name + " not found!");
				output = null;
				return false;
			}
		}

		//Enter name like the below:
		//name:instanceID:componentKey
		//tank:236:unit
		public static bool TryGet<T> (string name, out T output) {
			if (instance == null) {
				output = default(T);
				return false;
			}

			string[] split = name.Split(':');

			if (!(split.Length > 1)) {
				//Debug.LogWarning("Registered instance " + name + " not found!");
				output = default(T);
				return false;
			}

			if (TryGet(split[0] + ":" + split[1], out Entity entityComponent)) {
				if (split.Length > 2 && entityComponent.TryGet(split[2], out T superType)) {
					output = superType;
					return true;
				}
				else if (entityComponent.TryGet<T>(out T foundType)) {
					output = foundType;
					return true;
				}
			}
			
			output = default(T);
			return false;
		}

		//Returns -1 for an unsuccessful register
		public static int Register (Entity entity) 
		{
			int id = Count + 1;
			
			if (entity.Id > 0)
			{
				if (!NetworkManager.Singleton.IsServer)
					id = entity.Id;
				else
				{
					Debug.LogError($"Attempting to register already registered entity {entity.Key}:{entity.Id}!");
					return -1;
				}
			}
			
			Dictionary<int, Entity> map = instance.GetMap(entity.Key);
			return map.TryAdd(id, entity) ? id : -1;
		}

		private Dictionary<int, Entity> GetMap (string key) {
			Dictionary<int, Entity> map = instanceMap.GetValueOrDefault(key, new Dictionary<int, Entity>());
			if (!instanceMap.ContainsKey(key)) instanceMap.Add(key, map);
			return map;
		}

		private void OnEntityDestroyed (EntityDestroyEvent _event) {
			if (instance == null) return;
			if (TryGet(_event.Entity.gameObject.name, out Entity found)) {
				Dictionary<int, Entity> map = GetMap(found.Key);
				map.Remove(found.Id);
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}