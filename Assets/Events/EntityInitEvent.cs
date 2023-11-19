using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityInitEvent : AbstractEvent {

		public Entity ParentEntity { get; private set; }

		public EntityInitEvent (Entity _parentEntity, EventAgent _source) : base("entityInit", _source) {
			ParentEntity = _parentEntity;
		}
	}
}