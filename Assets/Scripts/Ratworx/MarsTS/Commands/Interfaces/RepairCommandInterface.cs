using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Interfaces
{
    public class RepairCommandInterface : BaseCommandInterface
    {
        public override void StartSelection() {
            Player.Player.Input.Hook("Select", OnSelect);
            Player.Player.Input.Hook("Order", OnOrder);
            Player.Player.UI.SetCursor(Cursor);
        }

        private void OnSelect(InputAction.CallbackContext context) {
            //On Mouse Up
            if (!context.canceled) return;

            Vector2 cursorPos = Player.Player.MousePos;
            Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)
                && EntityCache.TryGetEntityComponent(hit.rigidbody.name, out IAttackable target)
                && (target.GetRelationship(Player.Player.Commander) == Relationship.Friendly
                    || target.GetRelationship(Player.Player.Commander) == Relationship.Owned)
            ) {
                CommandPrimer.GetFactory<CommandFactory<IAttackable>>().ConstructCommand(
                    CommandKey,
                    target,
                    Player.Player.Commander,
                    Player.Player.ListSelected.ToArray(),
                    Player.Player.Include
                );
            }

            Player.Player.Input.Release("Select");
            Player.Player.UI.ResetCursor();
        }

        private void OnOrder(InputAction.CallbackContext context) {
            //On Mouse Up
            if (context.canceled) CancelSelection();
        }

        public override void CancelSelection() {
            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
            Player.Player.UI.ResetCursor();
        }
    }
}