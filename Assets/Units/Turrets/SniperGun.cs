using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MarsTS.Units {

    public class SniperGun : ProjectileTurret {

		//This damage is added to the attack whenever infantry is hit
		[SerializeField]
		protected int bonusDamage;

		protected override void Fire (Vector3 position) {
			Vector3 direction = (position - transform.position).normalized;

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(position);

			bullet.Init(parent, OnHit);

			currentCooldown += cooldown;
		}

		protected virtual void OnHit (bool result, IAttackable hit) {
			if (hit.GameObject.tag == "Infantry") {
				hit.Attack(damage + bonusDamage);
			}
			else hit.Attack(damage);
		}
	}
}