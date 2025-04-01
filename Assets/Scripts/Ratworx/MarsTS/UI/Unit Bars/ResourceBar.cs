using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class ResourceBar : UnitBar
    {
        private bool _hasStored;
        private bool _isHoveredOrSelected;

        [SerializeField] private ResourceStorage _storage;

        private void Start()
        {
            _hasStored = false;
            _isHoveredOrSelected = false;

            _storage.OnAttributeChange += OnStorageValueChanged;

            UpdateValue(_storage.Amount, _storage.Capacity);

            EventAgent bus = GetComponentInParent<EventAgent>();

            bus.AddListener<UnitHoverEvent>(OnUnitHover);
            bus.AddListener<UnitSelectEvent>(OnUnitSelect);
        }

        private void OnStorageValueChanged(int oldValue, int newValue)
        {
            UpdateValue(newValue, _storage.Capacity);
        }

        private void UpdateValue(int currentValue, int maxValue)
        {
            UpdateBarWithFillLevel((float)currentValue / maxValue);

            if (currentValue > 0)
            {
                if (!_isHoveredOrSelected)
                    _barRenderer.enabled = true;

                _hasStored = true;
            }
            else
            {
                if (!_isHoveredOrSelected)
                    _barRenderer.enabled = false;

                _hasStored = false;
            }
        }

        private void OnUnitHover(UnitHoverEvent _event)
        {
            _isHoveredOrSelected = _event.Status;

            if (_event.Status)
                _barRenderer.enabled = true;
            else if (!_hasStored)
                _barRenderer.enabled = false;
        }

        private void OnUnitSelect(UnitSelectEvent _event)
        {
            _isHoveredOrSelected = _event.Status;

            if (_event.Status)
                _barRenderer.enabled = true;
            else if (!_hasStored)
                _barRenderer.enabled = false;
        }
    }
}