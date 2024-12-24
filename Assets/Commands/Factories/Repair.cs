using System;
using System.Collections.Generic;
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
    public class Repair : CommandFactory<IAttackable>
    {
        public override string Name => "repair";

        public override string Description => description;

        [SerializeField] private string description;

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

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)
                && EntityCache.TryGet(hit.collider.transform.parent.name + ":selectable", out ISelectable target)
                && target is IAttackable attackable
               )
                Construct(attackable, Player.Commander.Id, Player.ListSelected, Player.Include);

            Player.Input.Release("Select");
            Player.UI.ResetCursor();
        }

        private void OnOrder(InputAction.CallbackContext context)
        {
            //On Mouse Up
            if (context.canceled) CancelSelection();
        }

        public override void Construct(IAttackable target, int factionId, List<string> selection, bool inclusive)
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

            ConstructCommandletServer(unit, factionId, selection.ToList(), inclusive);
        }

        public override CostEntry[] GetCost() => Array.Empty<CostEntry>();

        public override void CancelSelection()
        {
            Player.Input.Release("Select");
            Player.Input.Release("Order");
            Player.UI.ResetCursor();
        }
    }
}