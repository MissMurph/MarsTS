using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Turrets
{
    [RequireComponent(typeof(AttackableSensor))]
    public class ProjectileTurret : NetworkBehaviour,
                                    IEntityServerUpdate,
                                    IEntityClientUpdate
    {
        [SerializeField] protected float Cooldown;
        
        [SerializeField] private int _damage;
        [SerializeField] private AttackableSensor _sensor;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _barrel;
        // [SerializeField] private GameObject _rangeIndicator;

        protected float CurrentCooldown;
        protected IAttackable TrackedTarget;
        
        private Quaternion _startingBarrelRotation;
        private EventAgent _eventAgent;
        private UnitTargetManager _unitTargeting;
        private UnitOwnership _ownership;
        private Entity _entity;

        private void Awake() {
            _entity = GetComponentInParent<Entity>();
            _eventAgent = GetComponentInParent<EventAgent>();
            _unitTargeting = GetComponentInParent<UnitTargetManager>();
            _ownership = GetComponentInParent<UnitOwnership>();
            _sensor = GetComponent<AttackableSensor>();

            _startingBarrelRotation = _barrel.transform.localRotation;
        }

        private void Start() {
            _sensor.OnUnitDetected += OnUnitDetected;
        }

        private void OnDisable() {
            TrackedTarget = null;
        }

        private void OnUnitDetected(IAttackable unit, bool detected) {
            if (unit.GetRelationship(_ownership.Owner) != Relationship.Hostile) 
                return;

            if (!detected && TrackedTarget == unit) {
                TrackedTarget = GetClosestDetected();
                return;
            }

            if (_unitTargeting.TargetUnit is IAttackable && unit == _unitTargeting.TargetUnit) {
                TrackedTarget = unit;
                return;
            }
            
            if (TrackedTarget != null) return;

            if (detected) 
                TrackedTarget = unit;
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

        public virtual void UpdateServer() {
            if (CurrentCooldown > 0f) 
                CurrentCooldown -= Time.deltaTime;
            
            if (TrackedTarget == null) 
                return;

            if (CurrentCooldown > 0f)
                return;
            
            FireProjectile(TrackedTarget.GameObject.transform.position);
            CurrentCooldown += Cooldown;
        }

        public void UpdateClient() {
            if (TrackedTarget != null)
                _barrel.transform.LookAt(
                    _sensor.GetDetectedCollider(TrackedTarget.GameObject.name).transform.position);
            else
                _barrel.transform.rotation = _startingBarrelRotation;
        }

        // TODO: Investigate this _always_ hitting by tracking transform
        protected virtual void FireProjectile(Vector3 position) {
            if (NetworkManager.Singleton.IsServer) 
                FireProjectileClientRpc(position);

            // Vector3 direction = (position - transform.position).normalized;

            Projectile bullet =
                Instantiate(_projectilePrefab, _barrel.transform.position, Quaternion.Euler(Vector3.zero))
                    .GetComponent<Projectile>();

            bullet.transform.LookAt(position);

            bullet.Init(_ownership.Owner, OnHit);
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