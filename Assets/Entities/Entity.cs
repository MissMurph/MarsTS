using MarsTS.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

		private Dictionary<string, ITaggable> registeredTaggables;
		private Dictionary<string, Component> taggedComponents;

		private EventAgent eventAgent;

		[SerializeField]
		private TagReference[] toTag;

		private void Awake () {
			eventAgent = GetComponent<EventAgent>();

			//eventAgent.AddListener<EventAgentInitEvent>(Init);

			registeredTaggables = new Dictionary<string, ITaggable>();
			taggedComponents = new Dictionary<string, Component>();

			foreach (ITaggable component in GetComponents<ITaggable>()) {
				registeredTaggables[component.Key] = component;
			}

			if (TryGetComponent(out NetworkObject found)) taggedComponents["networking"] = found;

			foreach (TagReference entry in toTag) {
				taggedComponents[entry.Tag] = entry.Component;
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
			if (registeredTaggables.TryGetValue(key, out ITaggable component) && component is T superType) {
				output = superType;
				return true;
			}

			if (typeof(T) == typeof(Component) && taggedComponents.TryGetValue(key, out Component found) && component is T superTypedComponent) {
				output = superTypedComponent;
				return true;
			}

			output = default(T);
			return false;
		}

		public bool TryGet<T> (out T output) {
			foreach (ITaggable taggableComponent in registeredTaggables.Values) {
				if (taggableComponent is T superType) {
					output = superType;
					return true;
				}
			}

			if (typeof(T) == typeof(Component)) {
				foreach (Component nonTaggableComponent in taggedComponents.Values) {
					if (nonTaggableComponent is T superTypedComponent) {
						output = superTypedComponent;
						return true;
					}
				}
			}

			output = default;
			return false;
		}

		public T Get<T> (string key) {
			if (registeredTaggables.TryGetValue(key, out ITaggable component) && component is T superType) {
				return superType;
			}

			if (typeof(T) == typeof(Component) && taggedComponents.TryGetValue(key, out Component found) && component is T superTypedComponent) {
				return superTypedComponent;
			}

			return default;
		}

		private void OnDestroy () {
			eventAgent.Global(new EntityDestroyEvent(eventAgent, this));
		}
	}

	[Serializable]
	public class TagReference {
		public string Tag;
		public Component Component;
	}
}