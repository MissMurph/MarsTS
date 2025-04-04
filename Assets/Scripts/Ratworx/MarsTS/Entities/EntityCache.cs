using System.Collections;
using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities {

	public class EntityCache : MonoBehaviour, IEnumerable<Entity> {

		private static EntityCache _instance;

		private Dictionary<int, Entity> _instanceMap;

		private static int _count = 0;

		public Entity this[string name] {
			get {
				string[] split = name.Split(':');
				string id = split[1];
				
				return _instanceMap[int.Parse(id)];
			}
		}

		private void Awake () {
			_instance = this;
			_instanceMap = new Dictionary<int, Entity>();
		}

		private void Start () {
			EventBus.AddListener<EntityDestroyEvent>(OnEntityDestroyed);
		}

		public static bool TryGetEntity (string name, out Entity output) {
			string[] split = name.Split(':');

			if (!(split.Length > 1)) {
				output = null;
				return false;
			}

			string id = split[1];

			if (_instance._instanceMap.TryGetValue(int.Parse(id), out Entity found)) {
				output = found;
				return true;
			}

			output = null;
			return false;
		}

		/// <example>
		/// <code>
		/// name:instanceID:componentKey
		/// tank:236:health
		/// </code>
		/// </example>
		public static bool TryGetEntityComponent<T> (string name, out T output) {
			if (_instance == null) {
				output = default(T);
				return false;
			}

			string[] split = name.Split(':');

			if (!(split.Length > 1)) {
				output = default(T);
				return false;
			}

			if (TryGetEntity(split[0] + ":" + split[1], out Entity entityComponent)) {
				if (split.Length > 2 && entityComponent.TryGetEntityComponent(split[2], out T superType)) {
					output = superType;
					return true;
				}
				else if (entityComponent.TryGetEntityComponent<T>(out T foundType)) {
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
					Debug.LogError($"Attempting to register already registered entity {entity.RegistryKey}:{entity.Id}!");
					return -1;
				}
			}
			
			return _instance._instanceMap.TryAdd(id, entity) ? id : -1;
		}

		private void OnEntityDestroyed (EntityDestroyEvent _event) {
			if (_instance == null) return;
			if (TryGetEntity(_event.Entity.gameObject.name, out Entity found)) {
				_instanceMap.Remove(found.Id);
			}
		}

		private void OnDestroy () {
			_instance = null;
		}

		public IEnumerator<Entity> GetEnumerator() => _instanceMap.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}