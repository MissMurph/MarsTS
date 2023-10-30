using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MarsTS.Entities {

	[RequireComponent(typeof(EventAgent))]
	public class Entity : MonoBehaviour {

		public int ID {
			get {
				return id;
			}
		}

		public string Key {
			get {
				return registryKey;
			}
		}

		private int id;

		[SerializeField]
		private string registryKey;

		private Dictionary<string, ITaggable> taggedComponents;

		private EventAgent eventAgent;

		private void Awake () {
			eventAgent = GetComponent<EventAgent>();

			taggedComponents = new Dictionary<string, ITaggable> ();

			foreach (ITaggable component in GetComponents<ITaggable>()) {
				taggedComponents[component.Key] = component;
			}
		}

		private void Start () {
			id = EntityCache.Register(this);
			name = registryKey + ":" + id.ToString();
			eventAgent.Local(new EntityInitEvent(this, eventAgent));
		}

		public bool TryGet<T> (string key, out T output) {
			if (taggedComponents.TryGetValue(key, out ITaggable component) && component is T superType) {
				output = superType;
				return true;
			}

			output = default(T);
			return false;
		}

		public bool TryGet<T> (out T output) {
			foreach (ITaggable component in taggedComponents.Values) {
				if (component is T superType) {
					output = superType;
					return true;
				}
			}

			output = default;
			return false;
		}
	}
}