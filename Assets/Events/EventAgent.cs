using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarsTS.Events {

	public class EventAgent : MonoBehaviour {

		private Dictionary<Type, UnityEventBase> listeners = new Dictionary<Type, UnityEventBase>();

		//This will return 0 if the agent isn't registered
		public int ID {
			get {
				return id;
			}
		}

		private int id = 0;

		private void Start () {
			id = EventBus.RegisterAgent(this);
		}

		public void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			UnityEvent<T> _event = (listeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;
			if (!listeners.ContainsKey(typeof(T))) listeners.Add(typeof(T), _event);
			_event.AddListener(func);
		}

		public T Local<T>(T postedEvent) where T : AbstractEvent {
			if (listeners.TryGetValue(typeof(T), out UnityEventBase value) && value is UnityEvent<T> superTypeEvent) {
				superTypeEvent.Invoke(postedEvent);
			}

			return postedEvent;
		}
	}
}