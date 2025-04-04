using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Entities;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using MarsTS.Networking;
using MarsTS.Units;
using UnityEngine.Serialization;

namespace MarsTS.Commands {

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
		private CostEntry[] _cost;

		public override void StartSelection() {
			if (!CanFactionAfford(Player.Commander)) return;

			_markerTransform = Instantiate(_markerPrefab).transform;
			Player.Input.Hook("Select", OnSelect);
			Player.Input.Hook("Order", OnOrder);
		}

		protected virtual void Update () {
			if (_markerTransform != null) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					_markerTransform.position = hit.point;
				}
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					if (!CanFactionAfford(Player.Commander)) return;

					string selection = string.Empty;
					
					foreach (Roster roster in Player.Selected.Values) {
						if (!roster.Commands.Contains(Name)) continue;

						// TODO: Replace this with a check for which instance is closest
						selection = roster.Orderable[0].GameObject.name;
						break;
					}
					
					Construct(hit.point, selection);

					Destroy(_markerTransform.gameObject);

					Player.Input.Release("Select");
					Player.Input.Release("Order");
				}
			}
		}

		protected virtual void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		public void Construct(Vector3 hitPoint, string selection) {
			ConstructCommandletServerRpc(
				hitPoint, 
				Player.Commander.Id, 
				selection, 
				Player.Include
			);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(
			Vector3 target,
			int factionId,
			string selection,
			bool inclusive
		) {
			ConstructCommandletServer(target, factionId, new List<string>{ selection }, inclusive);
		}

		protected override void ConstructCommandletServer(Vector3 target, int factionId, ICollection<string> selection, bool inclusive) {
			Faction faction = TeamCache.Faction(factionId);
			
			if (!CanFactionAfford(faction)) return;
			
			FlareCommandlet order = (FlareCommandlet)Instantiate(orderPrefab);

			order.InitFlare(Name, target, TeamCache.Faction(factionId), _cooldown, _cost);

			foreach (string entity in selection) {
				if (EntityCache.TryGet(entity, out ICommandable unit))
					unit.Order(order, inclusive);
				else
					Debug.LogWarning($"ICommandable on Unit {entity} not found! Command {Name} being ignored by unit!");
			}
			
			WithdrawResourcesFromFaction(faction);
		}

		public override void CancelSelection () {
			if (_markerTransform != null) {
				Destroy(_markerTransform.gameObject);

				Player.Input.Release("Select");
				Player.Input.Release("Order");
			}
		}

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = _cost.ToList();

			CostEntry time = new CostEntry
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
			foreach (CostEntry entry in _cost)
			{
				faction.GetResource(entry.key).Withdraw(entry.amount);
			}
		}
	}
}