using MarsTS.Entities;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MarsTS.Units {

    public class Projectile : MonoBehaviour {

        [SerializeField]
        private float speed;

		[SerializeField]
		private float lifeTime;

		private bool initialized = false;

		private ISelectable parent;

		private Action<bool, IAttackable> hitCallback;

		public void Init (ISelectable _parent, Action<bool, IAttackable> callback) {
			parent = _parent;
			initialized = true;
			hitCallback = callback;
		}

		private void Update () {
			if (initialized) {
				Vector3 oldPos = transform.position;

				transform.position += transform.forward * speed * Time.deltaTime;

				if (Physics.Raycast(oldPos, transform.position - oldPos, out RaycastHit hit, (speed * Time.deltaTime), GameWorld.EntityMask)) {
					OnTriggerEnter(hit.collider);
				}

				lifeTime -= Time.deltaTime;

				if (lifeTime <= 0f) Destroy(gameObject);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (initialized) {
				if (EntityCache.TryGet(other.transform.root.name, out IAttackable unit)) {
					if (unit.GetRelationship(parent.Owner) != Teams.Relationship.Owned && unit.GetRelationship(parent.Owner) != Teams.Relationship.Friendly) {
						hitCallback(true, unit);
						Destroy(gameObject);
					}
				}
				else {
					Destroy(gameObject);
				}
			}
		}
	}
}