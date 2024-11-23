using MarsTS.Buildings;
using MarsTS.Events;

namespace MarsTS.UI
{
    public class PumpjackResourceBar : UnitBar
    {
        private bool HasStored
        {
            get => _hasStored;
            set
            {
                barRenderer.enabled = value;
                _hasStored = value;
            }
        }

        private bool _hasStored;

        private bool DisplayBar
        {
            get => _displayBar;
            set
            {
                barRenderer.enabled = value;
                _displayBar = value;
            }
        }

        private bool _displayBar;

        private Pumpjack _parent;

        private void Start()
        {
            HasStored = false;
            barRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            _parent = GetComponentInParent<Pumpjack>();

            FillLevel = (float)_parent.StoredAmount / _parent.OriginalAmount;

            bus.AddListener<ResourceHarvestedEvent>(_event =>
            {
                FillLevel = (float)_event.Deposit.StoredAmount / _event.Deposit.OriginalAmount;
            });

            bus.AddListener<UnitHoverEvent>(_event =>
            {
                if (_event.Status) DisplayBar = true;
                else if (!HasStored) DisplayBar = false;
            });

            bus.AddListener<UnitSelectEvent>(_event =>
            {
                if (_event.Status) DisplayBar = true;
                else if (!HasStored) DisplayBar = false;
            });

            bus.AddListener<ResourceHarvestedEvent>(_event =>
            {
                FillLevel = (float)_event.StoredAmount / _event.Capacity;

                HasStored = FillLevel > 0f;
            });

            bus.AddListener<HarvesterDepositEvent>(_event =>
            {
                FillLevel = (float)_event.StoredAmount / _event.Capacity;

                HasStored = FillLevel > 0f;
            });
        }
    }
}