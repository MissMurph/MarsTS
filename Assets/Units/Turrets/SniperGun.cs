using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace MarsTS.Units {

    public class SniperGun : ProjectileTurret {

		//This damage is added to the attack whenever infantry is hit
		[SerializeField]
		protected int bonusDamage;

		protected override void OnHit (bool _success, IAttackable _unit) {
			if (NetworkManager.Singleton.IsServer && _success) {
				if (_unit.GameObject.tag.Equals("Infantry")) {
					_unit.Attack(damage + bonusDamage);
				}
				else _unit.Attack(damage);
			}
		}
	}
}