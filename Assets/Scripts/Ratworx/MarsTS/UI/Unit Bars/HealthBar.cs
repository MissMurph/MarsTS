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

            BarRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            IAttackable parent = GetComponentInParent<IAttackable>();

            UpdateBarWithFillLevel((float)parent.Health / parent.MaxHealth);

            bus.AddListener<UnitHurtEvent>(_event =>
            {
                UpdateBarWithFillLevel((float)_event.Attackable.Health / _event.Attackable.MaxHealth);

                if (_event.Attackable.Health < _event.Attackable.MaxHealth)
                {
                    _hurt = true;
                    BarRenderer.enabled = true;
                }
                else
                {
                    _hurt = false;
                    BarRenderer.enabled = false;
                }
            });

            bus.AddListener<UnitHoverEvent>(_event =>
            {
                if (_event.Status) BarRenderer.enabled = true;
                else if (!_hurt) BarRenderer.enabled = false;
            });

            bus.AddListener<UnitSelectEvent>(_event =>
            {
                if (_event.Status) BarRenderer.enabled = true;
                else if (!_hurt) BarRenderer.enabled = false;
            });
        }
    }
}