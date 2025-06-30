using System.Collections.Generic;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands.UI
{
    public class DeployCommandUserInterface : BaseCommandUserInterface
    {
        public override void StartSelection() {
            var toCommand = new List<int>();

            foreach (Roster rollup in Player.Player.Selected.Values) {
                if (!rollup.GetCommands().Contains(CommandKey)) 
                    continue;
				
                foreach (ICommandable unit in rollup.GetCommandables()) {
                    if (unit.ActiveCommands.Count == 0) continue;
                    if (unit.Commands()[CommandKey].IsActive) continue;

                    toCommand.Add(unit.Entity.Id);
                }
            }

            CommandPrimer.GetFactory<CommandFactory<bool>>()
                .ConstructCommand(
                    CommandKey,
                    true,
                    Player.Player.Commander,
                    toCommand,
                    Player.Player.Include
                );
        }

        public override void CancelSelection() {
            throw new System.NotImplementedException();
        }
    }
}