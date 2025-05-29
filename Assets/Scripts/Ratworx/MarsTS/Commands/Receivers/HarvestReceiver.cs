using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class HarvestReceiver : AbstractCommandReceiver<HarvestableCommandlet>
    {
        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;
        
        [SerializeField] private HarvestSensor _harvestRange;
        [SerializeField] private DepositSensor _depositRange;
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private bool _automaticallyReturn;

        private HarvestableCommandlet _harvestCommand;
        private IDepositable _depositable;

        public override void ReceiveCommand(HarvestableCommandlet command) {
            _harvestCommand = command;
            UnitTargeting.SetTarget(_harvestCommand.Target);
            
            _storage.OnAttributeChange += OnResourceHarvested;
            _storage.OnAttributeChange += OnResourceDeposited;
            _harvestRange.OnUnitDetected += OnHarvestableDetected;
            _depositRange.OnUnitDetected += OnDepositableDetected;
			
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);
            _harvestCommand.OnCommandComplete.AddListener(OnCommandComplete);
        }

        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) {
            if (!entity.TryGetEntityComponent(out IHarvestable harvestable)
                || _storage.Value >= _storage.Capacity
                || harvestable.Resource != _storage.Resource)
                return (false, null);

            return (true, CommandPrimer.Get(CommandKey));
        }

        private void OnHarvestableDetected(IHarvestable unit, bool detected) {
            if (!detected
                || unit.Entity != _harvestCommand.Target.Entity) 
                return;
            
            UnitPathing.ClearPath();
            UnitTargeting.ClearTarget();
        }

        private void OnDepositableDetected(IDepositable unit, bool detected) {
            if (_depositable is null
                || !detected
                || unit.Entity != _depositable.Entity) 
                return;
            
            UnitPathing.ClearPath();
            UnitTargeting.ClearTarget();
        }

        private void OnResourceHarvested(int oldValue, int newValue) {
            if (newValue < oldValue
                || newValue < _storage.Capacity)
                return;
            
            FindDepositable();
        }

        private void OnResourceDeposited(int oldValue, int newValue) {
            if (newValue > oldValue
                || newValue > 0)
                return;
            
            UnitTargeting.SetTarget(_harvestCommand.Target);
            _depositable = null;
        }
        
        private void FindDepositable() {
            IDepositable closestBank = null;
            const float currentDist = 1000f;

            foreach (IDepositable bank in Ownership.Owner.GetOwnedDepositables()) {
                float newDistance = Vector3.Distance(bank.GameObject.transform.position, transform.position);
                if (newDistance < currentDist) closestBank = bank;
            }

            if (closestBank != null) 
                _depositable = closestBank;
            else
                _harvestCommand?.CompleteCommand(CommandQueue, true);
        }

        private void OnDepositDepleted(UnitDeathEvent evnt) => _harvestCommand.CompleteCommand(CommandQueue);

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _storage.OnAttributeChange -= OnResourceHarvested;
            _storage.OnAttributeChange -= OnResourceDeposited;
            _harvestRange.OnUnitDetected -= OnHarvestableDetected;
            _depositRange.OnUnitDetected -= OnDepositableDetected;
            
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);
            _harvestCommand.OnCommandComplete.RemoveListener(OnCommandComplete);
            _harvestCommand = null;
        }
    }
}