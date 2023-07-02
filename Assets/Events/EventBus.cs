using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MarsTS.Events {

	public class EventBus : MonoBehaviour {

		private static EventBus instance;

		private Dictionary<Type, UnityEventBase> registeredListeners;

		private void Awake () {
			instance = this;
			registeredListeners = new Dictionary<Type, UnityEventBase>();
		}

		public static T Post<T> (T postedEvent) {
			if (instance.registeredListeners.TryGetValue(typeof(T), out UnityEventBase value)) {
				UnityEvent<T> superType = (UnityEvent<T>)value;
				superType.Invoke(postedEvent);
			}

			return postedEvent;
		}

		public static void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			UnityEvent<T> _event = (instance.registeredListeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;
			if (!instance.registeredListeners.ContainsKey(typeof(T))) instance.registeredListeners.Add(typeof(T), _event);
			_event.AddListener(func);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}