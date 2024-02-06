using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
using UnityEngine.SocialPlatforms.Impl;

namespace MarsTS.Buildings {

	public class Pumpjack : Building, IHarvestable {

		/*	IHarvestable Properties	*/

		public int OriginalAmount { get { return capacity; } }

		[SerializeField]
		private int capacity;

		public int StoredAmount { get { return stored; } }

		private int stored;

		/*	Pumpjack Fields	*/

		private OilDeposit exploited;

		[SerializeField]
		private int harvestRate;

		private int harvestAmount;
		private float cooldown;
		private float currentCooldown;

		private GameObject oilDetector;

		protected override void Awake () {
			base.Awake();

			cooldown = 1f / harvestRate;
			harvestAmount = (int)(harvestRate * cooldown);
			currentCooldown = cooldown;

			oilDetector = transform.Find("OilCollider").gameObject;
			oilDetector.SetActive(false);
		}

		protected override void Start () {
			base.Start();

			bus.AddListener<PumpjackExploitInitEvent>(OnExploitInit);
			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		protected void Update () {
			if (Constructed && exploited != null) {
				currentCooldown -= Time.deltaTime;

				if (currentCooldown <= 0) {
					int harvested = exploited.Harvest("oil", this, harvestAmount, Pump);
					bus.Global(new ResourceHarvestedEvent(bus, exploited, this, ResourceHarvestedEvent.Side.Harvester, harvested, "oil", stored, capacity));

					currentCooldown += cooldown;
				}
			}
		}

		private void OnEntityInit (EntityInitEvent _event) {
			oilDetector.SetActive(true);
		}

		private void OnExploitInit (PumpjackExploitInitEvent _event) {
			exploited = _event.Oil;
			exploited.Exploited = true;
		}

		public virtual bool CanHarvest (string resourceKey, ISelectable unit) {
			return resourceKey == "oil" && (unit.Owner == Owner || unit.UnitType == "roughneck");
		}

		private int Pump (int amount) {
			int newAmount = Mathf.Min(capacity, stored + amount);

			int difference = newAmount - stored;

			stored = newAmount;

			return difference;
		}

		public virtual int Harvest (string resourceKey, ISelectable harvester, int harvestAmount, Func<int, int> extractor) {
			if (CanHarvest(resourceKey, harvester)) {
				int availableAmount = Mathf.Min(harvestAmount, stored);

				int finalAmount = extractor(availableAmount);

				if (finalAmount > 0) {
					bus.Global(new ResourceHarvestedEvent(bus, this, harvester, ResourceHarvestedEvent.Side.Deposit, finalAmount, "oil", stored, capacity));
					stored -= finalAmount;
				}

				return finalAmount;
			}

			return 0;
		}

		protected override void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			base.OnUnitInfoDisplayed(_event);

			if (ReferenceEquals(_event.Unit, this)) {
				StorageInfo info = _event.Info.Module<StorageInfo>("storage");
				info.CurrentUnit = this;
				info.CurrentValue = stored;
				info.MaxValue = capacity;
			}
		}

		private void OnProductionStart (ProductionEvent _event) {
			if (_event.Name == "productionStarted") {
				
			}
		}

		private void OnProductionComplete (ProductionCompleteEvent _event) {

		}

		private void OnDestroy () {
			if (exploited != null) exploited.Exploited = false;
		}
	}
}