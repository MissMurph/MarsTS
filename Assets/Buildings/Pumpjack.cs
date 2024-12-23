using System;
using MarsTS.Events;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.World;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class Pumpjack : Building, IHarvestable
    {
        /*	IHarvestable Properties	*/

        public int OriginalAmount => capacity;

        [SerializeField] 
        private int capacity;

        public int StoredAmount { get; private set; }

        /*	Pumpjack Fields	*/

        private OilDeposit _exploitedDeposit;

        [SerializeField] 
        private int harvestRate;

        private int _harvestAmount;
        private float _cooldown;
        private float _currentCooldown;

        private GameObject _oilDetector;

        protected override void Awake()
        {
            base.Awake();

            _cooldown = 1f / harvestRate;
            _harvestAmount = (int)(harvestRate * _cooldown);
            _currentCooldown = _cooldown;

            _oilDetector = transform.Find("OilCollider").gameObject;
            _oilDetector.SetActive(false);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                Bus.AddListener<PumpjackExploitInitEvent>(OnExploitInit);
                Bus.AddListener<EntityInitEvent>(OnEntityInit);
            }
        }

        protected void Update()
        {
            if (_exploitedDeposit == null) return;

            _currentCooldown -= Time.deltaTime;

            if (_currentCooldown <= 0)
            {
                int harvested = _exploitedDeposit.Harvest("oil", this, _harvestAmount, Pump);
                
                Bus.Global(new ResourceHarvestedEvent(
                    Bus, 
                    _exploitedDeposit, 
                    this, 
                    ResourceHarvestedEvent.Side.Harvester,
                    harvested, 
                    "oil", 
                    StoredAmount, 
                    capacity
                    )
                );

                _currentCooldown += _cooldown;
            }
        }

        private void OnEntityInit(EntityInitEvent _event)
        {
            _oilDetector.SetActive(true);
        }

        private void OnExploitInit(PumpjackExploitInitEvent _event)
        {
            _exploitedDeposit = _event.Oil;
            _exploitedDeposit.Exploited = true;
        }

        public virtual bool CanHarvest(string resourceKey, ISelectable unit) =>
            resourceKey == "oil" && (unit.Owner == Owner || unit.UnitType == "roughneck");

        private int Pump(int amount)
        {
            int newAmount = Mathf.Min(capacity, StoredAmount + amount);

            int difference = newAmount - StoredAmount;

            StoredAmount = newAmount;

            return difference;
        }

        public virtual int Harvest(string resourceKey, ISelectable harvester, int harvestAmount,
            Func<int, int> extractor)
        {
            if (CanHarvest(resourceKey, harvester))
            {
                int availableAmount = Mathf.Min(harvestAmount, StoredAmount);

                int finalAmount = extractor(availableAmount);

                if (finalAmount > 0)
                {
                    /*Bus.Global(new ResourceHarvestedEvent(Bus, this, harvester, ResourceHarvestedEvent.Side.Deposit,
                        finalAmount, "oil", StoredAmount, capacity));*/
                    //StoredAmount -= finalAmount;
                    _resourceStorage.Consume(finalAmount);
                }

                return finalAmount;
            }

            return 0;
        }

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            base.OnUnitInfoDisplayed(_event);

            if (ReferenceEquals(_event.Unit, this))
            {
                StorageInfo info = _event.Info.Module<StorageInfo>("storage");
                info.CurrentUnit = this;
                info.CurrentValue = StoredAmount;
                info.MaxValue = capacity;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_exploitedDeposit != null) _exploitedDeposit.Exploited = false;
        }
    }
}