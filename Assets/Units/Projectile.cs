using MarsTS.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MarsTS.Units {

    public class Projectile : MonoBehaviour {

        [SerializeField]
        private float speed;

		private bool initialized = false;

		private Unit parent;

		private Action<bool, IAttackable> hitCallback;

		public void Init (Unit _parent, Action<bool, IAttackable> callback) {
			parent = _parent;
			initialized = true;
			hitCallback = callback;
		}

		private void Update () {
			if (initialized) {
				transform.position += transform.forward * speed * Time.deltaTime;
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (initialized && EntityCache.TryGet(other.transform.root.name, out IAttackable unit) && unit.GetRelationship(parent.Owner) != Teams.Relationship.Owned) {
				hitCallback(true, unit);
				Destroy(gameObject);
			}
		}
	}
}