using Ratworx.MarsTS.Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Interfaces
{
    public class MoveCommandInterface : BaseCommandInterface
    {
        public override void StartSelection() {
            Player.Player.Input.Hook("Select", OnClick);
            Player.Player.Input.Hook("Order", OnOrder);
            Player.Player.UI.SetCursor(Cursor);
        }

        private void OnClick (InputAction.CallbackContext context) {
            //On Mouse Up
            if (context.canceled) {
                Vector2 cursorPos = Player.Player.MousePos;
                Ray ray = Player.Player.ViewPort.ScreenPointToRay(cursorPos);

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
                    CommandPrimer.GetFactory<CommandFactory<Vector3>>()
                        .ConstructCommand(
                            CommandKey,
                            hit.point,
                            Player.Player.Commander,
                            Player.Player.ListSelected,
                            Player.Player.Include
                        );
                }

                CancelSelection();
            }
        }

        private void OnOrder (InputAction.CallbackContext context) {
            //On Mouse Up
            if (context.canceled) {
                CancelSelection();
            }
        }

        public override void CancelSelection () {
            Player.Player.Input.Release("Select");
            Player.Player.Input.Release("Order");
            Player.Player.UI.ResetCursor();
        }
    }
}