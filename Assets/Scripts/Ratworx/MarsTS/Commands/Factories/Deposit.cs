using Ratworx.MarsTS.Buildings;
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
    public class Deposit : CommandFactory<IDepositable>
    {
        public override string Name => "deposit";

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
            if (!context.canceled) return;

            Vector2 cursorPos = Player.Player.MousePos;
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask) &&
                EntityCache.TryGetEntityComponent(hit.collider.transform.parent.name + ":selectable", out ISelectable target) &&
                target is IDepositable depositable)
                Construct(depositable);

            Player.Player.Input.Release("Select");
            Player.Player.UI.ResetCursor();
        }

        public void Construct(IDepositable target) {
            ConstructCommandletServerRpc(
                target.GameObject.name,
                Player.Player.Commander.Id,
                Player.Player.ListSelected.ToNativeArray32(),
                Player.Player.Include
            );
        }

        [Rpc(SendTo.Server)]
        private void ConstructCommandletServerRpc(
            string target, 
            int factionId,
            NativeArray<FixedString32Bytes> selection, 
            bool inclusive)
        {
            if (!EntityCache.TryGetEntityComponent(target, out IDepositable unit))
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