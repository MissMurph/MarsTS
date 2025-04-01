using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Buildings.Ghosts;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Factories {

	public class PlaceBuilding : CommandFactory<IAttackable> {

		public override string Name => "construct/" + building.UnitType;

		public override Sprite Icon => building.Icon;

		public override string Description => description;

		[SerializeField]
		private string description;

		[SerializeField]
		protected Building building;

		protected Transform GhostTransform;
		protected BuildingSelectionGhost SelectionGhostComp;

		[SerializeField]
		protected int constructionWorkRequired;
		
		[SerializeField]
		protected CostEntry[] Cost;

		private void Awake () {
			//Cost = building.ConstructionCost;
			EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
		}

		public override void StartSelection () {
			if (!CanFactionAfford(Player.Player.Commander)) return;
			
			GhostTransform = Instantiate(building.SelectionGhost).transform;
			SelectionGhostComp = GhostTransform.GetComponent<BuildingSelectionGhost>();
			SelectionGhostComp.InitializeGhost(building);
			
			Player.Player.Input.Hook("Select", OnSelect);
			Player.Player.Input.Hook("Order", OnOrder);
		}

		private void OnPlayerInit(PlayerInitEvent @event) {
			if (@event.Phase != Phase.Post) return;
			
			orderPrefab = CommandPrimer.Get<CommandFactory<IAttackable>>("repair").Prefab;
		}

		protected virtual void Update () {
			if (GhostTransform == null) return;
			
			Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				GhostTransform.position = hit.point;
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (!context.canceled) return;
			
			if (!CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
				return;
			
			Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				PlaceBuildingServerRpc(
					hit.point,
					Quaternion.Euler(Vector3.zero),
					Player.Player.Commander.Id,
					Player.Player.ListSelected.ToNativeArray32(),
					Player.Player.Include
				);

				Destroy(GhostTransform.gameObject);

				Player.Player.Input.Release("Select");
				Player.Player.Input.Release("Order");
			}
		}

		protected virtual void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		[Rpc(SendTo.Server)]
		protected void PlaceBuildingServerRpc(
			Vector3 position,
			Quaternion rotation,
			int factionId,
			NativeArray<FixedString32Bytes> selection,
			bool inclusive
		) => PlaceBuildingServer(position, rotation, factionId, selection.ToStringList(), inclusive);

		private void PlaceBuildingServer(
			Vector3 position,
			Quaternion rotation,
			int factionId,
			List<string> selection,
			bool inclusive
		) {
			Faction faction = TeamCache.Faction(factionId);
			
			if (!CanFactionAfford(faction)) 
				return;
			
			GameObject constructionGhost = Instantiate(building.ConstructionGhost, position, rotation);
			
			//Building newBuilding = Instantiate(building, position, rotation);

			BuildingConstructionGhost ghost = constructionGhost.GetComponent<BuildingConstructionGhost>();
			NetworkObject buildingNetworking = constructionGhost.GetComponent<NetworkObject>();
			EventAgent buildingEvents = constructionGhost.GetComponent<EventAgent>();

			buildingEvents.AddListener<UnitInitEvent>(
				_ => {
					//if (@event.Phase == Phase.Pre) 
					//return;
					
					CommandPrimer.Get<Repair>("repair").Construct(ghost, factionId, selection, inclusive);
				}
			);
			
			buildingNetworking.Spawn();
			ghost.SetOwner(faction);
			ghost.InitializeGhost(building.RegistryKey, constructionWorkRequired, Cost);

			WithdrawResourcesFromFaction(faction);
		}

		public override CostEntry[] GetCost () {
			 List<CostEntry> spool = Cost.ToList();

			CostEntry time = new CostEntry {
				key = "time",
				amount = constructionWorkRequired
			};

			spool.Add(time);

			return spool.ToArray();
		}

		public override void CancelSelection () {
			if (GhostTransform != null) {
				Destroy(GhostTransform.gameObject);

				Player.Player.Input.Release("Select");
				Player.Player.Input.Release("Order");
			}
		}

		protected bool CanFactionAfford(Faction faction)
			=> !Cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);

		protected void WithdrawResourcesFromFaction(Faction faction) {
			foreach (CostEntry entry in Cost) 
				faction.GetResource(entry.key).Withdraw(entry.amount);
		}
	}
}