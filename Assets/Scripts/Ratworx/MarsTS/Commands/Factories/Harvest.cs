using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.WorldObject;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class Harvest : CommandFactory<IHarvestable>
    {
        public override string Name => "harvest";

        public override string Description => _description;

        [SerializeField] private string _description;

        public override void StartSelection()
        {
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);
            Player.Player.UI.SetCursor(Pointer);
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (context.canceled)
            {
                Vector2 cursorPos = Player.Player.MousePos;
                Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)
                    && EntityCache.TryGetEntityComponent(hit.collider.transform.parent.name + ":selectable", out ISelectable unit)
                    && unit is IHarvestable target)
                {
                    Construct(target, Player.Player.Commander.Id, Player.Player.ListSelected, Player.Player.Include);
                }

                Player.Player.Input.Release("Select");
                Player.Player.UI.ResetCursor();
            }
        }

        public void Construct(IHarvestable target, int factionId, List<string> selection, bool inclusive)
        {
            if (NetworkManager.Singleton.IsServer)
                ConstructCommandletServer(target, factionId, selection, inclusive);
            else
                ConstructCommandletServerRpc(target.GameObject.name, factionId, selection.ToNativeArray32(), inclusive);
        }

        [Rpc(SendTo.Server)]
        private void ConstructCommandletServerRpc(string target, int factionId, NativeArray<FixedString32Bytes> selection, bool inclusive)
        {
            if (!EntityCache.TryGetEntityComponent(target, out IHarvestable unit))
            {
                Debug.LogError($"Invalid target entity {target} for {Name} Command! Command being ignored!");
                return;
            }

            ConstructCommandletServer(unit, factionId, selection.ToStringList(), inclusive);
        }

        private void OnOrder(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (context.canceled) CancelSelection();
        }

        public override CostEntry[] GetCost() => new CostEntry[0];

        public override void CancelSelection()
        {
            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
            Player.Player.UI.ResetCursor();
        }
    }
}