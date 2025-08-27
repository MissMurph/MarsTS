using Ratworx.MarsTS.Buildings.Ghosts;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Buildings.BuildingPlacers
{
    // TODO: Expand into interface and hook into registry system for custom content
    public class BaseBuildingPlacer : NetworkBehaviour
    {
        public string[] TargetBuildings => _targetBuildings;
        
        protected Transform GhostTransform;
        protected BuildingSelectionGhost SelectionGhostComp;
        protected ConstructionOption CurrentlyPlacingOption;
        
        [SerializeField] private string[] _targetBuildings;

        public virtual void StartPlacingBuilding(ConstructionOption option) {
            if (!TryGetBuildingGhosts(option, out BuildingGhosts ghosts)) return;

            GhostTransform = Instantiate(ghosts.SelectionGhost).transform;
            SelectionGhostComp = GhostTransform.GetComponent<BuildingSelectionGhost>();
            SelectionGhostComp.InitializeGhost(ghosts);
            CurrentlyPlacingOption = option;
			
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);
        }

        public virtual void CancelPlacingBuilding() {
            if (GhostTransform is null) 
                return;
            
            Destroy(GhostTransform.gameObject);

            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
        }
        
        protected virtual void OnSelect (InputAction.CallbackContext context) {
            if (!context.canceled) return;
			
            if (!CurrentlyPlacingOption.CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
                return;
			
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                PlaceBuildingServerRpc(
                    GhostTransform.position,
                    Quaternion.Euler(Vector3.zero),
                    CurrentlyPlacingOption,
                    Player.Player.Commander.Id,
                    Player.Player.ListSelected.ToArray(),
                    Player.Player.Include
                );
                
                Destroy(GhostTransform.gameObject);

                Player.Player.Input.Release("Select");
                Player.Player.Input.Release("Order");
                
                GhostTransform = null;
                SelectionGhostComp = null;
                CurrentlyPlacingOption = null;
            }
        }

        protected virtual void OnOrder (InputAction.CallbackContext context) {
            if (context.canceled) {
                CancelPlacingBuilding();
            }
        }
        
        protected virtual void Update () {
            if (GhostTransform is null) return;
			
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                GhostTransform.position = hit.point;
            }
        }
        
        private static bool TryGetBuildingGhosts(ConstructionOption arg, out BuildingGhosts ghosts)
        {
            if (!Registry.Registry.TryGetPrefab(arg.BuildingKey, out GameObject prefab)) {
                RatLogger.Error?.Log($"No prefab with key {arg.BuildingKey} found!");
                ghosts = null;
                return false;
            }

            if (!prefab.TryGetComponent(out ghosts)) {
                RatLogger.Error?.Log($"No {nameof(BuildingGhosts)} component on prefab {arg.BuildingKey}!");
                return false;
            }

            return true;
        }
        
        [Rpc(SendTo.Server)]
        private  void PlaceBuildingServerRpc(
            Vector3 position,
            Quaternion rotation,
            ConstructionOption option,
            int factionId,
            int[] selection,
            bool inclusive
        ) => PlaceBuildingServer(position, rotation, option, factionId, selection, inclusive);

        private void PlaceBuildingServer(Vector3 position,
            Quaternion rotation,
            ConstructionOption option,
            int factionId,
            int[] selection,
            bool inclusive
        ) {
            Faction faction = TeamCache.Faction(factionId);
			
            if (!option.CanFactionAfford(faction) || !TryGetBuildingGhosts(option, out BuildingGhosts ghosts)) 
                return;
			
            GameObject constructionGhost = Instantiate(ghosts.ConstructionGhost, position, rotation);
			
            //Building newBuilding = Instantiate(building, position, rotation);

            BuildingConstructionGhost ghost = constructionGhost.GetComponent<BuildingConstructionGhost>();
            NetworkObject buildingNetworking = constructionGhost.GetComponent<NetworkObject>();
            EventAgent buildingEvents = constructionGhost.GetComponent<EventAgent>();
            var ghostOwnership = constructionGhost.GetComponent<UnitOwnership>();
            var ghostHealth = constructionGhost.GetComponent<HealthAttribute>();

            buildingEvents.AddListener<UnitInitEvent>(
                _ => {
                    CommandPrimer.GetFactory<CommandFactory<IAttackable>>().ConstructCommand(
                        "repair",
                        ghostHealth,
                        Player.Player.Commander,
                        Player.Player.ListSelected.ToArray(),
                        Player.Player.Include
                    );
                }
            );
			
            buildingNetworking.Spawn();
            ghostOwnership.SetOwner(faction);
            ghost.InitializeGhost(option.BuildingKey, option.Cost);
            
            WithdrawResourcesFromFaction(option.Cost, faction);
        }
        
        // TODO: Should move this to the player object maybe? Pass in a params ResourceCost[]?
        private void WithdrawResourcesFromFaction(ResourceCost[] cost, Faction faction) {
            foreach (ResourceCost entry in cost) {
                if (entry.key == "time") continue;
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }
        }
    }
}