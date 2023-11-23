using MarsTS.Players;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Move : Command<Vector3> {
		public override string Name { get { return "move"; } }
		public Vector3 Target { get; private set; }

		public override Sprite Icon { get { return icon; } }

		[SerializeField]
		private Sprite icon;

		public override void StartSelection () {
			Player.Input.Hook("Select", OnClick);
			Player.Input.Hook("Order", OnOrder);
			Player.UI.SetCursor(Pointer);
		}

		private void OnClick (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				Vector2 cursorPos = Player.MousePos;
				Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					Player.Main.DeliverCommand(Construct(hit.point), Player.Include);
				}

				Player.Input.Release("Select");
				Player.UI.ResetCursor();
			}
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				Player.Input.Release("Select");
				Player.UI.ResetCursor();
			}
		}
	}
}