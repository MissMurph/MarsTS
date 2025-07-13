using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Ratworx.MarsTS.Events
{
    // [RequireComponent(typeof(Entity))]
    public class EventAgent : MonoBehaviour, IEntityComponent<EventAgent>
    {
        private readonly Dictionary<Type, UnityEventBase> _listeners = new Dictionary<Type, UnityEventBase>();

        public string Key => "eventAgent";

        public void AddListener<T>(UnityAction<T> func) where T : AbstractEvent {
            var _event = (_listeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;

            if (!_listeners.ContainsKey(typeof(T)))
                _listeners.Add(typeof(T), _event);

            _event.AddListener(func);
        }

        public void RemoveListener<T>(UnityAction<T> func) where T : AbstractEvent {
            if (!_listeners.ContainsKey(typeof(T))) return;

            var _event = (_listeners.GetValueOrDefault(typeof(T), new UnityEvent<T>())) as UnityEvent<T>;

            _event.RemoveListener(func);
        }

        public T PostLocal<T>(T postedEvent) where T : AbstractEvent {
            if (_listeners.TryGetValue(typeof(T), out UnityEventBase value) && value is UnityEvent<T> superTypeEvent) {
                superTypeEvent.Invoke(postedEvent);
            }

            return postedEvent;
        }

        public T PostGlobal<T>(T postedEvent) where T : AbstractEvent {
            if (_listeners.TryGetValue(typeof(T), out UnityEventBase value) && value is UnityEvent<T> superTypeEvent) {
                superTypeEvent.Invoke(postedEvent);
            }

            EventBus.Post(postedEvent);

            return postedEvent;
        }

        public EventAgent Get() => this;
    }
}