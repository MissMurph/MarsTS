using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.UI
{
    public class ProductionCommandUserInterface : BaseCommandUserInterface,
                                                  ICommandUserInterfaceArgumentAccepter<ProductionOption>
    {
        public override void StartSelection() {
            throw new System.NotImplementedException();
        }

        public override void CancelSelection() {
            throw new System.NotImplementedException();
        }

        public override Sprite GetIcon() => base.GetIcon();

        public string GetArgDescription(ProductionOption arg) => throw new System.NotImplementedException();

        public Sprite GetArgIcon(ProductionOption arg) => throw new System.NotImplementedException();

        public void StartArgSelection(ProductionOption arg) {
            if (!arg.CanFactionAfford(Player.Player.Commander)) return;

            foreach (KeyValuePair<string, Roster> entry in Player.Player.Selected) {
                int lowestAmount = 9999;
                ICommandable lowestCommandable = null;

                foreach (ICommandable commandable in entry.Value.GetCommandables()) {
                    if (!commandable.CanCommand(CommandKey)
                        || commandable.QueueCount >= lowestAmount)
                        continue;

                    lowestAmount = commandable.QueueCount;
                    lowestCommandable = commandable;
                }

                // if (lowestCommandable != null)
                // ConstructProductionletServerRpc(Player.Player.Commander.Id, lowestCommandable.GameObject.name);
            }
        }
        
        
    }
}