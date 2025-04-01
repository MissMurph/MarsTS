using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class HealthBar : UnitBar
    {
        private bool _hurt;

        private void Start()
        {
            _hurt = false;

            _barRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            IAttackable parent = GetComponentInParent<IAttackable>();

            UpdateBarWithFillLevel((float)parent.Health / parent.MaxHealth);

            bus.AddListener<UnitHurtEvent>(_event =>
            {
                UpdateBarWithFillLevel((float)_event.Targetable.Health / _event.Targetable.MaxHealth);

                if (_event.Targetable.Health < _event.Targetable.MaxHealth)
                {
                    _hurt = true;
                    _barRenderer.enabled = true;
                }
                else
                {
                    _hurt = false;
                    _barRenderer.enabled = false;
                }
            });

            bus.AddListener<UnitHoverEvent>(_event =>
            {
                if (_event.Status) _barRenderer.enabled = true;
                else if (!_hurt) _barRenderer.enabled = false;
            });

            bus.AddListener<UnitSelectEvent>(_event =>
            {
                if (_event.Status) _barRenderer.enabled = true;
                else if (!_hurt) _barRenderer.enabled = false;
            });
        }
    }
}