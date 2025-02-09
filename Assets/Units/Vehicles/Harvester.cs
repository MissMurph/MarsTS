using System;
using System.Collections.Generic;
using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.World;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Units
{
    public class Harvester : AbstractUnit
    {
        [Header("Movement")]
        [SerializeField] 
        private float _topSpeed;

        [SerializeField] 
        private float _acceleration;

        [SerializeField] 
        private float _turnSpeed;

        private float CurrentAngle => transform.rotation.eulerAngles.y;

        [SerializeField]
        private float _angleTolerance;

        private float velocity;

        [Header("Turrets")]
        protected Dictionary<string, HarvesterTurret> registeredTurrets = new Dictionary<string, HarvesterTurret>();

        public IHarvestable HarvestTarget
        {
            get => harvestTarget;
            set
            {
                if (harvestTarget != null)
                {
                    EntityCache.TryGet(harvestTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_event => HarvestTarget = null);
                }

                harvestTarget = value;

                if (value != null)
                {
                    EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

                    agent.AddListener<UnitDeathEvent>(_event => HarvestTarget = null);
                }
            }
        }

        protected IHarvestable harvestTarget;

        protected IDepositable DepositTarget
        {
            get => depositTarget;
            set
            {
                if (depositTarget != null)
                {
                    EntityCache.TryGet(depositTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_event => DepositTarget = null);
                }

                depositTarget = value;

                if (value != null)
                {
                    EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);
                    agent.AddListener<UnitDeathEvent>(_event => DepositTarget = null);
                }
            }
        }

        protected IDepositable depositTarget;

        protected int Stored => _storageComp.Amount;

        protected int Capacity => _storageComp.Capacity;

        protected ResourceStorage _storageComp;

        protected DepositSensor _depositableDetector;

        //This is how many units per second
        [SerializeField] protected float _depositRate;

        protected int _depositAmount;
        protected float _cooldown;
        protected float _currentCooldown;

        private GroundDetection _ground;

        protected override void Awake()
        {
            base.Awake();

            _storageComp = GetComponent<ResourceStorage>();
            _depositableDetector = GetComponentInChildren<DepositSensor>();
            _ground = GetComponent<GroundDetection>();

            _cooldown = 1f / _depositRate;
            _depositAmount = Mathf.RoundToInt(_depositRate * _cooldown);
            _currentCooldown = _cooldown;

            foreach (HarvesterTurret turret in GetComponentsInChildren<HarvesterTurret>())
            {
                registeredTurrets.TryAdd(turret.name, turret);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!NetworkManager.Singleton.IsServer) return;

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
                if (registeredTurrets["turret_main"].IsInRange(HarvestTarget))
                {
                    TrackedTarget = null;
                    CurrentPath = Path.Empty;
                }
                else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform))
                {
                    SetTarget(HarvestTarget.GameObject.transform);
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            velocity = Body.velocity.sqrMagnitude;

            if (!_ground.Grounded) return;
            
            if (!CurrentPath.IsEmpty)
            {
                Vector3 targetWaypoint = CurrentPath[PathIndex];

                Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0,
                    targetWaypoint.z - transform.position.z).normalized;
                float targetAngle = Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90f;

                float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, _turnSpeed * Time.fixedDeltaTime);
                Body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

                Vector3 currentVelocity = Body.velocity;
                Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, _ground.Slope.normal);

                adjustedVelocity *= currentVelocity.magnitude;

                if (Vector3.Angle(targetDirection, transform.forward) <= _angleTolerance)
                {
                    float accelCap = 1f - velocity / (_topSpeed * _topSpeed);

                    //This moves the velocity according to the rotation of the unit
                    Body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity,
                        _turnSpeed * accelCap * Time.fixedDeltaTime);

                    //Relative so it can take into account the forward vector of the car
                    Body.AddRelativeForce(Vector3.forward * (_acceleration * accelCap) * Time.fixedDeltaTime,
                        ForceMode.Acceleration);
                }

                if (velocity > _topSpeed * _topSpeed)
                {
                    Vector3 direction = Body.velocity.normalized;
                    direction *= _topSpeed;
                    Body.velocity = direction;
                }
            }
            else if (velocity >= 0.5f)
            {
                Body.AddRelativeForce(-Body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }

        public override void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "harvest":
                    break;
                case "deposit":
                    break;
                default:
                    base.Order(order, inclusive);
                    return;
            }

            if (inclusive) commands.Enqueue(order);
            else commands.Execute(order);
        }

        protected override void ExecuteOrder(CommandStartEvent _event)
        {
            switch (_event.Command.Name)
            {
                case "harvest":
                    Harvest(_event.Command);
                    break;
                case "deposit":
                    Deposit(_event.Command);
                    break;
                default:
                    base.ExecuteOrder(_event);
                    break;
            }
        }

        private void Harvest(Commandlet order)
        {
            if (Stored >= Capacity) FindDepositable();

            if (order is Commandlet<IHarvestable> deserialized)
            {
                HarvestTarget = deserialized.Target;

                Bus.AddListener<ResourceHarvestedEvent>(OnExtraction);

                EntityCache.TryGet(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);

                order.Callback.AddListener(HarvestCancelled);
            }
        }

        private void Deposit(Commandlet order)
        {
            if (order is Commandlet<IDepositable> deserialized)
            {
                DepositTarget = deserialized.Target;
                TrackedTarget = deserialized.Target.GameObject.transform;

                Bus.AddListener<HarvesterDepositEvent>(OnDeposit);

                order.Callback.AddListener(DepositCancelled);
            }
        }

        protected virtual void DepositResources()
        {
            _storageComp.Consume(DepositTarget.Deposit("resource_unit", _depositAmount));
            Bus.Global(new HarvesterDepositEvent(Bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
                DepositTarget));
            _currentCooldown += _cooldown;
        }

        private void OnDeposit(HarvesterDepositEvent _event)
        {
            if (Stored <= 0)
            {
                Bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

                //CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

                //CurrentCommand.Callback.Invoke(newEvent);

                DepositTarget = null;
                TrackedTarget = null;

                //if (HarvestTarget != null) Order(CommandRegistry.Get<Harvest>("harvest").Construct(HarvestTarget));
            }
        }

        private void FindDepositable()
        {
            IDepositable closestBank = null;
            float currentDist = 1000f;

            foreach (IDepositable bank in Owner.GetOwnedDepositables())
            {
                float newDistance = Vector3.Distance(bank.GameObject.transform.position, transform.position);

                if (newDistance < currentDist) closestBank = bank;
            }

            if (closestBank != null)
            {
                DepositTarget = closestBank;
                TrackedTarget = DepositTarget.GameObject.transform;

                Bus.AddListener<HarvesterDepositEvent>(OnDeposit);
            }
        }

        //Could potentially move these to the actual Command Classes
        private void OnExtraction(ResourceHarvestedEvent _event)
        {
            if (Stored >= Capacity)
                //bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);
                //EntityCache.TryGet(_event.Deposit.GameObject.transform.root.name, out EventAgent targetBus);
                //targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);
                //CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);
                //CurrentCommand.Callback.Invoke(newEvent);
                FindDepositable();
        }

        private void OnDepositDepleted(UnitDeathEvent _event)
        {
            Bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

            CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

            CurrentCommand.Callback.Invoke(newEvent);
        }

        private void HarvestCancelled(CommandCompleteEvent _event)
        {
            if (_event.Command is Commandlet<IHarvestable> deserialized && _event.IsCancelled)
            {
                Bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

                EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);

                HarvestTarget = null;
                DepositTarget = null;
            }
        }

        private void DepositCancelled(CommandCompleteEvent _event)
        {
            if (_event.Command is Commandlet<IDepositable> deserialized && _event.IsCancelled)
            {
                Bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

                DepositTarget = null;
                HarvestTarget = null;
            }
        }

        public override CommandFactory Evaluate(ISelectable target)
        {
            if (target is IHarvestable harvestable
                && Stored < Capacity
                && harvestable.StoredAmount > 0
                && harvestable.CanHarvest(_storageComp.Resource, this))
                return CommandPrimer.Get("harvest");

            if (target is IDepositable
                && Stored > 0)
                return CommandPrimer.Get("deposit");

            return CommandPrimer.Get("move");
        }

        public override void AutoCommand(ISelectable target)
        {
            if (target is IHarvestable harvestable
                && Stored < Capacity
                && harvestable.StoredAmount > 0
                && harvestable.CanHarvest(_storageComp.Resource, this))
            {
                CommandPrimer.Get<Harvest>("harvest")
                    .Construct(harvestable, owner, Player.ListSelected, Player.Include);
                
                return;
            }

            if (target is IDepositable deserialized
                && Stored > 0)
            {
                CommandPrimer.Get<Deposit>("deposit")
                    .Construct(deserialized, owner, Player.ListSelected, Player.Include);
                
                return;
            }

            CommandPrimer.Get<Move>("move")
                .Construct(target.GameObject.transform.position, owner, Player.ListSelected, Player.Include);
        }

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            base.OnUnitInfoDisplayed(_event);

            if (ReferenceEquals(_event.Unit, this))
            {
                UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("storage");
                info.SetStorage(_storageComp);
            }
        }

        public override bool CanCommand(string key)
        {
            if (key == "deposit") return true;

            return base.CanCommand(key);
        }
    }
}