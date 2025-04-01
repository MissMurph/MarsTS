using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Units;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class Repair : CommandFactory<IAttackable>
    {
        public override string Name => "repair";

        public override string Description => description;

        [SerializeField] private string description;

        public override void StartSelection()
        {
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);
            Player.Player.UI.SetCursor(Pointer);
        }

        private void OnSelect(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (!context.canceled) return;

            Vector2 cursorPos = Player.Player.MousePos;
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)
                && EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable target)
                && target is IAttackable attackable
               )
                Construct(attackable, Player.Player.Commander.Id, Player.Player.ListSelected, Player.Player.Include);

            Player.Player.Input.Release("Select");
            Player.Player.UI.ResetCursor();
        }

        private void OnOrder(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (context.canceled) CancelSelection();
        }

        public void Construct(IAttackable target, int factionId, List<string> selection, bool inclusive)
        {
            if (NetworkManager.Singleton.IsServer)
                ConstructCommandletServer(target, factionId, selection, inclusive);
            else
                ConstructCommandletServerRpc(target.GameObject.name, factionId, selection.ToNativeArray32(), inclusive);
        }

        [Rpc(SendTo.Server)]
        private void ConstructCommandletServerRpc(string target, int factionId,
            NativeArray<FixedString32Bytes> selection, bool inclusive)
        {
            if (!EntityCache.TryGet(target, out IAttackable unit))
            {
                Debug.LogError($"Invalid target entity {target} for {Name} Command! Command being ignored!");
                return;
            }

            ConstructCommandletServer(unit, factionId, selection.ToStringList(), inclusive);
        }

        public override CostEntry[] GetCost() => Array.Empty<CostEntry>();

        public override void CancelSelection()
        {
            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
            Player.Player.UI.ResetCursor();
        }
    }
}