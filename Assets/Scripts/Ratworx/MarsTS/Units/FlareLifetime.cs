using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class FlareLifetime : MonoBehaviour,
                                 IEntityServerUpdate
    {
        // In seconds
        [SerializeField] private int _lifeTime;

        private float _remainingLifeTime;
        private HealthAttribute _flareHealthAttribute;

        private void Awake() {
            _flareHealthAttribute = GetComponent<HealthAttribute>();
            
            _remainingLifeTime = _lifeTime;
        }

        public void UpdateServer() {
            if (_remainingLifeTime <= 0 
                || _flareHealthAttribute.Health <= 0) 
                return;
            
            _remainingLifeTime -= Time.deltaTime;
            _flareHealthAttribute.Value 
                = Mathf.RoundToInt(_flareHealthAttribute.MaxHealth * (_remainingLifeTime / _lifeTime));
        }
    }
}