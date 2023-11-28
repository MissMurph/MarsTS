using MarsTS.Commands;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units {

    public class ProjectileTurret : AbstractTurret {

		[SerializeField]
		private GameObject projectile;

		[SerializeField]
		private int damage;

		[SerializeField]
		private float cooldown;
		private float currentCooldown;

		private void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "attack") {
				Commandlet<IAttackable> attackCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

				if (inRangeUnits.ContainsKey(attackCommand.Target.GameObject.name)) {
					target = attackCommand.Target as ISelectable;
				}
			}

			if (target == null) {
				float distance = Range;
				IAttackable currentClosest = null;

				foreach (ISelectable unit in inRangeUnits.Values) {
					if (unit is IAttackable targetable && unit.GetRelationship(parent.Owner) == Relationship.Hostile) {
						float newDistance = Vector3.Distance(unit.GameObject.transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = targetable;
						}
					}
				}

				if (currentClosest != null) target = currentClosest as ISelectable;
			}

			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name) && currentCooldown <= 0) {
				Fire();
			}
		}

		protected void Fire () {
			Vector3 direction = (target.GameObject.transform.position - transform.position).normalized;

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(target.GameObject.transform.position);

			bullet.Init(parent, (success, unit) => { if (success) unit.Attack(damage); });

			currentCooldown += cooldown;
		}
	}
}