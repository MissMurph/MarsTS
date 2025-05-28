using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class Produce : CommandFactory<string>
    {
        public override string Name => $"{CommandKey}/{_productRegistryKey}";
        protected virtual string CommandKey => _commandKey;
        public override Sprite Icon => _unit.Icon;

        public override string Description => _description;

        [FormerlySerializedAs("description")]
        [SerializeField]
        protected string _description;

        protected string _productRegistryKey;

        private ISelectable _unit { get; set; }

        [SerializeField] private string _commandKey = "produce";

        public override void StartSelection() {
            if (!CanFactionAfford(Player.Player.Commander)) return;

            foreach (KeyValuePair<string, Roster> entry in Player.Player.Selected) {
                int lowestAmount = 9999;
                ICommandable lowestCommandable = null;

                foreach (ICommandable commandable in entry.Value.Orderable) {
                    if (!commandable.CanCommand(Name)
                        || commandable.Count >= lowestAmount)
                        continue;

                    lowestAmount = commandable.Count;
                    lowestCommandable = commandable;
                }

                if (lowestCommandable != null)
                    ConstructProductionletServerRpc(Player.Player.Commander.Id, lowestCommandable.GameObject.name);
            }
        }

        //We create separate calls for now since Productionlets are different to normal commands
        //This is due to having to serialize GameObject as a target when we don't need to
        [Rpc(SendTo.Server)]
        protected virtual void ConstructProductionletServerRpc(int factionId, string selection) {
            ConstructProductionletServer(factionId, selection);
        }

        protected virtual void ConstructProductionletServer(int factionId, string selection) {
            Faction faction = TeamCache.Faction(factionId);

            if (!CanFactionAfford(faction))
                return;

            ProduceCommandlet order = Instantiate(orderPrefab) as ProduceCommandlet;

            order.InitProduce(Name, CommandKey, _productRegistryKey, TeamCache.Faction(factionId));

            if (EntityCache.TryGetEntityComponent(selection, out ICommandable unit))
                unit.Order(order, true);
            else
                RatLogger.Error?.Log($"Failed to find selected entity {selection} for command {Name}");

            WithdrawResourcesFromFaction(faction);
        }

        public override ResourceCost[] GetCost() {
            /*List<ResourceCost> spool = _cost.ToList();

            ResourceCost time = new ResourceCost
            {
                key = "time",
                amount = _timeRequired
            };

            spool.Add(time);

            return spool.ToArray();*/
            return Array.Empty<ResourceCost>();
        }

        public override void CancelSelection() { }

        protected bool CanFactionAfford(Faction faction)
            // => !_cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);
            => true;

        protected void WithdrawResourcesFromFaction(Faction faction) {
            /*foreach (ResourceCost entry in _cost) {
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }*/
        }
    }
}