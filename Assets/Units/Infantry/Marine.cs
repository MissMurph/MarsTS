using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Marine : InfantryMember {

		/*	Attacking	*/

		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

		private ProjectileTurret equippedWeapon;

		/*	Marine Fields	*/

		[SerializeField]
		protected float adrenoSpeed;

		protected override void Awake () {
			base.Awake();

			equippedWeapon = GetComponentInChildren<ProjectileTurret>();
		}

		protected override void Update () {
			base.Update();

			//I'd like to move these all to commands, for now they'll remain here
			//Will start devising a method to do so
			if (AttackTarget.Get != null) {
				if (equippedWeapon.IsInRange(AttackTarget.Get)) {
					TrackedTarget = null;
					currentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
					SetTarget(AttackTarget.GameObject.transform);
				}
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			switch (order.Name) {
				case "attack":
					Attack(order);
					break;
				case "adrenaline":
					Adrenaline(order);
					break;
				default:
					base.Order(order, inclusive);
					break;
			}
		}

		/*	Commands	*/

		/*	Attack	*/
		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnTargetDeath (EntityDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			Stop();
		}

		/*	Adrenaline	*/

		private void Adrenaline (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			if (deserialized.Target) {
				currentSpeed = adrenoSpeed;

				bus.AddListener<CommandActiveEvent>(AdrenalineComplete);
			}
		}

		private void AdrenalineComplete (CommandActiveEvent _event) {
			bus.RemoveListener<CommandActiveEvent>(AdrenalineComplete);

			if (!_event.Activity) {
				currentSpeed = moveSpeed;
			}
		}
	}
}