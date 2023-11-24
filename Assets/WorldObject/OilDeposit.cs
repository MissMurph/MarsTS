using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World {

    public class OilDeposit : ResourceDeposit {

        private GameObject selectionCollider;
        private GameObject resourceBars;

        public bool Exploited {
            get {
                return exploited;
            }
            set {
                exploited = value;
                selectionCollider.SetActive(!exploited);
				resourceBars.SetActive(!exploited);

			}
        }

        private bool exploited;

        protected override void Awake () {
            base.Awake();
            selectionCollider = transform.Find("SelectionCollider").gameObject;
			resourceBars = transform.Find("BarOrientation").gameObject;
		}

		public override int Harvest (string resourceKey, ISelectable harvester, int harvestAmount, Func<int, int> extractor) {
            if (harvester is Pumpjack) {
                int availableAmount = Mathf.Min(harvestAmount, attribute.Amount);

                int finalAmount = extractor(availableAmount);

                if (finalAmount > 0) {
                    bus.Global(new ResourceHarvestedEvent(bus, this, finalAmount, resourceKey));
                    attribute.Amount -= finalAmount;
                }

                if (StoredAmount <= 0) {
                    bus.Global(new EntityDeathEvent(bus, this));
                    Destroy(gameObject, 0.01f);
                }

                return finalAmount;
            }
            else return 0;
		}
	}
}