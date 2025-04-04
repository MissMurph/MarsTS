using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Init;
using UnityEngine;
using UnityEngine.Events;

namespace Ratworx.MarsTS.Events {

	public class EventAgent : MonoBehaviour, IEntityComponent<EventAgent> {

		private Dictionary<Type, UnityEventBase> listeners = new Dictionary<Type, UnityEventBase>();

		//This will return 0 if the agent isn't registered
		public int Id => id;

		public string Key => "eventAgent";

		public Type Type => typeof(EventAgent);

		private int id = 0;

		private void Awake () {
			id = EventBus.RegisterAgent(this);
			Local(new EventAgentInitEvent(this));
		}

		public void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			UnityEvent<T> _event = (listeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;
			if (!listeners.ContainsKey(typeof(T))) listeners.Add(typeof(T), _event);
			_event.AddListener(func);
		}

		public void RemoveListener<T> (UnityAction<T> func) where T : AbstractEvent {
			if (!listeners.ContainsKey(typeof(T))) return;

			UnityEvent<T> _event = (listeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;

			_event.RemoveListener(func);
		}

		public T Local<T>(T postedEvent) where T : AbstractEvent {
			if (listeners.TryGetValue(typeof(T), out UnityEventBase value) && value is UnityEvent<T> superTypeEvent) {
				superTypeEvent.Invoke(postedEvent);
			}

			return postedEvent;
		}

		public T Global<T> (T postedEvent) where T : AbstractEvent {
			if (listeners.TryGetValue(typeof(T), out UnityEventBase value) && value is UnityEvent<T> superTypeEvent) {
				superTypeEvent.Invoke(postedEvent);
			}

			EventBus.Global(postedEvent);

			return postedEvent;
		}

		public EventAgent Get () {
			return this;
		}
	}
}