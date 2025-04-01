using System;
using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.World;
using UnityEngine;

namespace MarsTS.Units
{
    public class RoughneckSquad : InfantrySquad
    {
        public int Stored => storageComp.Amount;

        public int Capacity => storageComp.Capacity;

        public ResourceStorage storageComp;

        private Transform resourceBar;

        protected override void Awake()
        {
            base.Awake();

            resourceBar = transform.Find("BarOrientation");
            storageComp = GetComponent<ResourceStorage>();
        }

        protected override void Update()
        {
            base.Update();

            foreach (MemberEntry entry in _members.Values)
            {
                resourceBar.transform.position = entry.Member.transform.position;
                break;
            }
        }

        protected override void AttachMemberServerListeners(InfantryMember unit)
        {
            base.AttachMemberServerListeners(unit);

            EventAgent unitEvents = unit.GetComponent<EventAgent>();
            
            unitEvents.AddListener<ResourceHarvestedEvent>(OnMemberHarvest);
            unitEvents.AddListener<HarvesterDepositEvent>(OnMemberDeposit);
        }

        public void OnMemberHarvest(ResourceHarvestedEvent _event)
        {
            _bus.Global(new ResourceHarvestedEvent(_bus, this, ResourceHarvestedEvent.Side.Harvester,
                _event.HarvestAmount, _event.Resource, Stored, Capacity));
        }

        public void OnMemberDeposit(HarvesterDepositEvent _event)
        {
            _bus.Global(new HarvesterDepositEvent(_bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
                _event.Bank));
        }

        public override void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "sneak":
                    SquadSneak(order);
                    return;
                case "attack":

                    break;
                case "repair":

                    break;
                case "harvest":

                    break;
                case "deposit":

                    break;
                default:
                    base.Order(order, inclusive);
                    return;
            }

            if (inclusive) _commands.Enqueue(order);
            else _commands.Execute(order);
        }

        private void SquadSneak(Commandlet order)
        {
            if (!CanCommand(order.Name)) return;
            var deserialized = order as Commandlet<bool>;

            _commands.Activate(order, deserialized.Target);

            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Order(order, false);
            }
        }

        public override CommandFactory Evaluate(ISelectable target)
        {
            if (target is IHarvestable harvestable
                && Stored < Capacity
                && harvestable.StoredAmount > 0
                && harvestable.CanHarvest(storageComp.Resource, this))
                return CommandPrimer.Get("harvest");

            if (target is IDepositable
                && Stored > 0)
                return CommandPrimer.Get("deposit");

            if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile)
                return CommandPrimer.Get("attack");

            return CommandPrimer.Get("move");
        }

        public override void AutoCommand(ISelectable target)
        {
            if (target is IHarvestable harvestable
                && Stored < Capacity
                && harvestable.StoredAmount > 0
                && harvestable.CanHarvest(storageComp.Resource, this))
            {
                //return CommandRegistry.Get<Harvest>("harvest").Construct(harvestable);
            }

            if (target is IDepositable depositable
                && Stored > 0)
            {
                CommandPrimer.Get<Deposit>("deposit").Construct(depositable);
            }

            if (target is IAttackable attackable && target.GetRelationship(Owner) == Relationship.Hostile)
            {
                CommandPrimer.Get<Attack>("attack").Construct(attackable);
            }

            CommandPrimer.Get<Move>("move").Construct(target.GameObject.transform.position);
        }

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                base.OnUnitInfoDisplayed(_event);

                UnitResourceStorageInfo storage = _event.Info.Module<UnitResourceStorageInfo>("storage");
                storage.SetStorage(storageComp);
            }
        }

        public override bool CanCommand(string key)
        {
            if (key == "deposit") return true;

            return base.CanCommand(key);
        }
    }
}