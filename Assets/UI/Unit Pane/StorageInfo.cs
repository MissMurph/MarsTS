using MarsTS.Events;
using MarsTS.Units;
using TMPro;
using UnityEngine;

namespace MarsTS.UI
{
    public class StorageInfo : MonoBehaviour, IInfoModule
    {
        public int CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;

                FillLevel = (float)_currentValue / MaxValue;

                _text.text = _currentValue + " / " + MaxValue;
            }
        }

        private int _currentValue = 1;

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;

                FillLevel = (float)_currentValue / MaxValue;

                _text.text = $"{CurrentValue} / {_maxValue}";
            }
        }

        private int _maxValue = 1;

        private float FillLevel
        {
            set
            {
                float rightEdge = _literalSize - _literalSize * value;
                _barTransform.offsetMax = new Vector2(-rightEdge, 0f);
            }
        }

        public ISelectable CurrentUnit { get; set; }

        public GameObject GameObject => gameObject;

        public string Name => "storage";

        private TextMeshProUGUI _text;
        private RectTransform _barTransform;

        private float _literalSize;

        private void Awake()
        {
            _text = transform.Find("Number").GetComponent<TextMeshProUGUI>();
            _barTransform = transform.Find("Bar") as RectTransform;

            //xMax is the max literal x co-ords from the center, so if we multiply by 2 that gets us the literal size
            _literalSize = _barTransform.rect.xMax * 2;
        }

        private void Start()
        {
            EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);
            EventBus.AddListener<ResourceHarvestedEvent>(OnResourceHarvested);
            EventBus.AddListener<HarvesterDepositEvent>(OnResourceDeposited);
        }

        private void OnResourceHarvested(ResourceHarvestedEvent _event)
        {
            if (ReferenceEquals(_event.Unit, CurrentUnit) && _event.EventSide == ResourceHarvestedEvent.Side.Deposit)
            {
                CurrentValue = _event.StoredAmount;
                MaxValue = _event.Capacity;
            }
            else if (ReferenceEquals(_event.Harvester, CurrentUnit) &&
                     _event.EventSide == ResourceHarvestedEvent.Side.Harvester)
            {
                CurrentValue = _event.StoredAmount;
                MaxValue = _event.Capacity;
            }
        }

        private void OnResourceDeposited(HarvesterDepositEvent _event)
        {
            if (ReferenceEquals(_event.Bank, CurrentUnit) && _event.EventSide == HarvesterDepositEvent.Side.Bank)
            {
                CurrentValue = _event.StoredAmount;
                MaxValue = _event.Capacity;
            }
            else if (ReferenceEquals(_event.Harvester, CurrentUnit) && _event.EventSide == HarvesterDepositEvent.Side.Harvester)
            {
                CurrentValue = _event.StoredAmount;
                MaxValue = _event.Capacity;
            }
        }

        private void OnEntityDeath(UnitDeathEvent _event)
        {
            if (ReferenceEquals(_event.Unit, CurrentUnit)) CurrentUnit = null;
        }

        public T Get<T>()
        {
            if (this is T output) return output;
            return default;
        }
    }
}