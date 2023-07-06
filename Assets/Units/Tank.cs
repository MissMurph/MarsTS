using MarsTS.Units.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Tank : Unit {

		protected Dictionary<string, Turret> registeredTurrets = new Dictionary<string, Turret>();

		[SerializeField]
		protected Turret[] turretsToRegister;

		protected Unit AttackTarget {
			get {
				return attackTarget;
			}
			set {
				attackTarget = value;
				registeredTurrets["turret_main"].target = attackTarget;

				
			}
		}

		protected Unit attackTarget;

		protected override void Awake () {
			base.Awake();
			foreach (Turret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (attackTarget == null) return;

			if (Vector3.Distance(transform.position, attackTarget.transform.position) >= registeredTurrets["turret_main"].Range
				&& !ReferenceEquals(target, attackTarget.transform)) {
				SetTarget(attackTarget.transform, (result) => { /*	we dont want to do anything here	*/});
			}
			else if (target == attackTarget.transform) {
				Stop();
			}
		}

		protected override void ProcessOrder (Commandlet order) {
			AttackTarget = null;
			switch (order.Name) {
				case "move":
				Move(order);
				break;
				case "stop":
				Stop();
				break;
				case "attack":
				Attack(order);
				break;
			}
		}

		protected void Attack (Commandlet order) {
			if (order.TargetType.Equals(typeof(ISelectable))) {
				Commandlet<ISelectable> deserialized = order as Commandlet<ISelectable>;

				AttackTarget = deserialized.Target.Get();
			}
		}
	}
}