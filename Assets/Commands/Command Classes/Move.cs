using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Move : CommandFactory<Vector3> {
		public override string Name { get { return "move"; } }
		public Vector3 Target { get; private set; }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		private void Start () {
			EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
		}

		private void OnPlayerInit (PlayerInitEvent _event) {
			
		}

		public override void Construct (Vector3 _target) {
			throw new NotImplementedException();
		}

		[ServerRpc]
		private void ConstructCommandletServerRpc (Vector3 _target, int _factionId) {
			ConstructCommandletServer(_target, _factionId);
		}

		private void ConstructCommandletServer (Vector3 _target, int _factionId) {
			Commandlet<Vector3> order = Instantiate(orderPrefab);

			order.Init(Name, _target, TeamCache.Faction(_factionId));
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[0];
		}

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
					//Player.Main.DeliverCommand(Construct(hit.point), Player.Include);
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