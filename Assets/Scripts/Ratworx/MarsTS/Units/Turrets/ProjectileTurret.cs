using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    [RequireComponent(typeof(AttackableSensor))]
    public class ProjectileTurret : NetworkBehaviour, 
                                    IEntityClientUpdate
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _cooldown;
        [SerializeField] private AttackableSensor _sensor;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _barrel;
        // [SerializeField] private GameObject _rangeIndicator;

        private float _currentCooldown;
        private IAttackable _trackedTarget;
        private Quaternion _startingBarrelRotation;

        private EventAgent _eventAgent;
        private UnitTargetManager _unitTargeting;
        private UnitOwnership _ownership;
        private UnitSelection _unitSelection;
        private Entity _entity;

        private void Awake() {
            _entity = GetComponentInParent<Entity>();
            _eventAgent = GetComponentInParent<EventAgent>();
            _unitTargeting = GetComponentInParent<UnitTargetManager>();
            _ownership = GetComponentInParent<UnitOwnership>();
            _unitSelection = GetComponentInParent<UnitSelection>();
            _sensor = GetComponent<AttackableSensor>();

            _startingBarrelRotation = _barrel.transform.localRotation;
        }

        private void Start() {
            _sensor.OnUnitDetected += OnUnitDetected;
        }

        private void OnDisable() {
            _trackedTarget = null;
        }

        private void OnUnitDetected(IAttackable unit, bool detected) {
            if (unit.GetRelationship(_ownership.Owner) != Relationship.Hostile) 
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
                if (unit.GetRelationship(_ownership.Owner) != Relationship.Hostile) 
                    continue;
                
                float newDistance =
                    Vector3.Distance(_sensor.GetDetectedCollider(unit.GameObject.name).transform.position, 
                        transform.position);

                if (newDistance < distance) currentClosest = unit;
            }

            return currentClosest;
        }

        public void UpdateClient() {
            if (_trackedTarget != null)
                _barrel.transform.LookAt(
                    _sensor.GetDetectedCollider(_trackedTarget.GameObject.name).transform.position);
            else
                _barrel.transform.rotation = _startingBarrelRotation;
        }

        protected virtual void FireProjectile(Vector3 position) {
            if (NetworkManager.Singleton.IsServer) 
                FireProjectileClientRpc(position);

            // Vector3 direction = (position - transform.position).normalized;

            Projectile bullet =
                Instantiate(_projectilePrefab, _barrel.transform.position, Quaternion.Euler(Vector3.zero))
                    .GetComponent<Projectile>();

            bullet.transform.LookAt(position);

            bullet.Init(_ownership.Owner, OnHit);

            _currentCooldown += _cooldown;
        }

        [Rpc(SendTo.NotServer)]
        private void FireProjectileClientRpc(Vector3 position) {
            FireProjectile(position);
        }

        protected virtual void OnHit(bool success, IAttackable unit) {
            if (!NetworkManager.Singleton.IsServer || !success) return;
            
            UnitAttackEvent attackEvent = new UnitAttackEvent(unit, _entity, _damage);
				
            attackEvent.Phase = Phase.Pre;
            _eventAgent.PostGlobal(attackEvent);

            // Captures modified damage
            int damage = attackEvent.Damage;
            unit.Attack(damage);
			
            attackEvent.Phase = Phase.Post;
            _eventAgent.PostGlobal(attackEvent);
        }
        
        // TODO: Convert below to selection circle
        /*private void OnSelect(UnitSelectEvent evnt)
        {
            if (_isDeployed && evnt.Status)
                _rangeIndicator.SetActive(true);
            else
                _rangeIndicator.SetActive(false);
        }

        private void OnHover(UnitHoverEvent evnt)
        {
            if (_isDeployed && evnt.Status)
                _rangeIndicator.SetActive(true);
            else
                _rangeIndicator.SetActive(false);
        }*/

        public bool IsInRange(IAttackable target) => _sensor.IsDetected(target);
    }
}