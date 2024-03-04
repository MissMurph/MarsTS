using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.Vision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Landmine : Building {

		private Explosion explosion;

		[SerializeField]
		private int damage;

		protected override void Awake () {
			base.Awake();

			explosion = transform.Find("Explosion").GetComponent<Explosion>();
			explosion.gameObject.SetActive(false);
		}

		protected void OnTriggerEnter (Collider other) {
			if (!Constructed) return;

			if (EntityCache.TryGet(other.transform.root.name, out IAttackable unit) && other.transform.root.tag == "Vehicle") {
				if (unit.GetRelationship(Owner) != Teams.Relationship.Owned && unit.GetRelationship(Owner) != Teams.Relationship.Friendly) {
					explosion.Init(damage, Owner);
					explosion.gameObject.SetActive(true);
					explosion.transform.SetParent(null, true);

					Destroy(gameObject);
				}
			}
		}

		public void SetConstructionProgress (int progress) {
			currentWork = progress;
			Health = maxHealth * (int)(currentWork / constructionWork);
		}

		protected override void OnVisionUpdate (EntityVisibleEvent _event) {
			bool visible = _event.Visible;

			foreach (GameObject hideable in visionObjects) {
				hideable.SetActive(visible);
			}
		}
	}
}