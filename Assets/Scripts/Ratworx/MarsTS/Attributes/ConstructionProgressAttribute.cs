using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities
{
    [RequireComponent(typeof(HealthAttribute))]
    public class ConstructionProgressAttribute : EntityAttribute
    {
        [SerializeField] private HealthAttribute _healthAttribute;

        private void Start() {
            if (NetworkManager.Singleton.IsServer) 
                _healthAttribute.OnAttributeChange += OnHealthChange;
        }

        private void OnHealthChange(int oldValue, int newValue) {
            if (newValue > oldValue) 
                Value += newValue - oldValue;
        }
    }
}