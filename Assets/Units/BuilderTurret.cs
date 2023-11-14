using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units {

    public class BuilderTurret : Turret {

		[SerializeField]
		private int repairAmount;

        protected override void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (target == null) {
				foreach (ISelectable unit in inRangeUnits.Values) {
					if (unit.GetRelationship(parent.Owner) == Relationship.Owned || unit.GetRelationship(parent.Owner) == Relationship.Friendly) {
						target = unit;
						break;
					}
				}
			}
			if (target != null && inRangeUnits.ContainsKey(target.ID) && currentCooldown <= 0) {
				Fire();
			}
		}

		protected override void Fire () {
			//When health is a thing this will have the repairing logic
			target.Attack(-repairAmount);
			currentCooldown = cooldown;
		}
    }
}