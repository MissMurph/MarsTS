using System;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.WorldObject
{
    public class OilDeposit : ResourceDeposit
    {
        private GameObject _selectionCollider;
        private GameObject _resourceBars;

        public bool Exploited
        {
            get => _exploited;
            set
            {
                _exploited = value;
                _selectionCollider.SetActive(!_exploited);
                _resourceBars.SetActive(!_exploited);
            }
        }

        private bool _exploited;

        protected override void Awake()
        {
            base.Awake();
            _selectionCollider = transform.Find("SelectionCollider").gameObject;
            _resourceBars = transform.Find("BarOrientation").gameObject;
        }

        public override int Harvest(string resourceKey, Entity harvester, int harvestAmount, Func<int, int> extractor) {
            // TODO: These checks should probably be done by the harvester
            // if (harvester is Pumpjack)
            // {
            int availableAmount = Mathf.Min(harvestAmount, _resourceStorage.Value);

            int finalAmount = extractor(availableAmount);

            if (finalAmount > 0) {
                _bus.PostGlobal(new ResourceHarvestedEvent(_bus, this, ResourceHarvestedEvent.Side.Deposit, finalAmount,
                    resourceKey, StoredAmount, OriginalAmount));
                _resourceStorage.Value -= finalAmount;
            }

            if (StoredAmount <= 0) {
                _bus.PostGlobal(new UnitDeathEvent(_bus, this));
                Destroy(gameObject, 0.01f);
            }

            return finalAmount;
            // }

            return 0;
        }
    }
}