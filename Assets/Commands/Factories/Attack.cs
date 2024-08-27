using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using System;
using System.Collections.Generic;
using MarsTS.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {
	public class Attack : CommandFactory<IAttackable> {
		public override string Name => "attack";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Player.Input.Hook("Select", OnSelect);
			Player.Input.Hook("Order", OnOrder);
			Player.UI.SetCursor(Pointer);
		}

		private void OnSelect (InputAction.CallbackContext context) {
			//On Mouse Up
			if (!context.canceled) return;
			
			Vector2 cursorPos = Player.MousePos;
			Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.EntityMask) 
			    && EntityCache.TryGet(hit.collider.transform.parent.name, out IAttackable unit)) {
				Construct(unit, Player.ListSelected);
			}

			CancelSelection();
		}

		private void OnOrder (InputAction.CallbackContext context) {
			//On Mouse Up
			if (context.canceled) {
				CancelSelection();
			}
		}

		public void Construct(IAttackable target, List<string> selection) {
			ConstructCommandletServerRpc(target.GameObject.name, Player.Commander.Id, selection.ToNativeArray32(), Player.Include);
		}
		
		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc (string _target, int _factionId, NativeArray<FixedString32Bytes> _selection, bool _inclusive)
		{
			if (!EntityCache.TryGet(_target, out IAttackable unit))
			{
				Debug.LogError($"Invalid target entity {_target} for {Name} Command! Command being ignored!");
				return;
			}
			
			ConstructCommandletServer(unit, _factionId, _selection.ToList(), _inclusive);
		}

		public override CostEntry[] GetCost () {
			return Array.Empty<CostEntry>();
		}

		public override void CancelSelection () {
			Player.Input.Release("Select");
			Player.Input.Release("Order");
			Player.UI.ResetCursor();
		}
	}
}