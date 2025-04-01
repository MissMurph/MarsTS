using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class ProjectileTurret : NetworkBehaviour
    {
        [FormerlySerializedAs("projectile")] [SerializeField]
        protected GameObject _projectile;

        [FormerlySerializedAs("damage")] [SerializeField]
        protected int _damage;

        [FormerlySerializedAs("cooldown")] [SerializeField]
        protected float _cooldown;

        protected float CurrentCooldown;

        [FormerlySerializedAs("barrel")] [SerializeField]
        protected GameObject _barrel;

        public float Range => _sensor.Range;

        protected IAttackable _target;

        protected ISelectable _parent;
        protected EventAgent _bus;

        protected AttackableSensor _sensor;

        protected virtual void Awake()
        {
            _parent = GetComponentInParent<ISelectable>();
            _bus = GetComponentInParent<EventAgent>();
            _sensor = GetComponent<AttackableSensor>();

            _bus.AddListener<SensorUpdateEvent<IAttackable>>(OnSensorUpdate);
        }

        protected virtual void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (CurrentCooldown >= 0f) CurrentCooldown -= Time.deltaTime;

            if (_parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null &&
                commandableUnit.CurrentCommand.Name == "attack")
            {
                var attackCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

                if (_sensor.IsDetected(attackCommand.Target)) _target = attackCommand.Target;
            }

            if (_target == null)
            {
                float distance = _sensor.Range * _sensor.Range;
                IAttackable currentClosest = null;

                foreach (IAttackable unit in _sensor.Detected)
                {
                    if (unit.GetRelationship(_parent.Owner) == Relationship.Hostile)
                    {
                        float newDistance =
                            Vector3.Distance(_sensor.GetDetectedCollider(unit.GameObject.name).transform.position,
                                transform.position);

                        if (newDistance < distance) currentClosest = unit;
                    }
                }

                if (currentClosest != null) _target = currentClosest;
            }

            if (_target != null && _sensor.IsDetected(_target) && CurrentCooldown <= 0)
                FireProjectile(_sensor.GetDetectedCollider(_target.GameObject.name).transform.position);
        }

        private void FixedUpdate() {
            if (!NetworkManager.Singleton.IsServer) return;
            
            if (_target != null && _sensor.IsDetected(_target))
                _barrel.transform.LookAt(_sensor.GetDetectedCollider(_target.GameObject.name).transform.position,
                    Vector3.up);
        }

        protected virtual void FireProjectile(Vector3 _position)
        {
            if (NetworkManager.Singleton.IsServer) FireProjectileClientRpc(_position);

            Vector3 direction = (_position - transform.position).normalized;

            Projectile bullet = Instantiate(_projectile, _barrel.transform.position, Quaternion.Euler(Vector3.zero))
                .GetComponent<Projectile>();

            bullet.transform.LookAt(_position);

            bullet.Init(_parent, OnHit);

            CurrentCooldown += _cooldown;
        }

        [Rpc(SendTo.NotServer)]
        protected virtual void FireProjectileClientRpc(Vector3 position)
        {
            FireProjectile(position);
        }

        protected virtual void OnHit(bool success, IAttackable unit)
        {
            if (!NetworkManager.Singleton.IsServer || !success) return;
            
            UnitAttackEvent attackEvent = new UnitAttackEvent(_bus, unit as ISelectable, _parent, _damage);
				
            attackEvent.Phase = Phase.Pre;
            _bus.Global(attackEvent);

            // Captures modified damage
            int damage = attackEvent.Damage;
            unit.Attack(damage);
			
            attackEvent.Phase = Phase.Post;
            _bus.Global(attackEvent);
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