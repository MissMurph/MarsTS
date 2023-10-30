using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MarsTS.Units {

    public class ProjectileTurret : Turret {

		[SerializeField]
		private GameObject projectile;

		protected override void Fire () {
			Vector3 direction = (target.GameObject.transform.position - transform.position).normalized;

			//Physics.Raycast(barrel.transform.position, direction, range.radius);
			//Debug.DrawLine(barrel.transform.position, barrel.transform.position + (direction * range.radius), Color.cyan, 0.1f);

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(target.GameObject.transform.position);

			bullet.Init(parent);

			currentCooldown = cooldown;
		}
	}
}