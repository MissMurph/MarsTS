using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Networking;
using MarsTS.Teams;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

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
			if (!CanFactionAfford(Player.Commander)) return;
			
			GhostTransform = Instantiate(building.SelectionGhost).transform;
			SelectionGhostComp = GhostTransform.GetComponent<BuildingSelectionGhost>();
			SelectionGhostComp.InitializeGhost(building);
			
			Player.Input.Hook("Select", OnSelect);
			Player.Input.Hook("Order", OnOrder);
		}

		private void OnPlayerInit(PlayerInitEvent @event) {
			if (@event.Phase != Phase.Post) return;
			
			orderPrefab = CommandRegistry.Get<CommandFactory<IAttackable>>("repair").Prefab;
		}

		protected virtual void Update () {
			if (GhostTransform == null) return;
			
			Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				GhostTransform.position = hit.point;
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (!context.canceled) return;
			
			if (!CanFactionAfford(Player.Commander) || !SelectionGhostComp.Legal) 
				return;
			
			Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				PlaceBuildingServerRpc(
					hit.point,
					Quaternion.Euler(Vector3.zero),
					Player.Commander.Id,
					Player.ListSelected.ToNativeArray32(),
					Player.Include
				);

				Destroy(GhostTransform.gameObject);

				Player.Input.Release("Select");
				Player.Input.Release("Order");
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
		) => PlaceBuildingServer(position, rotation, factionId, selection.ToList(), inclusive);

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

			buildingNetworking.Spawn();
			ghost.SetOwner(faction);
			ghost.InitializeGhost(building.RegistryKey, constructionWorkRequired, Cost);

			WithdrawResourcesFromFaction(faction);

			buildingEvents.AddListener<EntityInitEvent>(
				@event => {
					if (@event.Phase == Phase.Pre) 
						return;
					
					CommandRegistry.Get<Repair>("repair").Construct(ghost as IAttackable, factionId, selection, inclusive);
				}
			);
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

				Player.Input.Release("Select");
				Player.Input.Release("Order");
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