using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Production
{
    public class ProductionQueue : NetworkBehaviour,
                                   IEntityComponent<ProductionQueue>
    {
        public Action<ProductionOrder[]> OnQueueChanged;
        public Action<ProductionOrder> OnOrderEnqueued;
        public Action<ProductionOrder, bool> OnOrderComplete;
        
        [SerializeField] private ProductionOrder _orderPrefab;
        
        public ProductionQueue Get() => this;
        public string Key => "productionQueue";

        public ProductionOrder CurrentOrder => _productionQueue[0];
        public int Count => _productionQueue.Count;
        
        private readonly List<ProductionOrder> _productionQueue = new List<ProductionOrder>();

        private UnitOwnership _ownership;

        private void Awake() {
            _ownership = GetComponent<UnitOwnership>();
        }

        public void EnqueueOrder(ProductionOrder order) {
            _productionQueue.Add(order);
            OnQueueChanged?.Invoke(_productionQueue.ToArray());

            if (NetworkManager.Singleton.IsServer) 
                EnqueueOrderClientRpc(order.ProductKey, order.ProductionRequired);
        }

        [Rpc(SendTo.NotServer)]
        private void EnqueueOrderClientRpc(string productKey, int productionRequired) {
            ProductionOrder order = Instantiate(_orderPrefab).GetComponent<ProductionOrder>();

            order.ProductKey = productKey;
            order.ProductionRequired = productionRequired;
            
            EnqueueOrder(order);
        }

        public void CompleteCurrentOrder(bool isCancelled = false) => CompleteOrder(0, isCancelled);

        private void CompleteOrder(int queuePosition, bool isCancelled) {
            var completedOrder = _productionQueue[queuePosition];
            
            if (isCancelled) {
                foreach (ResourceCost cost in completedOrder.Cost) {
                    _ownership.Owner.GetResource(cost.key).Deposit(cost.amount);
                }
            }
            
            OnOrderComplete?.Invoke(completedOrder, isCancelled);
            OnQueueChanged?.Invoke(_productionQueue.ToArray());
            
            CompleteOrderClientRpc(queuePosition, isCancelled);
            
            Destroy(completedOrder, 0.1f);
        }

        [Rpc(SendTo.NotServer)]
        private void CompleteOrderClientRpc(int queuePosition, bool isCancelled) {
            var completedOrder = _productionQueue[queuePosition];
            
            OnOrderComplete?.Invoke(completedOrder, isCancelled);
            OnQueueChanged?.Invoke(_productionQueue.ToArray());
        }

        public void CancelOrder(int position) => CompleteOrder(position, true);

        /*protected virtual void OnUnitInfoDisplayed(UnitInfoEvent @event)
        {
            if (!ReferenceEquals(@event.Unit, this)) return;

            HealthInfo info = @event.Info.Module<HealthInfo>("health");
            info.CurrentUnit = this;

            @event.Info.Module<ProductionInfo>("productionQueue").SetQueue(this, production.Current as IProducable,
                production.QueuedProduction);
        }*/
    }
}