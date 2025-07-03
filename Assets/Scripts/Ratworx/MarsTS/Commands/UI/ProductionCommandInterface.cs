using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.UI
{
    public class ProductionCommandInterface : BaseCommandInterface,
                                              ICommandInterfaceArgumentAccepter<ProductionOption>
    {
        public override void StartSelection() {
            throw new NotImplementedException(
                $"Wrong method called! Should be calling {nameof(StartArgSelection)} on {nameof(ProductionCommandInterface)}!");
        }

        public override void CancelSelection() { }
        
        public string GetArgDescription(ProductionOption arg) => arg.Description;

        public Sprite GetArgIcon(ProductionOption arg) {
            if (!Registry.Registry.TryGetPrefab(arg.ProductKey, out GameObject prefab))
                RatLogger.Error?.Log($"Couldn't find registered prefab {arg.ProductKey} for icon!");

            // TODO: Create an image cache so we don't have to keep getting components
            return prefab.GetComponent<ISelectable>().Icon;
        }

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

                if (lowestCommandable is null)
                    return;

                WithdrawResourcesFromFaction(arg.Cost, Player.Player.Commander);

                CommandPrimer.GetFactory<CommandFactory<ProductionOption>>()
                    .ConstructCommand(
                        CommandKey,
                        arg,
                        Player.Player.Commander,
                        new[] { lowestCommandable.Entity.Id },
                        Player.Player.Include
                    );
            }
        }

        private void WithdrawResourcesFromFaction(ResourceCost[] costs, Faction faction) {
            foreach (ResourceCost entry in costs) {
                if (entry.key == "time") continue;
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }
        }
    }
}