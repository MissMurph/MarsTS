using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class ProduceCommandFactory : CommandFactory<ProductionOption>
    {
        public override string Name => _commandKey;
        protected virtual string CommandKey => _commandKey;
        public override Sprite Icon => icon;

        public override string Description => _description;

        [FormerlySerializedAs("description")]
        [SerializeField]
        protected string _description;

        
        [SerializeField] private string _commandKey = "produce";

        public override void StartSelection() {
            
        }
        
        

        //We create separate calls for now since Productionlets are different to normal commands
        //This is due to having to serialize GameObject as a target when we don't need to
        [Rpc(SendTo.Server)]
        protected virtual void ConstructProductionletServerRpc(SerializedProductionOption productionOption, int factionId, int selection) {
            ConstructProductionletServer(productionOption.GetDeserializedOption(), factionId, selection);
        }

        protected virtual void ConstructProductionletServer(ProductionOption productionOption, int factionId, int selection) {
            Faction faction = TeamCache.Faction(factionId);

            if (!productionOption.CanFactionAfford(faction))
                return;

            ProduceCommandlet order = Instantiate(OrderPrefab) as ProduceCommandlet;

            order.Init(Name, productionOption, TeamCache.Faction(factionId));

            
            
            if (EntityCache.TryGetEntity(selection, out Entity entity)
                && entity.TryGetEntityComponent(out ICommandable unit))
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

        protected void WithdrawResourcesFromFaction(Faction faction) {
            /*foreach (ResourceCost entry in _cost) {
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }*/
        }
    }
}