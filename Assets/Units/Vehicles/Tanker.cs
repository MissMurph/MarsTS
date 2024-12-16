using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Tanker : Harvester {

        private HarvestSensor harvestableDetector;

        //This is how many units per second are harvested
        [SerializeField]
        private float harvestRate;

		private int harvestAmount;
		private float harvestCooldown;
		private float currentHarvestCooldown;

		protected override void Awake () {
			base.Awake();

			harvestableDetector = GetComponentInChildren<HarvestSensor>();

			harvestCooldown = 1f / harvestRate;
			harvestAmount = Mathf.RoundToInt(harvestRate * harvestCooldown);
			currentHarvestCooldown = harvestCooldown;
		}

		protected override void Update () {
			if (!CurrentPath.IsEmpty) {
				Vector3 targetWaypoint = CurrentPath[PathIndex];
				float distance = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).magnitude;

				if (distance <= waypointCompletionDistance) {
					PathIndex++;
				}

				if (PathIndex >= CurrentPath.Length) {
					Bus.Local(new PathCompleteEvent(Bus, true));
					CurrentPath = Path.Empty;
				}
			}

			if (DepositTarget != null) {
				if (_depositableDetector.IsDetected(DepositTarget)) {
					TrackedTarget = null;
					CurrentPath = Path.Empty;

					if (_currentCooldown <= 0f) DepositResources();

					_currentCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, DepositTarget.GameObject.transform)) {
					SetTarget(DepositTarget.GameObject.transform);
				}

				return;
			}

			if (HarvestTarget != null) {
				if (harvestableDetector.IsDetected(HarvestTarget)) {
					TrackedTarget = null;
					CurrentPath = Path.Empty;

					if (currentHarvestCooldown <= 0f) SiphonOil();

					currentHarvestCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform)) {
					SetTarget(HarvestTarget.GameObject.transform);
				}
			}
		}

		private void SiphonOil () {
			int harvested = HarvestTarget.Harvest("oil", this, harvestAmount, _storageComp.Submit);
			Bus.Global(new ResourceHarvestedEvent(Bus, HarvestTarget, this, ResourceHarvestedEvent.Side.Harvester, harvested, "oil", Stored, Capacity));

			currentHarvestCooldown += harvestCooldown;
		}

		protected override void DepositResources () {
			_storageComp.Consume(DepositTarget.Deposit("oil", _depositAmount));
			Bus.Global(new HarvesterDepositEvent(Bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity, DepositTarget));
			_currentCooldown += _cooldown;
		}
	}
}