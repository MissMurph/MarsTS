using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityInitEvent : AbstractEvent {

		public Entity ParentEntity {
			get {
				return parentEntity;
			}
		}

		private Entity parentEntity;

		public EntityInitEvent (Entity _parentEntity, EventAgent _source) : base("entityInit", _source) {
			parentEntity = _parentEntity;
		}
	}
}