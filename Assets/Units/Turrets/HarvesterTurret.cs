using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Units {

    public class HarvesterTurret : MonoBehaviour {

        //This is how many units per second
        [SerializeField]
        private int harvestRate;

		private int harvestAmount;

		private float cooldown;
		private float currentCooldown;

		private ResourceStorage localStorage;

		[SerializeField]
		private GameObject barrel;

		public float Range { get { return sensor.Range; } }

		private IHarvestable target;

		private ISelectable parent;
		private EventAgent bus;

		private HarvestSensor sensor;

		private void Awake () {
			parent = GetComponentInParent<ISelectable>();
			bus = GetComponentInParent<EventAgent>();
			sensor = GetComponent<HarvestSensor>();

			bus.AddListener<SensorUpdateEvent<IHarvestable>>(OnSensorUpdate);

			localStorage = GetComponentInParent<ResourceStorage>();

			cooldown = 1f / harvestRate;
			harvestAmount = (int)(harvestRate * cooldown);
		}

		private void Update () {
			if (!NetworkManager.Singleton.IsServer) return;

			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "harvest") {
				Commandlet<IHarvestable> harvestCommand = commandableUnit.CurrentCommand as Commandlet<IHarvestable>;

				if (sensor.IsDetected(harvestCommand.Target)) {
					target = harvestCommand.Target;
				}
			}

			if (target == null) {
				foreach (IHarvestable unit in sensor.Detected) {
					target = unit;
					break;
				}
			}

			if (target != null && sensor.IsDetected(target) && currentCooldown <= 0) {
				Harvest();
			}
		}

		private void FixedUpdate () {
			if (!NetworkManager.Singleton.IsServer) return;

			if (target != null && sensor.IsDetected(target)) {
				barrel.transform.LookAt(target.GameObject.transform, Vector3.up);
			}
		}

		private void Harvest () {
			IHarvestable harvestable = target as IHarvestable;

			int harvested = harvestable.Harvest("resource_unit", parent, harvestAmount, localStorage.Submit);
			bus.Global(new ResourceHarvestedEvent(bus, harvestable, parent, ResourceHarvestedEvent.Side.Harvester, harvested, "resource_unit", localStorage.Amount, localStorage.Capacity));

			currentCooldown += cooldown;
		}

		private void OnSensorUpdate (SensorUpdateEvent<IHarvestable> _event) {
			if (_event.Detected == true) {
				if (target == null) {
					target = _event.Target;
				}
			}
			else if (ReferenceEquals(_event.Target, target)) {
				target = null;
			}
		}

		public bool IsInRange (IHarvestable target) {
			return sensor.IsDetected(target);
		}
	}
}