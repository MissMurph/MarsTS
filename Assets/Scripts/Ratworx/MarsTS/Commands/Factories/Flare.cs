using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands.Factories {

    public class Flare : CommandFactory<Vector3> {

		public override string Name => "flare";

		public override Sprite Icon => icon;

		public override string Description => _description;

		[FormerlySerializedAs("description")]
		[SerializeField]
		private string _description;

		[FormerlySerializedAs("markerPrefab")]
		[SerializeField]
		private GameObject _markerPrefab;
		private Transform _markerTransform;

		[FormerlySerializedAs("cooldown")]
		[SerializeField]
		private float _cooldown;

		[FormerlySerializedAs("cost")]
		[SerializeField]
		private ResourceCost[] _cost;

		public override void StartSelection() {
			if (!CanFactionAfford(Player.Player.Commander)) return;

			_markerTransform = Instantiate(_markerPrefab).transform;
			Player.Player.Input.Hook("Select", OnSelect);
			Player.Player.Input.Hook("Order", OnOrder);
		}

		protected virtual void Update () {
			if (_markerTransform != null) {
				Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					_markerTransform.position = hit.point;
				}
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					if (!CanFactionAfford(Player.Player.Commander)) return;

					int selection = 0;
					
					foreach (Roster roster in Player.Player.Selected.Values) {
						if (!roster.GetCommands().Contains(Name)) continue;

						// TODO: Replace this with a check for which instance is closest
						selection = roster.GetCommandables()[0].Entity.Id;
						break;
					}
					
					Construct(hit.point, selection);

					Destroy(_markerTransform.gameObject);

					Player.Player.Input.Release("Select");
					Player.Player.Input.Release("Order");
				}
			}
		}

		protected virtual void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		public void Construct(Vector3 hitPoint, int selection) {
			ConstructCommandletServerRpc(
				hitPoint, 
				Player.Player.Commander.Id, 
				selection, 
				Player.Player.Include
			);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(
			Vector3 target,
			int factionId,
			int selection,
			bool inclusive
		) {
			ConstructCommandletServer(target, factionId, new[] { selection }, inclusive);
		}

		public override void CancelSelection () {
			if (_markerTransform != null) {
				Destroy(_markerTransform.gameObject);

				Player.Player.Input.Release("Select");
				Player.Player.Input.Release("Order");
			}
		}

		public override ResourceCost[] GetCost () {
			List<ResourceCost> spool = _cost.ToList();

			ResourceCost time = new ResourceCost
			{
				key = "time",
				amount = (int)_cooldown,
			};

			spool.Add(time);

			return spool.ToArray();
		}
		
		private bool CanFactionAfford(Faction faction)
			=> !_cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);
		
		private void WithdrawResourcesFromFaction(Faction faction)
		{
			foreach (ResourceCost entry in _cost)
			{
				faction.GetResource(entry.key).Withdraw(entry.amount);
			}
		}
	}
}