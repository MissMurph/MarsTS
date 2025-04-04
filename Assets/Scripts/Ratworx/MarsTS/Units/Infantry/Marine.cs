using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units.SafeReference;
using Ratworx.MarsTS.Units.Turrets;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry {

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
					_currentPath = Path.Empty;
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

				EntityCache.TryGetEntityComponent(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized) {
				EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			if (CurrentCommand == null) return;

			EntityCache.TryGetEntityComponent(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			//CurrentCommand.OnComplete(commands, newEvent);

			Stop();
		}

		/*	Adrenaline	*/

		private void Adrenaline (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			if (deserialized.Target) {
				_currentSpeed = adrenoSpeed;

				_bus.AddListener<CooldownEvent>(AdrenalineCooldown);
			}
		}

		private void AdrenalineComplete (CommandActiveEvent _event) {
			_bus.RemoveListener<CommandActiveEvent>(AdrenalineComplete);

			if (!_event.Activity) {
				_currentSpeed = _moveSpeed;
			}
		}

		private void AdrenalineCooldown (CooldownEvent _event) {
			_bus.RemoveListener<CooldownEvent>(AdrenalineCooldown);

			_currentSpeed = _moveSpeed;
		}
	}
}