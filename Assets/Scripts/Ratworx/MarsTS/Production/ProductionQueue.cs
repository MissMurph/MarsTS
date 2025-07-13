using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Production
{
    public class ProductionQueue : NetworkBehaviour,
                                   IEntityComponent<ProductionQueue>
    {
        public Action OnQueueChanged;
        public Action OnOrderEnqueued;
        public Action OnOrderComplete;
        public Action<ProductionOrder, int> OnProductionProgressIncreased;
        
        [SerializeField] private ProductionOrder _orderPrefab;
        
        public ProductionQueue Get() => this;
        public string Key => "productionQueue";

        public ProductionOrder CurrentOrder
            => _productionQueue.Count > 0
                ? _productionQueue[0]
                : null;
        public int QueueCount => _productionQueue.Count;
        public float CurrentProductionAmount => _receiver.CurrentProductionAmount;
        public List<ProductionOrder> Queue => _productionQueue;
        
        private readonly List<ProductionOrder> _productionQueue = new List<ProductionOrder>();

        private UnitOwnership _ownership;
        private EventAgent _eventAgent;
        private ProductionReceiver _receiver;

        private void Awake() {
            _ownership = GetComponent<UnitOwnership>();
            _eventAgent = GetComponent<EventAgent>();
            _receiver = GetComponentInChildren<ProductionReceiver>();
        }

        private void Start() {
            _eventAgent.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        public void EnqueueOrder(ProductionOrder order) {
            _productionQueue.Add(order);
            OnQueueChanged?.Invoke();

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
            ProductionOrder completedOrder = _productionQueue[queuePosition];
            
            if (isCancelled) {
                foreach (ResourceCost cost in completedOrder.Cost) {
                    _ownership.Owner.GetResource(cost.key).Deposit(cost.amount);
                }
            }
            
            OnOrderComplete?.Invoke();
            OnQueueChanged?.Invoke();
            
            _productionQueue.RemoveAt(queuePosition);
            CompleteOrderClientRpc(queuePosition, isCancelled);
            
            Destroy(completedOrder.gameObject, 0.1f);
        }

        [Rpc(SendTo.NotServer)]
        private void CompleteOrderClientRpc(int queuePosition, bool isCancelled) {
            var completedOrder = _productionQueue[queuePosition];
            
            OnOrderComplete?.Invoke();
            OnQueueChanged?.Invoke();
        }

        public void CancelOrder(int position) => CompleteOrder(position, true);

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent evnt) {
            evnt.Info.Module<ProductionInfo>("productionQueue").SetQueue(this);
        }
    }
}