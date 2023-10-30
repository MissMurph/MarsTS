using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Projectile : MonoBehaviour {

        [SerializeField]
        private float speed;

		private bool initialized = false;

		private Unit parent;

		public void Init (Unit _parent) {
			parent = _parent;
			initialized = true;
		}

		private void Update () {
			if (initialized) {
				transform.position += transform.forward * speed * Time.deltaTime;
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (initialized && EntityCache.TryGet(other.transform.root.name, out Unit unit) && unit.GetRelationship(parent.Owner) != Teams.Relationship.Owned) {
				Destroy(gameObject);
			}
		}
	}
}