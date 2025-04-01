using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class ArtilleryTurret : ProjectileTurret
    {
        private bool _isDeployed;
        private Quaternion _startingPos;
        private GameObject _rangeIndicator;

        protected override void Awake()
        {
            base.Awake();

            _rangeIndicator = transform.root.Find("RangeIndicator").gameObject;
        }

        private void Start()
        {
            _startingPos = _barrel.transform.localRotation;

            _bus.AddListener<DeployEvent>(OnDeploy);
            _bus.AddListener<UnitSelectEvent>(OnSelect);
            _bus.AddListener<UnitHoverEvent>(OnHover);
        }

        protected override void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (_isDeployed)
            {
                if (CurrentCooldown >= 0f)
                {
                    CurrentCooldown -= Time.deltaTime;
                    //_bus.Local(new WorkEvent(_bus, _parent, (int)_cooldown, _cooldown - CurrentCooldown));
                }

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
            else
            {
                _barrel.transform.localRotation = _startingPos;

                if (CurrentCooldown >= 0f) CurrentCooldown -= Time.deltaTime;
            }
        }

        private void OnDeploy(DeployEvent evnt)
        {
            _isDeployed = evnt.IsDeployed;
            _rangeIndicator.SetActive(evnt.IsDeployed);
        }

        private void OnSelect(UnitSelectEvent evnt)
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
        }
    }
}