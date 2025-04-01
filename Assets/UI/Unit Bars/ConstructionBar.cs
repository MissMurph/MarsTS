using MarsTS.Events;
using MarsTS.Units;

namespace MarsTS.UI
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
                    UpdateBarWithFillLevel((float)_event.Targetable.Health / _event.Targetable.MaxHealth);
                    if (_event.Targetable.Health >= _event.Targetable.MaxHealth) gameObject.SetActive(false);
                });
            }
            else
            {
                _barRenderer.enabled = false;
            }
        }
    }
}