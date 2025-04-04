using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Units;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Factories {
	public class Attack : CommandFactory<IAttackable> {
		public override string Name => "attack";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Player.Player.Input.Hook("Select", OnSelect);
			Player.Player.Input.Hook("Order", OnOrder);
			Player.Player.UI.SetCursor(Pointer);
		}

		private void OnSelect (InputAction.CallbackContext context) {
			//On Mouse Up
			if (!context.canceled) return;
			
			Vector2 cursorPos = Player.Player.MousePos;
			Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.EntityMask) 
			    && EntityCache.TryGetEntityComponent(hit.collider.transform.parent.name, out IAttackable unit)) {
				Construct(unit);
			}

			CancelSelection();
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				CancelSelection();
			}
		}

		public void Construct(IAttackable target) {
			ConstructCommandletServerRpc(
				target.GameObject.name, 
				Player.Player.Commander.Id, 
				Player.Player.ListSelected.ToNativeArray32(), 
				Player.Player.Include
			);
		}
		
		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc (
			string target, 
			int factionId, 
			NativeArray<FixedString32Bytes> selection, 
			bool inclusive
		) {
			if (!EntityCache.TryGetEntityComponent(target, out IAttackable unit))
			{
				Debug.LogError($"Invalid target entity {target} for {Name} Command! Command being ignored!");
				return;
			}
			
			ConstructCommandletServer(unit, factionId, selection.ToStringList(), inclusive);
		}

		public override CostEntry[] GetCost () => Array.Empty<CostEntry>();

		public override void CancelSelection () {
			Player.Player.Input.Release("Select");
			Player.Player.Input.Release("Order");
			Player.Player.UI.ResetCursor();
		}
	}
}