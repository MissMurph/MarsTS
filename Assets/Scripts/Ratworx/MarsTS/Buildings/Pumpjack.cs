using System;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Building;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public class Pumpjack : Building, IHarvestable
    {
        /*	IHarvestable Properties	*/

        public int OriginalAmount => _resourceStorage.Capacity;
        public int StoredAmount => _resourceStorage.Amount;

        /*	Pumpjack Fields	*/

        private OilDeposit _exploitedDeposit;

        [SerializeField] private int _harvestRate;

        private int _harvestAmount;
        private float _cooldown;
        private float _currentCooldown;

        private GameObject _oilDetector;

        private ResourceStorage _resourceStorage;

        protected override void Awake()
        {
            base.Awake();

            _resourceStorage = GetComponent<ResourceStorage>();

            _cooldown = 1f / _harvestRate;
            _harvestAmount = (int)(_harvestRate * _cooldown);
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
                _exploitedDeposit.Harvest("oil", this, _harvestAmount, _resourceStorage.Submit);

                /*Bus.Global(new ResourceHarvestedEvent(
                    Bus,
                    _exploitedDeposit,
                    this,
                    ResourceHarvestedEvent.Side.Harvester,
                    harvested,
                    "oil",
                    StoredAmount,
                    capacity
                    )
                );*/

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

        public virtual int Harvest(string resourceKey, ISelectable harvester, int harvestAmount,
            Func<int, int> extractor)
        {
            if (CanHarvest(resourceKey, harvester))
            {
                int availableAmount = Mathf.Min(harvestAmount, StoredAmount);

                int finalAmount = extractor(availableAmount);

                if (finalAmount > 0)
                    /*Bus.Global(new ResourceHarvestedEvent(Bus, this, harvester, ResourceHarvestedEvent.Side.Deposit,
                        finalAmount, "oil", StoredAmount, capacity));*/
                    //StoredAmount -= finalAmount;
                    _resourceStorage.Consume(finalAmount);

                return finalAmount;
            }

            return 0;
        }

        protected override void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            base.OnUnitInfoDisplayed(_event);

            if (ReferenceEquals(_event.Unit, this))
            {
                UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("storage");
                info.SetStorage(_resourceStorage);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_exploitedDeposit != null) _exploitedDeposit.Exploited = false;
        }
    }
}