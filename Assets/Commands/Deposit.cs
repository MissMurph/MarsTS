using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Deposit : Command<IDepositable> {

		public override string Name { get { return "deposit"; } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Player.Input.Hook("Select", OnSelect);
			Player.Input.Hook("Order", OnOrder);
			Player.UI.SetCursor(Pointer);
		}

		private void OnSelect (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				Vector2 cursorPos = Player.MousePos;
				Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask) && EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable target) && target is IDepositable deserialized) {
					Player.Main.DeliverCommand(Construct(deserialized), Player.Include);
				}

				Player.Input.Release("Select");
				Player.UI.ResetCursor();
			}
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				CancelSelection();
			}
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[0];
		}

		public override void CancelSelection () {
			Player.Input.Release("Select");
			Player.Input.Release("Order");
			Player.UI.ResetCursor();
		}
	}
}