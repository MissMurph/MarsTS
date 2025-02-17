using System.Collections.Generic;
using MarsTS.Entities;
using MarsTS.Networking;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace MarsTS.Commands
{
    public class Harvest : CommandFactory<IHarvestable>
    {
        public override string Name => "harvest";

        public override string Description => _description;

        [SerializeField] private string _description;

        public override void StartSelection()
        {
            Player.Input.Hook("Select", OnSelect);
            Player.Input.Hook("Order", OnOrder);
            Player.UI.SetCursor(Pointer);
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (context.canceled)
            {
                Vector2 cursorPos = Player.MousePos;
                Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)
                    && EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable unit)
                    && unit is IHarvestable target)
                {
                    Construct(target, Player.Commander.Id, Player.ListSelected, Player.Include);
                }

                Player.Input.Release("Select");
                Player.UI.ResetCursor();
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
            if (!EntityCache.TryGet(target, out IHarvestable unit))
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
            Player.Input.Release("Select");
            Player.Input.Release("Order");
            Player.UI.ResetCursor();
        }
    }
}