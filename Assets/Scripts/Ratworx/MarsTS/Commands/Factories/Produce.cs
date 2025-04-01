using System;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Entities;
using MarsTS.Logging;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Commands
{
    public class Produce : CommandFactory<GameObject>
    {
        public override string Name => $"{CommandKey}/{_unitPrefab.name}";
        protected virtual string CommandKey => _commandKey;
        public override Sprite Icon => _unit.Icon;

        public override string Description => _description;

        [FormerlySerializedAs("description")] [SerializeField]
        protected string _description;

        [FormerlySerializedAs("unitPrefab")] [SerializeField]
        protected GameObject _unitPrefab;

        [FormerlySerializedAs("timeRequired")] [SerializeField]
        protected int _timeRequired;

        [FormerlySerializedAs("Cost")] [SerializeField]
        protected CostEntry[] _cost;

        [SerializeField]
        protected string _productRegistryKey;

        private ISelectable _unit
        {
            get;
            set;
        }

        [SerializeField] private string _commandKey = "produce";

        private void Awake()
        {
            _unit = _unitPrefab.GetComponent<ISelectable>();
        }

        public override void StartSelection()
        {
            if (!CanFactionAfford(Player.Commander)) return;

            foreach (KeyValuePair<string, Roster> entry in Player.Selected)
            {
                int lowestAmount = 9999;
                ICommandable lowestCommandable = null;

                foreach (ICommandable commandable in entry.Value.Orderable)
                {
                    if (!commandable.CanCommand(Name)
                        || commandable.Count >= lowestAmount)
                        continue;

                    lowestAmount = commandable.Count;
                    lowestCommandable = commandable;
                }

                if (lowestCommandable != null)
                    ConstructProductionletServerRpc(Player.Commander.Id, lowestCommandable.GameObject.name);
            }
        }

        //We create separate calls for now since Productionlets are different to normal commands
        //This is due to having to serialize GameObject as a target when we don't need to
        [Rpc(SendTo.Server)]
        protected virtual void ConstructProductionletServerRpc(int factionId, string selection)
        {
            ConstructProductionletServer(factionId, selection);
        }

        protected virtual void ConstructProductionletServer(int factionId, string selection)
        {
            Faction faction = TeamCache.Faction(factionId);

            if (!CanFactionAfford(faction))
                return;

            ProduceCommandlet order = Instantiate(orderPrefab) as ProduceCommandlet;

            order.InitProduce(Name, CommandKey, _productRegistryKey,_unitPrefab, TeamCache.Faction(factionId), _timeRequired, _cost);

            if (EntityCache.TryGet(selection, out ICommandable unit)) 
                unit.Order(order, true);
            else
                RatLogger.Error?.Log($"Failed to find selected entity {selection} for command {Name}");

            WithdrawResourcesFromFaction(faction);
        }

        public override CostEntry[] GetCost()
        {
            List<CostEntry> spool = _cost.ToList();

            CostEntry time = new CostEntry
            {
                key = "time",
                amount = _timeRequired
            };

            spool.Add(time);

            return spool.ToArray();
        }

        public override void CancelSelection()
        {
        }

        protected bool CanFactionAfford(Faction faction)
            => !_cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);

        protected void WithdrawResourcesFromFaction(Faction faction)
        {
            foreach (CostEntry entry in _cost)
            {
                faction.GetResource(entry.key).Withdraw(entry.amount);
            }
        }
    }

    [Serializable]
    public class CostEntry
    {
        public string key;
        public int amount;
    }
}