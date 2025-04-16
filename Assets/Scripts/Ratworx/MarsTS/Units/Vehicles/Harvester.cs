using System.Collections.Generic;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.Units.Turrets;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Vehicles
{
    public class Harvester : AbstractUnit
    {

        [Header("Turrets")]
        protected Dictionary<string, HarvesterTurret> registeredTurrets = new Dictionary<string, HarvesterTurret>();

        public IHarvestable HarvestTarget
        {
            get => harvestTarget;
            set
            {
                if (harvestTarget != null)
                {
                    EntityCache.TryGetEntityComponent(harvestTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_event => HarvestTarget = null);
                }

                harvestTarget = value;

                if (value != null)
                {
                    EntityCache.TryGetEntityComponent(value.GameObject.name + ":eventAgent", out EventAgent agent);

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
                    EntityCache.TryGetEntityComponent(depositTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_event => DepositTarget = null);
                }

                depositTarget = value;

                if (value != null)
                {
                    EntityCache.TryGetEntityComponent(value.GameObject.name + ":eventAgent", out EventAgent agent);
                    agent.AddListener<UnitDeathEvent>(_event => DepositTarget = null);
                }
            }
        }

        protected IDepositable depositTarget;

        protected int Stored => _storageComp.Value;

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
            _storageComp.Value -= DepositTarget.Deposit("resource_unit", _depositAmount);
            Bus.PostGlobal(new HarvesterDepositEvent(Bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity,
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
                    .Construct(harvestable, owner, Player.Player.ListSelected, Player.Player.Include);
                
                return;
            }

            if (target is IDepositable deserialized
                && Stored > 0)
            {
                CommandPrimer.Get<Deposit>("deposit")
                    .Construct(deserialized);
                
                return;
            }

            CommandPrimer.Get<Move>("move")
                .Construct(target.GameObject.transform.position);
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