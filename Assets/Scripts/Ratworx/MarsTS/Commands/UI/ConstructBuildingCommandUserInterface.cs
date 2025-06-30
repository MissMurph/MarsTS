using Ratworx.MarsTS.Buildings.Ghosts;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.UI
{
    // TODO: Reimplement properly for ConstructionOptions
    public class ConstructBuildingCommandUserInterface : BaseCommandUserInterface,
                                                         ICommandUserInterfaceArgumentAccepter<ProductionOption>
    {
        protected Transform GhostTransform;
        protected BuildingSelectionGhost SelectionGhostComp;
        
        public override void StartSelection() {
            throw new System.NotImplementedException();
        }

        public override void CancelSelection() {
            if (GhostTransform != null) {
                Destroy(GhostTransform.gameObject);

                Player.Player.Input.Release("Select");
                Player.Player.Input.Release("Order");
            }
        }

        public string GetArgDescription(ProductionOption arg) => throw new System.NotImplementedException();

        public Sprite GetArgIcon(ProductionOption arg) => throw new System.NotImplementedException();

        public void StartArgSelection(ProductionOption arg) {
            /*if (!CanFactionAfford(Player.Player.Commander)) return;
			
            GhostTransform = Instantiate(_buildingGhosts.SelectionGhost).transform;
            SelectionGhostComp = GhostTransform.GetComponent<BuildingSelectionGhost>();
            SelectionGhostComp.InitializeGhost(_buildingGhosts);
			
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);*/
        }
        
        protected virtual void OnSelect (InputAction.CallbackContext context) {
            /*if (!context.canceled) return;
			
            if (!CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
                return;
			
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                PlaceBuildingServerRpc(
                    hit.point,
                    Quaternion.Euler(Vector3.zero),
                    Player.Player.Commander.Id,
                    Player.Player.ListSelected.ToArray(),
                    Player.Player.Include
                );

                Destroy(GhostTransform.gameObject);

                Player.Player.Input.Release("Select");
                Player.Player.Input.Release("Order");
            }*/
        }

        protected virtual void OnOrder (InputAction.CallbackContext context) {
            if (context.canceled) {
                CancelSelection();
            }
        }
        
        protected virtual void Update () {
            if (GhostTransform == null) return;
			
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                GhostTransform.position = hit.point;
            }
        }
        
        [Rpc(SendTo.Server)]
        protected void PlaceBuildingServerRpc(
            Vector3 position,
            Quaternion rotation,
            int factionId,
            int[] selection,
            bool inclusive
        ) => PlaceBuildingServer(position, rotation, factionId, selection, inclusive);

        private void PlaceBuildingServer(
            Vector3 position,
            Quaternion rotation,
            int factionId,
            int[] selection,
            bool inclusive
        ) {
            /*Faction faction = TeamCache.Faction(factionId);
			
            if (!CanFactionAfford(faction)) 
                return;
			
            GameObject constructionGhost = Instantiate(_buildingGhosts.ConstructionGhost, position, rotation);
			
            //Building newBuilding = Instantiate(building, position, rotation);

            BuildingConstructionGhost ghost = constructionGhost.GetComponent<BuildingConstructionGhost>();
            NetworkObject buildingNetworking = constructionGhost.GetComponent<NetworkObject>();
            EventAgent buildingEvents = constructionGhost.GetComponent<EventAgent>();
            var ghostOwnership = constructionGhost.GetComponent<UnitOwnership>();
            var ghostHealth = constructionGhost.GetComponent<HealthAttribute>();

            buildingEvents.AddListener<UnitInitEvent>(
                _ => {
                    //if (@event.Phase == Phase.Pre) 
                    //return;
					
                    CommandPrimer.GetFactory<Repair>("repair").Construct(ghostHealth, factionId, selection, inclusive);
                }
            );
			
            buildingNetworking.Spawn();
            ghostOwnership.SetOwner(faction);
            // ghost.InitializeGhost(building.RegistryKey, constructionWorkRequired, Cost);
            
            // CommandPrimer.GetFactory<CommandFactory<IAttackable>>("repair")

            WithdrawResourcesFromFaction(faction);*/
        }
        
        protected void WithdrawResourcesFromFaction(Faction faction) {
            /*foreach (ResourceCost entry in Cost) 
                faction.GetResource(entry.key).Withdraw(entry.amount);*/
        }
    }
}