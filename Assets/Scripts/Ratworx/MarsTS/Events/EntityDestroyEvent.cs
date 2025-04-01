using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityDestroyEvent : AbstractEvent {

		public Entity Entity { get; private set; }

		public EntityDestroyEvent (EventAgent _source, Entity _entity) : base("entityDestroyed", _source) {
			Entity = _entity;
		}
	}
}