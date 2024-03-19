using MarsTS.Entities;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Units {

    public class Explosion : MonoBehaviour {

        [SerializeField]
        private int damage;

        [SerializeField]
        private float lifeTime = 0.125f;

		private Faction owner;

		private bool initialized = false;

		private List<Collider> hit = new List<Collider>();

		public void Init (int _damage, Faction _owner) {
			damage = _damage;
			owner = _owner;
			initialized = true;
		}

		private void Update () {
			if (initialized) {
				lifeTime -= Time.deltaTime;

				if (lifeTime <= 0) Destroy(gameObject);
			}
		}

		private void OnTriggerEnter (Collider other) {
			hit.Add(other);
		}

		private void OnDestroy () {
			foreach (Collider other in hit) {
				if (other == null) continue;
				if (EntityCache.TryGet(other.transform.root.name, out IAttackable unit)) {
					if (unit.GetRelationship(owner) != Teams.Relationship.Owned && unit.GetRelationship(owner) != Teams.Relationship.Friendly) {
						unit.Attack(damage);
					}
				}
			}
		}
	}
}