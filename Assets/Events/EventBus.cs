using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarsTS.Events {

	public class EventBus : MonoBehaviour {

		private static EventBus instance;

		private Dictionary<Type, UnityEventBase> globalListeners;

		private Dictionary<int, Agent> registeredAgents;

		private Dictionary<string, int> nameAtlas;

		private Dictionary<int, List<UnityAction>> localListeners;

		private int currentId;

		private void Awake () {
			instance = this;
			currentId = 0;
			globalListeners = new Dictionary<Type, UnityEventBase>();
			registeredAgents = new Dictionary<int, Agent>();
			nameAtlas = new Dictionary<string, int>();
			localListeners = new Dictionary<int, List<UnityAction>>();
		}

		//Fires events to all listeners registered for this event type
		public static T Global<T> (T postedEvent) where T : AbstractEvent {
			if (!instance.registeredAgents.ContainsKey(postedEvent.Source.Id)) throw new ArgumentException("Event " + postedEvent.Name + " fired from unregistered agent " + postedEvent.Source.Id + " on object " + postedEvent.Source.name);
			if (instance.globalListeners.TryGetValue(typeof(T), out UnityEventBase value)) {
				UnityEvent<T> superType = (UnityEvent<T>)value;
				superType.Invoke(postedEvent);
			}

			return postedEvent;
		}

		//Fires events to all listeners of this event type registered to this event's source Agent ID
		public static T Local<T> (T postedEvent) where T : AbstractEvent {
			if (!instance.registeredAgents.ContainsKey(postedEvent.Source.Id)) throw new ArgumentException("Event " + postedEvent.Name + " fired from unregistered agent " + postedEvent.Source.Id + " on object " + postedEvent.Source.name);
			if (instance.localListeners.TryGetValue(postedEvent.Source.Id, out List<UnityAction> listeners)) {
				foreach (UnityAction listener in listeners) {
					if (listener.GetType().Equals(typeof(UnityAction<T>))) {
						(listener as UnityAction<T>).Invoke(postedEvent);
					}
				}
			}

			return postedEvent;
		}

		//Global Subscription
		public static void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			UnityEvent<T> _event = (instance.globalListeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;
			if (!instance.globalListeners.ContainsKey(typeof(T))) instance.globalListeners.Add(typeof(T), _event);
			_event.AddListener(func);
		}

		//Local Subscription
		public static void AddListener<T> (UnityAction<T> func, int agentID) where T : AbstractEvent {
			List<UnityAction> listenerMap = instance.localListeners.GetValueOrDefault(agentID, new List<UnityAction>());
			if (!instance.localListeners.ContainsKey(agentID)) instance.localListeners.Add(agentID, listenerMap);
			listenerMap.Add(func as UnityAction);
		}

		public static void RegisterAgent (Action<int> idCallback, Agent source) {
			if (source.Id != 0) throw new ArgumentException("Agent " + source.Id + " already registered with Event Bus " + instance.name);

			int id = instance.currentId++;
			idCallback(id);

			instance.registeredAgents[id] = source;
			if (!instance.localListeners.ContainsKey(id)) instance.localListeners[id] = new List<UnityAction>();
			instance.nameAtlas[source.name] = id;
		}

		public static Agent Agent (string name) {
			if (instance.nameAtlas.TryGetValue(name, out int id)) {
				return instance.registeredAgents[id];
			}
			else throw new ArgumentException("Agent " + name + " not registered with Event Bus");
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}