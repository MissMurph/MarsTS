using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class ProjectileTurret : NetworkBehaviour, 
                                    IEntityServerUpdate
    {
        [SerializeField] private int _damage;
        [SerializeField] private float _cooldown;
        [SerializeField] private AttackableSensor _sensor;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _barrel;

        private float _currentCooldown;
        private EventAgent _eventAgent;
        private UnitTargetManager _unitTargeting;
        private UnitOwnership _ownership;
        private Entity _entity;
        private IAttackable _trackedTarget;

        private void Awake() {
            _entity = GetComponentInParent<Entity>();
            _eventAgent = GetComponentInParent<EventAgent>();
            _sensor = GetComponent<AttackableSensor>();
            _unitTargeting = GetComponent<UnitTargetManager>();
            _ownership = GetComponent<UnitOwnership>();
        }

        private void OnEnable() {
            _sensor.OnUnitDetected += OnUnitDetected;
        }
        
        private void OnDisable() {
            _sensor.OnUnitDetected -= OnUnitDetected;
            _trackedTarget = null;
        }

        private void OnUnitDetected(IAttackable unit, bool detected) {
            if (unit.GetRelationship(_ownership.Owner) != Relationship.Hostile) 
                return;

            if (!detected && _trackedTarget == unit) {
                _trackedTarget = GetClosestDetected();
            }

            if (_unitTargeting.TargetUnit is IAttackable
                && unit == _unitTargeting.TargetUnit)
                _trackedTarget = unit;
            
            if (_trackedTarget != null) return;
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

        public void UpdateServer() {
            
        }

        public void UpdateClient() {
            
        }

        private void FixedUpdate() {
            if (!NetworkManager.Singleton.IsServer) return;
            
            if (_target != null && _sensor.IsDetected(_target))
                _barrel.transform.LookAt(_sensor.GetDetectedCollider(_target.GameObject.name).transform.position,
                    Vector3.up);
        }

        protected virtual void FireProjectile(Vector3 position) {
            if (NetworkManager.Singleton.IsServer) 
                FireProjectileClientRpc(position);

            // Vector3 direction = (position - transform.position).normalized;

            Projectile bullet =
                Instantiate(_projectilePrefab, _barrel.transform.position, Quaternion.Euler(Vector3.zero))
                    .GetComponent<Projectile>();

            bullet.transform.LookAt(position);

            bullet.Init(_parent, OnHit);

            CurrentCooldown += _cooldown;
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

        private void OnSensorUpdate(SensorUpdateEvent<IAttackable> evnt)
        {
            if (evnt.Detected)
            {
                if (_target == null && evnt.Target.GetRelationship(_parent.Owner) == Relationship.Hostile)
                    _target = evnt.Target;
            }
            else if (ReferenceEquals(evnt.Target, _target))
            {
                _target = null;
            }
        }

        public bool IsInRange(IAttackable target) => _sensor.IsDetected(target);
    }
}