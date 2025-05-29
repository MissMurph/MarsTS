using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class ConstructionBar : UnitBar
    {
        private ConstructionProgressAttribute _constructionAttribute;
        private HealthAttribute _healthAttribute;
        
        private void Awake() {
            _constructionAttribute = GetComponentInParent<ConstructionProgressAttribute>();
            _healthAttribute = GetComponentInParent<HealthAttribute>();
        }
        
        private void Start() {
            _constructionAttribute.OnAttributeChange += OnConstructionProgressChanged;
            _barRenderer.enabled = true;
        }

        private void OnConstructionProgressChanged(int oldValue, int newValue) 
            => UpdateBarWithFillLevel((float)_constructionAttribute.Value / _healthAttribute.MaxHealth);
    }
}