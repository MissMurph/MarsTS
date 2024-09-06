using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarsTS.Events {

	public class EventBus {

		private static EventBus instance;

		private Dictionary<Type, UnityEventBase> globalListeners;

		private Dictionary<int, EventAgent> registeredAgents;

		private Dictionary<string, int> nameAtlas;

		private int currentId;

		private static bool isInitialized => instance != null;

		private EventBus() {
			currentId = 0;
			globalListeners = new Dictionary<Type, UnityEventBase>();
			registeredAgents = new Dictionary<int, EventAgent>();
			nameAtlas = new Dictionary<string, int>();
		}

		private static void Init () {
			instance = new EventBus();
			
			AddListener<EntityDestroyEvent>(OnEntityDeath);
		}

		//Fires events to all listeners registered for this event type
		public static T Global<T> (T postedEvent) where T : AbstractEvent {
			if (!isInitialized) Init();
			
			if (!instance.registeredAgents.ContainsKey(postedEvent.Source.ID)) throw new ArgumentException("Event " + postedEvent.Name + " fired from unregistered agent " + postedEvent.Source.ID + " on object " + postedEvent.Source.name);
			if (instance.globalListeners.TryGetValue(typeof(T), out UnityEventBase value)) {
				UnityEvent<T> superType = (UnityEvent<T>)value;
				superType.Invoke(postedEvent);
			}

			return postedEvent;
		}

		//Fires events to all listeners of this event type registered to this event's source Agent ID
		/*public static T Local<T> (T postedEvent) where T : AbstractEvent {
			if (!instance.registeredAgents.ContainsKey(postedEvent.Source.Id)) throw new ArgumentException("Event " + postedEvent.Name + " fired from unregistered agent " + postedEvent.Source.Id + " on object " + postedEvent.Source.name);
			if (instance.registeredAgents.TryGetValue(postedEvent.Source.Id, out List<UnityAction> listeners)) {
				foreach (UnityAction listener in listeners) {
					if (listener.GetType().Equals(typeof(UnityAction<T>))) {
						(listener as UnityAction<T>).Invoke(postedEvent);
					}
				}
			}

			return postedEvent;
		}*/

		//Global Subscription
		public static void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			if (!isInitialized) Init();
			
			UnityEvent<T> _event = (instance.globalListeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;
			if (!instance.globalListeners.ContainsKey(typeof(T))) instance.globalListeners.Add(typeof(T), _event);
			_event.AddListener(func);
		}

		//Local Subscription
		public static void AddListener<T> (UnityAction<T> func, int agentID) where T : AbstractEvent {
			if (!isInitialized) Init();
			
			if (instance.registeredAgents.TryGetValue(agentID, out EventAgent agent)) {
				agent.AddListener(func);
				return;
			}
			else throw new ArgumentException("Agent " + agentID + " not registered with Event Bus, cannot Subscribe to local events!");
		}

		public static void RemoveListener<T> (UnityAction<T> func) where T : AbstractEvent {
			if (!isInitialized) Init();
			
			if (instance.globalListeners.TryGetValue(typeof(T), out UnityEventBase _event)) {
				UnityEvent<T> deserialized = _event as UnityEvent<T>;

				deserialized.RemoveListener(func);
				return;
			}
		}

		public static int RegisterAgent (EventAgent source)
		{
			if (!isInitialized) Init();
			
			if (source.ID != 0) throw new ArgumentException("Agent " + source.ID + " already registered with Event Bus");
			int id = instance.currentId++;

			instance.registeredAgents[id] = source;
			//if (!instance.localListeners.ContainsKey(id)) instance.localListeners[id] = new List<UnityAction>();
			instance.nameAtlas[source.name] = id;

			return id;
		}

		public static EventAgent Agent (string name) {
			if (instance.nameAtlas.TryGetValue(name, out int id)) {
				return instance.registeredAgents[id];
			}
			else throw new ArgumentException("Agent " + name + " not registered with Event Bus");
		}

		private static void OnEntityDeath (EntityDestroyEvent _event) {
			instance.registeredAgents.Remove(_event.Source.ID);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}