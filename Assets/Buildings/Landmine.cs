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

		public void Detonate () {
			explosion.Init(damage, Owner);
			explosion.gameObject.SetActive(true);
			explosion.transform.SetParent(null, true);

			bus.Global(new UnitDeathEvent(bus, this));

			Destroy(gameObject);
		}

		public void SetConstructionProgress (int progress) {
			currentWork = progress;
			Health = MaxHealth * (int)(currentWork / constructionWork);
		}

		protected override void OnVisionUpdate (EntityVisibleEvent _event) {
			bool visible = _event.Visible;

			foreach (GameObject hideable in visionObjects) {
				hideable.SetActive(visible);
			}
		}
	}
}