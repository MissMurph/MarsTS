using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Vehicles
{
    public class Tanker : Harvester
    {
        private HarvestSensor _harvestableDetector;

        //This is how many units per second are harvested
        [FormerlySerializedAs("harvestRate")] [SerializeField]
        private float _harvestRate;

        private int _harvestAmount;
        private float _harvestCooldown;
        private float _currentHarvestCooldown;

        protected override void Awake()
        {
            base.Awake();

            _harvestableDetector = GetComponentInChildren<HarvestSensor>();

            _harvestCooldown = 1f / _harvestRate;
            _harvestAmount = Mathf.RoundToInt(_harvestRate * _harvestCooldown);
            _currentHarvestCooldown = _harvestCooldown;
        }

        protected override void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (!CurrentPath.IsEmpty)
            {
                Vector3 targetWaypoint = CurrentPath[PathIndex];
                float distance = new Vector3(targetWaypoint.x - transform.position.x, 0,
                    targetWaypoint.z - transform.position.z).magnitude;

                if (distance <= waypointCompletionDistance) PathIndex++;

                if (PathIndex >= CurrentPath.Length)
                {
                    Bus.Local(new PathCompleteEvent(Bus, true));
                    CurrentPath = Path.Empty;
                }
            }

            if (DepositTarget != null)
            {
                if (_depositableDetector.IsDetected(DepositTarget))
                {
                    TrackedTarget = null;
                    CurrentPath = Path.Empty;

                    if (_currentCooldown <= 0f) DepositResources();

                    _currentCooldown -= Time.deltaTime;
                }
                else if (!ReferenceEquals(TrackedTarget, DepositTarget.GameObject.transform))
                {
                    SetTarget(DepositTarget.GameObject.transform);
                }

                return;
            }

            if (HarvestTarget != null)
            {
                if (_harvestableDetector.IsDetected(HarvestTarget))
                {
                    TrackedTarget = null;
                    CurrentPath = Path.Empty;

                    if (_currentHarvestCooldown <= 0f) SiphonOil();

                    _currentHarvestCooldown -= Time.deltaTime;
                }
                else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform))
                {
                    SetTarget(HarvestTarget.GameObject.transform);
                }
            }
        }

        private void SiphonOil()
        {
            int harvested = HarvestTarget.Harvest("oil", this, _harvestAmount, _storageComp.Submit);
            Bus.Global(new ResourceHarvestedEvent(Bus, this, ResourceHarvestedEvent.Side.Harvester, harvested, "oil",
                Stored, Capacity));

            _currentHarvestCooldown += _harvestCooldown;
        }

        protected override void DepositResources()
        {
            _storageComp.Consume(DepositTarget.Deposit("oil", _depositAmount));
            Bus.Global(new HarvesterDepositEvent(Bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
                DepositTarget));
            _currentCooldown += _cooldown;
        }
    }
}