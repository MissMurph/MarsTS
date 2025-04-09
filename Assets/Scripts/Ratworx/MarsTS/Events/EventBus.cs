using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Ratworx.MarsTS.Events 
{
	public class EventBus 
	{
		private static EventBus _instance;

		private readonly Dictionary<Type, UnityEventBase> _globalListeners;
		
		private static bool IsInitialized => _instance != null;

		private EventBus() {
			_globalListeners = new Dictionary<Type, UnityEventBase>();
		}

		private static void Init () {
			_instance = new EventBus();
		}

		public static T Post<T> (T postedEvent) where T : AbstractEvent {
			if (!IsInitialized) Init();

			if (!_instance._globalListeners.TryGetValue(typeof(T), out UnityEventBase value)) 
				return postedEvent;
			
			var superType = (UnityEvent<T>)value;
			superType.Invoke(postedEvent);

			return postedEvent;
		}

		public static void AddListener<T> (UnityAction<T> func) where T : AbstractEvent {
			if (!IsInitialized) Init();
			
			var _event = _instance._globalListeners.GetValueOrDefault(typeof(T), new UnityEvent<T>()) as UnityEvent<T>;
			
			if (!_instance._globalListeners.ContainsKey(typeof(T))) 
				_instance._globalListeners.Add(typeof(T), _event);
			
			_event.AddListener(func);
		}

		public static void RemoveListener<T> (UnityAction<T> func) where T : AbstractEvent {
			if (!IsInitialized) Init();

			if (!_instance._globalListeners.TryGetValue(typeof(T), out UnityEventBase _event)) 
				return;
			
			UnityEvent<T> deserialized = _event as UnityEvent<T>;
			deserialized.RemoveListener(func);
		}

		private void OnDestroy () {
			_instance = null;
		}
	}
}