using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
			UpdateCommands();

			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];
				float distance = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).magnitude;

				if (distance <= waypointCompletionDistance) {
					pathIndex++;
				}

				if (pathIndex >= currentPath.Length) {
					bus.Local(new PathCompleteEvent(bus, true));
					currentPath = Path.Empty;
				}
			}

			if (DepositTarget != null) {
				if (depositableDetector.IsInRange(DepositTarget)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentCooldown <= 0f) DepositResources();

					currentCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, DepositTarget.GameObject.transform)) {
					SetTarget(DepositTarget.GameObject.transform);
				}

				return;
			}

			if (HarvestTarget != null) {
				if (harvestableDetector.IsInRange(HarvestTarget)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentHarvestCooldown <= 0f) SiphonOil();

					currentHarvestCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform)) {
					SetTarget(HarvestTarget.GameObject.transform);
				}
			}
		}

		private void SiphonOil () {
			int harvested = HarvestTarget.Harvest("oil", this, harvestAmount, storageComp.Submit);
			bus.Global(new HarvesterExtractionEvent(bus, this, Stored, Capacity, HarvestTarget));

			currentHarvestCooldown += harvestCooldown;
		}
	}
}