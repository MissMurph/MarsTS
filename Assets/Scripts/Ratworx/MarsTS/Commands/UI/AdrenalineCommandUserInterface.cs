using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands.UI
{
    public class AdrenalineCommandUserInterface : BaseCommandUserInterface
    {
        public override void StartSelection() {
            int totalCanUse = 0;
            int totalUsing = 0;

            //Inspect all selected to make all units using this ability match up with others that are active using
            foreach (Roster roster in Player.Player.Selected.Values) {
                if (!roster.GetCommands().Contains(CommandKey)) continue;

                foreach (ICommandable unit in roster.GetCommandables()) {
                    if (unit.CanCommand(CommandKey)) totalCanUse++;

                    if (unit.ActiveCommands.Count == 0) continue;

                    foreach (ICommandReceiver activeCommand in unit.ActiveCommands) {
                        if (activeCommand.CommandKey == CommandKey) totalUsing++;
                    }
                }

                CommandPrimer.GetFactory<CommandFactory<bool>>()
                    .ConstructCommand(
                        CommandKey,
                        totalCanUse > totalUsing,
                        Player.Player.Commander,
                        Player.Player.ListSelected,
                        Player.Player.Include
                    );
            }
        }

        public override void CancelSelection() { }
    }
}