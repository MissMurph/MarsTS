using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Units.Turrets;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry {

    public class Marine : InfantryMembership {

		private ProjectileTurret equippedWeapon;

		/*	Marine Fields	*/

		[SerializeField]
		protected float adrenoSpeed;

		/*	Adrenaline	*/

		private void Adrenaline (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			if (deserialized.Target) {
				// _currentSpeed = adrenoSpeed;

				// _bus.AddListener<CooldownEvent>(AdrenalineCooldown);
			}
		}

		private void AdrenalineComplete (CommandActiveEvent _event) {
			// _bus.RemoveListener<CommandActiveEvent>(AdrenalineComplete);

			if (!_event.Activity) {
				// _currentSpeed = _moveSpeed;
			}
		}

		private void AdrenalineCooldown (CooldownEvent _event) {
			// _bus.RemoveListener<CooldownEvent>(AdrenalineCooldown);

			// _currentSpeed = _moveSpeed;
		}
	}
}