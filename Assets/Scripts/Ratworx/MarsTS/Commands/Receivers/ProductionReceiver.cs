using System;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Registry;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class ProductionReceiver : AbstractCommandReceiver<ProduceCommandlet>,
                                      IEntityServerUpdate,
                                      IEntityClientUpdate,
                                      ICostingCommand
    {
        [SerializeField] private int _productionPerSecond;
        [SerializeField] private ProductionOrder _orderPrefab;
        [SerializeField] private ProductionOption[] _productionOptions;
        [SerializeField] private EntitySpawner _spawnPoint;

        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;
        public float CurrentProductionAmount => _currentProductionAmount;

        private float _currentProductionAmount;
        private ProductionQueue _productionQueue;
        private Entity _entity;

        protected override void Awake() {
            base.Awake();
            
            _productionQueue = GetComponentInParent<ProductionQueue>();
            _entity = GetComponentInParent<Entity>();
        }

        private void Start() {
            _currentProductionAmount = 0f;
            _spawnPoint.SetOwner(Ownership.Owner);

            // Check if we're not server rather than if we are client to support host topology
            if (!NetworkManager.Singleton.IsServer) 
                _productionQueue.OnOrderComplete += OnProductionOrderCompleteClient;
        }

        public override void ReceiveCommand(ProduceCommandlet command) {
            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != command.Target.OptionKey) continue;
                
                ProductionOrder order = Instantiate(_orderPrefab).GetComponent<ProductionOrder>();

                order.ProductionRequired = option.ProductionRequired;
                order.ProductKey = option.ProductKey;
                order.ProductionType = option.ProductionType;
                order.Cost = option.Cost;
                    
                _productionQueue.EnqueueOrder(order);
                
                command.CompleteCommand(CommandQueue);
                return;
            }
            
            RatLogger.Error?.Log($"Couldn't find matching production option for {command.Target.ProductKey}! Cancelling order");
            command.CompleteCommand(CommandQueue, true);
        }

        public void UpdateServer() {
            if (_productionQueue.QueueCount <= 0
                || _productionQueue.CurrentOrder is null) 
                return;

            _currentProductionAmount += Time.deltaTime;
            var evnt = new ProductionStepEvent(_entity, _productionQueue.CurrentOrder,
                _currentProductionAmount / _productionQueue.CurrentOrder.ProductionRequired);
            EventAgent.PostGlobal(evnt);

            if (_currentProductionAmount >= _productionQueue.CurrentOrder.ProductionRequired) 
                CompleteProductionOrder();
        }

        public void UpdateClient() {
            // We check for if we're the server to support host topology
            if (NetworkManager.Singleton.IsServer
                || _productionQueue.QueueCount <= 0
                || _productionQueue.CurrentOrder is null) 
                return;

            if (_currentProductionAmount <= _productionQueue.CurrentOrder.ProductionRequired) ;
            
            _currentProductionAmount += Time.deltaTime;
            var evnt = new ProductionStepEvent(_entity, _productionQueue.CurrentOrder,
                _currentProductionAmount / _productionQueue.CurrentOrder.ProductionRequired);
            EventAgent.PostGlobal(evnt);
        }

        private void CompleteProductionOrder() {
            switch (_productionQueue.CurrentOrder.ProductionType)
            {
                case "production":
                    SpawnProductProduce();
                    break;
                case "research":
                    SpawnProductResearch();
                    break;
                case "upgrade":
                    SpawnProductUpgrade();
                    break;
            }

            _currentProductionAmount = 0f;
            _productionQueue.CompleteCurrentOrder();
        }

        // Use this for entities
        private void SpawnProductProduce() {
            if (!Registry.Registry.TryGetPrefab(_productionQueue.CurrentOrder.ProductKey, out GameObject prefab)) {
                RatLogger.Error?.Log($"Couldn't find registered prefab {_productionQueue.CurrentOrder.ProductKey}!");
                return;
            }
            
            _spawnPoint.SetEntity(prefab);
            _spawnPoint.SpawnEntity();
        }

        private void SpawnProductResearch() {
            if (!Registry.Registry.TryGetPrefab(_productionQueue.CurrentOrder.ProductKey, out GameObject prefab)) {
                RatLogger.Error?.Log($"Couldn't find registered prefab {_productionQueue.CurrentOrder.ProductKey}!");
                return;
            }

            NetworkObject networkObj = Instantiate(prefab).GetComponent<NetworkObject>();
            networkObj.Spawn();
            networkObj.TrySetParent(Ownership.Owner.transform);
        }

        private void SpawnProductUpgrade() {
            if (!Registry.Registry.TryGetPrefab(_productionQueue.CurrentOrder.ProductKey, out GameObject prefab)) {
                RatLogger.Error?.Log($"Couldn't find registered prefab {_productionQueue.CurrentOrder.ProductKey}!");
                return;
            }

            NetworkObject networkObj = Instantiate(prefab).GetComponent<NetworkObject>();
            networkObj.Spawn();
            networkObj.TrySetParent(_entity.transform);
        }

        private void OnProductionOrderCompleteClient() {
            _currentProductionAmount = 0;
        }
        
        // TODO: Turn below into an interface
        public override (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);

        public override void StartSelection(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error starting {CommandKey} selection, argument is empty!");
                return;
            }

            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != argument) continue;
                
                CommandPrimer.GetInterface<ProductionCommandInterface>(CommandKey).StartArgSelection(option);
                return;
            }
            
            RatLogger.Error?.Log($"Production option with key {argument} not found!");
        }

        public override Sprite GetIcon(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} icon, argument is empty!");
                return null;
            }

            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != argument) continue;
                
                return CommandPrimer.GetInterface<ProductionCommandInterface>(CommandKey).GetArgIcon(option);
            }
            
            RatLogger.Error?.Log($"Production option with key {argument} not found!");
            return null;
        }

        public override string GetDescription(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} description, argument is empty!");
                return string.Empty;
            }

            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != argument) continue;
                
                // TODO: bruh we don't need to go the interface
                return CommandPrimer.GetInterface<ProductionCommandInterface>(CommandKey).GetArgDescription(option);
            }
            
            RatLogger.Error?.Log($"Production option with key {argument} not found!");
            return string.Empty;
        }

        public ResourceCost[] GetCost(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} cost, argument is empty!");
                return Array.Empty<ResourceCost>();
            }

            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != argument) continue;
                
                return option.Cost;
            }
            
            RatLogger.Error?.Log($"Production option with key {argument} not found!");
            return Array.Empty<ResourceCost>();
        }

        public override string GetName(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} description, argument is empty!");
                return string.Empty;
            }

            foreach (ProductionOption option in _productionOptions) {
                if (option.OptionKey != argument) continue;
                
                return option.OptionKey;
            }
            
            RatLogger.Error?.Log($"Production option with key {argument} not found!");
            return string.Empty;
        }
    }
}