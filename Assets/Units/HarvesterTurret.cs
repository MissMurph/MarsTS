using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class HarvesterTurret : Turret {

        //This is how many units per second
        [SerializeField]
        private int harvestRate;

        private float stepTime;
		private float timeToStep;
		private int harvestAmount;

		private IHarvestable harvestTarget;

		protected override void Awake () {
			base.Awake();

			stepTime = 1f / harvestRate;
			timeToStep = stepTime;
			harvestAmount = (int)(harvestRate / stepTime);
		}

		protected override void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (target == null) {
				foreach (IAttackable unit in inRangeUnits.Values) {
					if (unit is IHarvestable harvestNode) {
						harvestTarget = harvestNode;
						break;
					}
				}
			}
			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name) && currentCooldown <= 0) {
				Fire();
			}
		}

		protected override void Fire () {
			harvestTarget.Harvest("resource_unit", harvestAmount);
		}
	}
}