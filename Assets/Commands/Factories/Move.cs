using MarsTS.Players;
using MarsTS.World;
using System;
using System.Collections.Generic;
using MarsTS.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Move : CommandFactory<Vector3> {
		public override string Name => "move";

		public override string Description => description;

		[SerializeField]
		private string description;

		public void Construct (Vector3 target) {
			ConstructCommandletServerRpc(target, Player.Commander.Id, Player.ListSelected.ToNativeArray32(), Player.Include);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc (
			Vector3 target, 
			int factionId, 
			NativeArray<FixedString32Bytes> selection, 
			bool inclusive
		) {
			ConstructCommandletServer(target, factionId, selection.ToStringList(), inclusive);
		}

		public override CostEntry[] GetCost () => Array.Empty<CostEntry>();

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
					Construct(hit.point);
				}

				CancelSelection();
			}
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				CancelSelection();
			}
		}

		public override void CancelSelection () {
			Player.Input.Release("Select");
			Player.Input.Release("Order");
			Player.UI.ResetCursor();
		}
	}
}