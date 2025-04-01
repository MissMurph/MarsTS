using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities {

	public class EntityCache : MonoBehaviour {

		private static EntityCache _instance;

		private Dictionary<string, Dictionary<int, Entity>> _instanceMap;

		/*public static int Count {
			get {
				int output = 0;

				foreach (Dictionary<int, Entity> map in _instance._instanceMap.Values) {
					output += map.Count;
				}

				return output;
			}
		}*/

		private static int _count = 0;

		public Entity this[string name] {
			get {
				string[] split = name.Split(':');
				string type = split[0];
				string id = split[1];

				Dictionary<int, Entity> map = _instance.GetMap(type);

				return map[int.Parse(id)];
			}
		}

		private void Awake () {
			_instance = this;
			_instanceMap = new Dictionary<string, Dictionary<int, Entity>>();
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

			if (_instance._instanceMap.TryGetValue(type, out Dictionary<int, Entity> idMap) && idMap.TryGetValue(int.Parse(id), out Entity found)) {
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
			if (_instance == null) {
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
		public static int Register (Entity entity) {
			_count++;
			
			int id = _count;
			
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
			
			Dictionary<int, Entity> map = _instance.GetMap(entity.Key);
			return map.TryAdd(id, entity) ? id : -1;
		}

		private Dictionary<int, Entity> GetMap (string key) {
			Dictionary<int, Entity> map = _instanceMap.GetValueOrDefault(key, new Dictionary<int, Entity>());
			if (!_instanceMap.ContainsKey(key)) _instanceMap.Add(key, map);
			return map;
		}

		private void OnEntityDestroyed (EntityDestroyEvent _event) {
			if (_instance == null) return;
			if (TryGet(_event.Entity.gameObject.name, out Entity found)) {
				Dictionary<int, Entity> map = GetMap(found.Key);
				map.Remove(found.Id);
			}
		}

		private void OnDestroy () {
			_instance = null;
		}
	}
}