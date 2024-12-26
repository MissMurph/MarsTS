using System;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.World
{
    public class ResourceDeposit : MonoBehaviour, IHarvestable, ISelectable, ITaggable<ResourceDeposit>
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*	ISelectable Properties	*/

        public int Id => _entityComponent.Id;

        public string UnitType => _key;

        public string RegistryKey => UnitType + ":" + _key;

        [SerializeField] private string _key;

        public Faction Owner => TeamCache.Faction(0);

        public Sprite Icon => _icon;

        [SerializeField] private Sprite _icon;

        /*	ITaggable Properties	*/

        public string Key => "selectable";

        public Type Type => typeof(ResourceDeposit);

        /*	IHarvestable Properties	*/

        public int OriginalAmount { get; private set; }

        public int StoredAmount => _resourceStorage.Amount;

        /*	Deposit Fields	*/

        private Entity _entityComponent;

        protected EventAgent _bus;

        private GameObject _selectionCircle;

        //This is just for the registry key, some examples:
        //deposit:scrap
        //deposit:oil_slick
        //deposit:shale_oil
        //deposit:biomass
        //deposit:rock
        [SerializeField] private string _depositType;

        protected ResourceStorage _resourceStorage;

        protected virtual void Awake()
        {
            _entityComponent = GetComponent<Entity>();
            _resourceStorage = GetComponent<ResourceStorage>();
            _bus = GetComponent<EventAgent>();
            _selectionCircle = transform.Find("SelectionCircle").gameObject;
            _selectionCircle.SetActive(false);
        }

        private void Start()
        {
            //selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();
            OriginalAmount = _resourceStorage.Amount;
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        public bool CanHarvest(string resourceKey, ISelectable unit)
        {
            if (resourceKey == _depositType) return true;
            return false;
        }

        public ResourceDeposit Get() => this;

        public Relationship GetRelationship(Faction player) => Relationship.Neutral;

        public virtual int Harvest(
            string resourceKey,
            ISelectable harvester,
            int harvestAmount,
            Func<int, int> extractor
        ) {
            int availableAmount = Mathf.Min(harvestAmount, _resourceStorage.Amount);

            int finalAmount = extractor(availableAmount);

            if (finalAmount > 0)
            {
                _bus.Global(new ResourceHarvestedEvent(_bus, this, ResourceHarvestedEvent.Side.Deposit,
                    finalAmount, resourceKey, StoredAmount, OriginalAmount));
                _resourceStorage.Consume(finalAmount);
            }

            if (StoredAmount <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject, 0.01f);
            }

            return finalAmount;
        }

        public void Select(bool status)
        {
            _selectionCircle.SetActive(status);
            _bus.Local(new UnitSelectEvent(_bus, status));
        }

        public void Hover(bool status)
        {
            //These are seperated due to the Player Selection Check
            if (status)
            {
                _selectionCircle.SetActive(true);
                _bus.Local(new UnitHoverEvent(_bus, status));
            }
            else if (!Player.Main.HasSelected(this))
            {
                _selectionCircle.SetActive(false);
                _bus.Local(new UnitHoverEvent(_bus, status));
            }
        }

        public bool SetOwner(Faction player) => false;

        private void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("deposit");
                info.SetStorage(_resourceStorage);
            }
        }
    }
}