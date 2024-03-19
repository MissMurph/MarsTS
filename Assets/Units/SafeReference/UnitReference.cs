using MarsTS.Entities;
using MarsTS.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

namespace MarsTS.Units {

	public class UnitReference<T> where T : class {

		private GameObject unitObject;

		public T Get { get { return value; } }

		public GameObject GameObject { get { return unitObject; } }

		private T value;

		public void Set (T newValue, GameObject _unit) {
			if (value != null) {
				EntityCache.TryGet(unitObject.name + ":eventAgent", out EventAgent oldAgent);
				oldAgent.RemoveListener<UnitDeathEvent>(OnEntityDeath);
				oldAgent.RemoveListener<EntityVisibleEvent>(OnEntityVisible);
			}

			value = newValue;
			unitObject = _unit;

			if (value != null) {
				EntityCache.TryGet(_unit.name + ":eventAgent", out EventAgent agent);
				agent.AddListener<UnitDeathEvent>(OnEntityDeath);
				agent.AddListener<EntityVisibleEvent>(OnEntityVisible);
			}
		}

		private void OnEntityDeath (UnitDeathEvent _event) {
			Set(null, null);
		}

		private void OnEntityVisible (EntityVisibleEvent _event) {
			if (!_event.Visible) {
				Set(null, null);
			}
		}
	}
}