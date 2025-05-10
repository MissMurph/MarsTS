using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class RoughneckSquad : InfantrySquad
    {
        public int Stored => storageComp.Value;

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

        /*public void OnMemberHarvest(ResourceHarvestedEvent _event)
        {
            _bus.PostGlobal(new ResourceHarvestedEvent(_bus, this, ResourceHarvestedEvent.Side.Harvester,
                _event.HarvestAmount, _event.Resource, Stored, Capacity));
        }

        public void OnMemberDeposit(HarvesterDepositEvent _event)
        {
            _bus.PostGlobal(new HarvesterDepositEvent(_bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
                _event.Bank));
        }*/

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

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                base.OnUnitInfoDisplayed(_event);

                UnitResourceStorageInfo storage = _event.Info.Module<UnitResourceStorageInfo>("storage");
                storage.SetStorage(storageComp);
            }
        }
    }
}