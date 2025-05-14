using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class ConstructionBar : UnitBar
    {
        private void Start()
        {
            IAttackable parent = GetComponentInParent<IAttackable>();

            if (parent.Health <= 1)
            {
                UpdateBarWithFillLevel(0f);

                _barRenderer.enabled = true;

                GetComponentInParent<EventAgent>().AddListener<UnitHurtEvent>(_event =>
                {
                    UpdateBarWithFillLevel((float)_event.Attackable.Health / _event.Attackable.MaxHealth);
                    if (_event.Attackable.Health >= _event.Attackable.MaxHealth) gameObject.SetActive(false);
                });
            }
            else
            {
                _barRenderer.enabled = false;
            }
        }
    }
}