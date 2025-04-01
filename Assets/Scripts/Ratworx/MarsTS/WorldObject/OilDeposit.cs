using System;
using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.World
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

        public override int Harvest(string resourceKey, ISelectable harvester, int harvestAmount,
            Func<int, int> extractor)
        {
            if (harvester is Pumpjack)
            {
                int availableAmount = Mathf.Min(harvestAmount, _resourceStorage.Amount);

                int finalAmount = extractor(availableAmount);

                if (finalAmount > 0)
                {
                    _bus.Global(new ResourceHarvestedEvent(_bus, this, ResourceHarvestedEvent.Side.Deposit, finalAmount,
                        resourceKey, StoredAmount, OriginalAmount));
                    _resourceStorage.Consume(finalAmount);
                }

                if (StoredAmount <= 0)
                {
                    _bus.Global(new UnitDeathEvent(_bus, this));
                    Destroy(gameObject, 0.01f);
                }

                return finalAmount;
            }

            return 0;
        }
    }
}