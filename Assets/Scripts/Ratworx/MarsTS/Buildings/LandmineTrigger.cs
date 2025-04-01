using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class LandmineTrigger : MonoBehaviour {

		private Landmine parent;
		private bool detonated = false;

		private void Awake () {
			parent = GetComponentInParent<Landmine>();
		}

		private void OnTriggerEnter (Collider other) {
			if (detonated) return;

			if (EntityCache.TryGet(other.transform.root.name, out IAttackable unit) && other.transform.root.tag == "Vehicle") {
				if (unit.GetRelationship(parent.Owner) != Teams.Relationship.Owned && unit.GetRelationship(parent.Owner) != Teams.Relationship.Friendly) {
					parent.Detonate();
					detonated = true;
				}
			}
		}
	}
}