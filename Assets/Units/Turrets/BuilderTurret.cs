using MarsTS.Commands;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units {

    public class BuilderTurret : AbstractTurret {

		[SerializeField]
		private int repairRate;

		private int repairAmount;

		private float cooldown;
		private float currentCooldown;

		protected override void Awake () {
			base.Awake();

			cooldown = 1f / repairRate;
			repairAmount = (int)(repairRate * cooldown);
		}

		private void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "repair") {
				Commandlet<IAttackable> attackCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

				if (inRangeUnits.ContainsKey(attackCommand.Target.GameObject.name)) {
					target = attackCommand.Target as ISelectable;
				}
			}

			if (target == null) {
				float distance = Range;
				IAttackable currentClosest = null;

				foreach (ISelectable unit in inRangeUnits.Values) {
					if (unit is IAttackable targetable && (unit.GetRelationship(parent.Owner) == Relationship.Owned || unit.GetRelationship(parent.Owner) == Relationship.Friendly)) {
						if (targetable.Health >= targetable.MaxHealth) break;

						float newDistance = Vector3.Distance(unit.GameObject.transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = targetable;
						}
					}
				}

				if (currentClosest != null) target = currentClosest as ISelectable;
			}

			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name) && currentCooldown <= 0) {
				Repair();
			}
		}

		private void Repair () {
			IAttackable targetable = target as IAttackable;
			targetable.Attack(-repairAmount);
			currentCooldown += cooldown;
		}
    }
}