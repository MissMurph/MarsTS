using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

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

		private int id = 0;

		[SerializeField]
		private string registryKey;

		private Dictionary<string, ITaggable> taggedComponents;

		private EventAgent eventAgent;

		private void Awake () {
			eventAgent = GetComponent<EventAgent>();

			//eventAgent.AddListener<EventAgentInitEvent>(Init);

			taggedComponents = new Dictionary<string, ITaggable> ();

			foreach (ITaggable component in GetComponents<ITaggable>()) {
				taggedComponents[component.Key] = component;
			}
		}

		private void Update () {
			if (id == 0) {
				id = EntityCache.Register(this);
				name = registryKey + ":" + id.ToString();

				EntityInitEvent initCall = new EntityInitEvent(this, eventAgent);

				initCall.Phase = Phase.Pre;
				eventAgent.Global(initCall);

				initCall.Phase = Phase.Post;
				eventAgent.Global(initCall);
			}
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

		public T Get<T> (string key) {
			if (taggedComponents.TryGetValue(key, out ITaggable component) && component is T superType) {
				return superType;
			}

			return default;
		}

		private void OnDestroy () {
			eventAgent.Global(new EntityDestroyEvent(eventAgent, this));
		}
	}
}