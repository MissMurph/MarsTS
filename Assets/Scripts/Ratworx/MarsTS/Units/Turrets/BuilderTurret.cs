using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets 
{
	// TODO: Lots of shared code between this and projectile turret, we oughta make an abstract turret
	[RequireComponent(typeof(AttackableSensor))]
    public class BuilderTurret : MonoBehaviour, 
								 IEntityServerUpdate, 
								 IEntityClientUpdate
	{
		[SerializeField] private int _repairRate;
		[SerializeField] private GameObject _barrel;

		[SerializeField] private int _repairAmount;
		[SerializeField] private float _cooldown;
		private float _currentCooldown;
		// TODO: Investigate if we can move a unit out of game bounds to trigger this to clear safely
		private IAttackable _trackedTarget;

		private Entity _parentEntity;
		private EventAgent _eventAgent;
		private AttackableSensor _sensor;
		private UnitOwnership _ownership;
		private UnitTargetManager _unitTargeting;
		private Quaternion _startingBarrelRotation;

		private void Awake () {
			_parentEntity = GetComponentInParent<Entity>();
			_eventAgent = GetComponentInParent<EventAgent>();
			_sensor = GetComponent<AttackableSensor>();
			_ownership = GetComponentInParent<UnitOwnership>();
			_unitTargeting = GetComponentInParent<UnitTargetManager>();
			
			// _cooldown = 1f / _repairRate;
			// _repairAmount = (int)(_repairRate * _cooldown);

			_startingBarrelRotation = _barrel.transform.localRotation;
		}

		private void Start() {
			_sensor.OnUnitDetected += OnUnitDetected;
		}

		private void OnDestroy() {
			_sensor.OnUnitDetected -= OnUnitDetected;
		}

		private void OnDisable() {
			_trackedTarget = null;
		}

		private void OnUnitDetected(IAttackable unit, bool detected) {
			if (unit.GetRelationship(_ownership.Owner) != Relationship.Owned
				&& unit.GetRelationship(_ownership.Owner) != Relationship.Friendly) 
				return;

			if (!detected && _trackedTarget == unit) {
				_trackedTarget = GetClosestDetected();
				return;
			}

			if (_unitTargeting.TargetUnit is IAttackable && unit == _unitTargeting.TargetUnit) {
				_trackedTarget = unit;
				return;
			}
            
			if (_trackedTarget != null) return;

			if (detected) 
				_trackedTarget = unit;
		}
		
		private IAttackable GetClosestDetected() {
			float distance = _sensor.Range * _sensor.Range;
			IAttackable currentClosest = null;

			foreach (IAttackable unit in _sensor.Detected) {
				if (unit.GetRelationship(_ownership.Owner) != Relationship.Owned
					&& unit.GetRelationship(_ownership.Owner) != Relationship.Friendly) 
					continue;
                
				float newDistance =
					Vector3.Distance(_sensor.GetDetectedCollider(unit.GameObject.name).transform.position, 
						transform.position);

				if (newDistance < distance) currentClosest = unit;
			}

			return currentClosest;
		}
		
		public void UpdateServer() {
			if (_currentCooldown > 0f) 
				_currentCooldown -= Time.deltaTime;
            
			if (_trackedTarget == null) 
				return;

			if (_currentCooldown > 0f)
				return;
            
			Repair();
			_currentCooldown += _cooldown;
		}
		
		public void UpdateClient() {
			if (_trackedTarget != null)
				_barrel.transform.LookAt(
					_sensor.GetDetectedCollider(_trackedTarget.GameObject.name).transform.position);
			else
				_barrel.transform.rotation = _startingBarrelRotation;
		}

		private void Repair () {
			_trackedTarget.Attack(-_repairAmount);
			_currentCooldown += _cooldown;
		}
	}
}