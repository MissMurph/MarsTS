using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using TMPro;
using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Pane
{
    public class HealthInfo : MonoBehaviour, IInfoModule
    {
        public int CurrentHealth {
            get => _currentHealth;
            set
            {
                _currentHealth = value;

                FillLevel = (float)_currentHealth / MaxHealth;

                _text.text = _currentHealth + " / " + MaxHealth;
            }
        }

        private int _currentHealth = 1;

        public int MaxHealth {
            get => _maxHealth;
            set
            {
                _maxHealth = value;

                FillLevel = (float)_currentHealth / MaxHealth;

                _text.text = CurrentHealth + " / " + _maxHealth;
            }
        }

        private int _maxHealth = 1;

        private float FillLevel {
            set
            {
                float rightEdge = _literalSize - _literalSize * value;
                _barTransform.offsetMax = new Vector2(-rightEdge, 0f);
            }
        }

        public IAttackable CurrentUnit {
            get => _currentUnit;
            set
            {
                if (_currentUnit is not null) {
                    var eventAgent = _currentUnit.Entity.GetEntityComponent<EventAgent>();
                    
                    eventAgent.RemoveListener<UnitHurtEvent>(OnEntityHurt);
                    eventAgent.RemoveListener<UnitDeathEvent>(OnEntityDeath);
                }
                
                _currentUnit = value;

                if (_currentUnit is not null) {
                    var eventAgent = _currentUnit.Entity.GetEntityComponent<EventAgent>();
                    
                    eventAgent.AddListener<UnitHurtEvent>(OnEntityHurt);
                    eventAgent.AddListener<UnitDeathEvent>(OnEntityDeath);

                    CurrentHealth = value.Health;
                    MaxHealth = value.MaxHealth;
                }
            }
        }

        private IAttackable _currentUnit;

        public GameObject GameObject => gameObject;

        public string Name => "health";

        private TextMeshProUGUI _text;
        private RectTransform _barTransform;

        private float _literalSize;

        private void Awake() {
            _text = transform.Find("HealthNumber").GetComponent<TextMeshProUGUI>();
            _barTransform = transform.Find("HealthBar") as RectTransform;

            //xMax is the max literal x co-ords from the center, so if we multiply by 2 that gets us the literal size
            _literalSize = _barTransform.rect.xMax * 2;
        }

        private void OnEntityHurt(UnitHurtEvent _event) {
            if (ReferenceEquals(_event.Attackable, CurrentUnit)) {
                CurrentHealth = _event.Attackable.Health;
                MaxHealth = _event.Attackable.MaxHealth;
            }
        }

        private void OnEntityDeath(UnitDeathEvent _event) {
            CurrentUnit = null;
            Deactivate();
        }

        public T Get<T>() {
            if (this is T output) return output;
            return default;
        }

        public void Deactivate() {
            gameObject.SetActive(false);
        }
    }
}