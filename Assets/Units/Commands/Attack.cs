using MarsTS.Players;
using MarsTS.Units.Cache;
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

		public ISelectable Target { get; private set; }

		public override void StartSelection () {
			Player.Input.Hook("Select", OnClick);
		}

		private void OnClick (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				Vector2 cursorPos = Player.MousePos;
				Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask) && UnitCache.TryGet(hit.collider.transform.parent.name, out Unit target)) {
					Player.Main.DeliverCommand(Construct(target), Player.Include);
				}

				Player.Input.Release("Select");
			}
		}
	}
}