using System.Collections.Generic;
using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Networking;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands
{
    public class Deposit : CommandFactory<IDepositable>
    {
        public override string Name => "deposit";

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
            if (!context.canceled) return;

            Vector2 cursorPos = Player.MousePos;
            Ray ray = Player.ViewPort.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask) &&
                EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable target) &&
                target is IDepositable depositable)
                Construct(depositable);

            Player.Input.Release("Select");
            Player.UI.ResetCursor();
        }

        public void Construct(IDepositable target) {
            ConstructCommandletServerRpc(
                target.GameObject.name,
                Player.Commander.Id,
                Player.ListSelected.ToNativeArray32(),
                Player.Include
            );
        }

        [Rpc(SendTo.Server)]
        private void ConstructCommandletServerRpc(
            string target, 
            int factionId,
            NativeArray<FixedString32Bytes> selection, 
            bool inclusive)
        {
            if (!EntityCache.TryGet(target, out IDepositable unit))
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