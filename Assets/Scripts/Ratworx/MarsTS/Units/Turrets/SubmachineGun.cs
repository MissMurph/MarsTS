using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class SubmachineGun : ProjectileTurret
    {
        //Amount of bullets fired per burst
        [SerializeField] private int burstCount;
        private int firedCount;

        [SerializeField] private float burstCooldown;
        private float currentBurstCooldown;

        private bool isSneaking;

        private void Start()
        {
            _bus.AddListener<SneakEvent>(OnSneak);
            isSneaking = false;
        }

        protected override void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (CurrentCooldown >= 0f) CurrentCooldown -= Time.deltaTime;
            if (currentBurstCooldown >= 0f) currentBurstCooldown -= Time.deltaTime;

            if (_parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null &&
                commandableUnit.CurrentCommand.Name == "attack")
            {
                var attackCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

                if (Sensor.IsDetected(attackCommand.Target)) _target = attackCommand.Target;
            }

            if (_target == null)
            {
                float distance = Sensor.Range * Sensor.Range;
                IAttackable currentClosest = null;

                foreach (IAttackable unit in Sensor.Detected)
                {
                    if (unit.GetRelationship(_parent.Owner) == Relationship.Hostile)
                    {
                        float newDistance =
                            Vector3.Distance(Sensor.GetDetectedCollider(unit.GameObject.name).transform.position,
                                transform.position);

                        if (newDistance < distance) currentClosest = unit;
                    }
                }

                if (currentClosest != null) _target = currentClosest;
            }

            if (!isSneaking && _target != null && Sensor.IsDetected(_target) && currentBurstCooldown <= 0 &&
                CurrentCooldown <= 0)
            {
                FireProjectile(Sensor.GetDetectedCollider(_target.GameObject.name).transform.position);
                firedCount++;

                if (firedCount >= burstCount)
                {
                    currentBurstCooldown += burstCooldown;
                    firedCount = 0;
                }
            }
        }

        private void OnSneak(SneakEvent _event)
        {
            isSneaking = _event.IsSneaking;
        }
    }
}