using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public class PumpjackOilStorage : ResourceStorage,
                                      IHarvestable
    {
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
        public int OriginalAmount => Capacity;
        public int StoredAmount => Value;
        
        private Entity _entity;

        protected override void Awake() {
            _entity = GetComponentInParent<Entity>();
            
            base.Awake();
        }

        public int Harvest(
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

        public bool CanHarvest(string resourceKey, Entity unit) => resourceKey == Resource;
    }
}