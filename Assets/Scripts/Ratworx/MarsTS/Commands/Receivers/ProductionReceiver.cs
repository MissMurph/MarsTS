using System;
using Ratworx.MarsTS.Commands.Commandlets;
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
                                      IEntityClientUpdate
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

        private void Awake() {
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
                if (option.ProductKey != command.ProductRegistryKey) continue;
                
                ProductionOrder order = Instantiate(_orderPrefab).GetComponent<ProductionOrder>();

                order.ProductionRequired = option.ProductionRequired;
                order.ProductKey = option.ProductKey;
                order.ProductionType = option.ProductionType;
                order.Cost = option.Cost;
                    
                _productionQueue.EnqueueOrder(order);
                
                command.CompleteCommand(CommandQueue);
                return;
            }
            
            RatLogger.Error?.Log($"Couldn't find matching production option for {command.ProductRegistryKey}! Cancelling order");
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
                case "produce":
                    SpawnProductProduce();
                    break;
                case "research":
                    SpawnProductResearch();
                    break;
                case "upgrade":
                    SpawnProductUpgrade();
                    break;
            }

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

        private void OnProductionOrderCompleteClient(ProductionOrder order, bool isCancelled) {
            _currentProductionAmount = 0;
        }
        
        // TODO: Turn below into an interface
        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) => (false, null);
    }
}