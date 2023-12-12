using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class HarvesterTurret : AbstractTurret {

        //This is how many units per second
        [SerializeField]
        private int harvestRate;

		private int harvestAmount;

		private float cooldown;
		private float currentCooldown;

		private ResourceStorage localStorage;

		protected override void Awake () {
			base.Awake();

			localStorage = GetComponentInParent<ResourceStorage>();

			cooldown = 1f / harvestRate;
			harvestAmount = (int)(harvestRate * cooldown);
		}

		private void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "harvest") {
				Commandlet<IHarvestable> attackCommand = commandableUnit.CurrentCommand as Commandlet<IHarvestable>;

				if (inRangeUnits.ContainsKey(attackCommand.Target.GameObject.name)) {
					target = attackCommand.Target as ISelectable;
				}
			}

			if (target == null) {
				foreach (ISelectable unit in inRangeUnits.Values) {
					if (unit is IHarvestable) {
						target = unit;
						break;
					}
				}
			}

			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name) && currentCooldown <= 0) {
				Harvest();
			}
		}

		private void Harvest () {
			IHarvestable harvestable = target as IHarvestable;

			int harvested = harvestable.Harvest("resource_unit", parent, harvestAmount, localStorage.Submit);
			bus.Global(new ResourceHarvestedEvent(bus, harvestable, parent, ResourceHarvestedEvent.Side.Harvester, harvested, "resource_unit", localStorage.Amount, localStorage.Capacity));

			currentCooldown += cooldown;
		}
	}
}