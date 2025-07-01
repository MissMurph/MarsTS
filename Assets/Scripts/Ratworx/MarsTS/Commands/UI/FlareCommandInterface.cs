using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.UI
{
    public class FlareCommandInterface : BaseCommandInterface
    {
        [SerializeField] private GameObject _markerPrefab;
        private Transform _markerTransform;

        public override void StartSelection() {
            ResourceCost[] cost = (Player.Player.Selection.PrimarySelection.GetCommandables()[0]
                .Commands()[CommandKey] as ICostingCommand)?.GetCost();

            if (cost is not null && !cost.CanFactionAfford(Player.Player.Commander)) return;

            _markerTransform = Instantiate(_markerPrefab).transform;
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);
        }

        private void OnSelect(InputAction.CallbackContext context) {
            if (context.canceled) {
                Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                    int selection = 0;

                    foreach (Roster roster in Player.Player.Selected.Values) {
                        if (!roster.GetCommandKeys().Contains(CommandKey)) continue;

                        // TODO: Replace this with a check for which instance is closest
                        selection = roster.GetCommandables()[0].Entity.Id;
                        break;
                    }
                    
                    ResourceCost[] cost = (Player.Player.Selection.PrimarySelection.GetCommandables()[0]
                        .Commands()[CommandKey] as ICostingCommand)?.GetCost();

                    CommandPrimer.GetFactory<CommandFactory<Vector3>>()
                        .ConstructCommand(
                            CommandKey,
                            hit.point,
                            Player.Player.Commander,
                            new []{selection},
                            Player.Player.Include
                        );
                    
                    WithdrawResourcesFromFaction(cost, Player.Player.Commander);

                    Destroy(_markerTransform.gameObject);

                    Player.Player.Input.Release("Select");
                    Player.Player.Input.Release("Order");
                }
            }
        }

        private void OnOrder(InputAction.CallbackContext context) {
            if (context.canceled) CancelSelection();
        }

        public override void CancelSelection() {
            if (_markerTransform != null) {
                Destroy(_markerTransform.gameObject);

                Player.Player.Input.Release("Select");
                Player.Player.Input.Release("Order");
            }
        }

        // TODO: network up below properly...
        private void WithdrawResourcesFromFaction(ResourceCost[] cost, Faction faction) {
            foreach (ResourceCost entry in cost) {
                if (entry.key == "time") continue;
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }
        }
    }
}