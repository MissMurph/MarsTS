using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Units.Commands {

	public class Attack : Command<ISelectable> {

		public override string Name { get { return "attack"; } }

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

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask) && EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable target)) {
					Player.Main.DeliverCommand(Construct(target), Player.Include);
				}

				Player.Input.Release("Select");
				Player.UI.ResetCursor();
			}
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				Player.Input.Release("Select");
				Player.Input.Release("Order");
				Player.UI.ResetCursor();
			}
		}
	}
}