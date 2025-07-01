using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands.UI
{
    public class SneakCommandInterface : BaseCommandInterface
    {
        public override void StartSelection() {
            int totalWithSneak = 0;
            int totalSneakActive = 0;

            //Inspect all selected to make all units using this ability match up with others that are active using
            foreach (Roster rollup in Player.Player.Selected.Values) {
                if (rollup.GetCommandKeys().Contains(CommandKey)) {
                    totalWithSneak += rollup.Count;

                    foreach (ICommandable unit in rollup.GetCommandables()) {
                        if (unit.ActiveCommands.Count == 0) continue;

                        foreach (ICommandReceiver activeCommand in unit.ActiveCommands) {
                            if (activeCommand.CommandKey == CommandKey) totalSneakActive++;
                        }
                    }
                }
            }

            CommandPrimer.GetFactory<CommandFactory<bool>>().ConstructCommand(
                CommandKey,
                totalWithSneak > totalSneakActive,
                Player.Player.Commander,
                Player.Player.ListSelected,
                Player.Player.Include
            );
        }

        public override void CancelSelection() {
            throw new System.NotImplementedException();
        }
    }
}