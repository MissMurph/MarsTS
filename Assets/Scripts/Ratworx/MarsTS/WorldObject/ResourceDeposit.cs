using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using UnityEngine;

namespace Ratworx.MarsTS.WorldObject
{
    public class ResourceDeposit : EntityAttribute, 
                                   IHarvestable
    {
        //This is just for the registry key, some examples:
        //deposit:scrap
        //deposit:oil_slick
        //deposit:shale_oil
        //deposit:biomass
        //deposit:rock
        [SerializeField] private string _depositType;

        // protected ResourceStorage _resourceStorage;
        
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
        public int OriginalAmount { get; private set; }

        public int StoredAmount => Value;
        public string Resource { get; }

        private EventAgent _eventAgent;
        private Entity _entity;

        protected virtual void Awake() {
            _entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
        }

        private void Start() {
            OriginalAmount = Value;
            // EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
            OnAttributeChange += OnResourceExtracted;
        }

        private void OnResourceExtracted(int oldValue, int newValue) {
            _eventAgent.PostGlobal(
                new ResourceHarvestedEvent(
                    _entity,
                    ResourceHarvestedEvent.Side.Deposit,
                    newValue - oldValue,
                    _depositType,
                    StoredAmount,
                    OriginalAmount
                )
            );

            if (Value > 0) return;
            
            _eventAgent.PostGlobal(new UnitDeathEvent(_entity));
            Destroy(gameObject, 0.01f);
        }

        public bool CanHarvest(string resourceKey, Entity unit) => resourceKey == _depositType;

        public virtual int Harvest(
            string resourceKey,
            Entity harvester,
            int harvestAmount,
            Func<int, int> extractor
        ) {
            int availableAmount = Mathf.Min(harvestAmount, Value);

            int finalAmount = extractor(availableAmount);
            Value -= finalAmount;

            

            return finalAmount;
        }

        /*private void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("deposit");
                info.SetStorage(_resourceStorage);
            }
        }*/
    }
}