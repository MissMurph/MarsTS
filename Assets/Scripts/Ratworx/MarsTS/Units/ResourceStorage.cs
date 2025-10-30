using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.UI.Unit_Pane;
using UnityEngine;

namespace Ratworx.MarsTS.Units {
    public class ResourceStorage : EntityAttribute {
        public int Capacity => _capacity;
        public string Resource => _resourceKey;

        [SerializeField]
        private int _capacity;

        [SerializeField]
        private string _resourceKey;

        private EventAgent _eventAgent; 
        
        protected Entity Entity;

        protected virtual void Awake() {
            Entity = GetComponentInParent<Entity>();
            _eventAgent = GetComponentInParent<EventAgent>();
            
            _key = "storage:" + _resourceKey;
        }

        private void Start() {
            _eventAgent.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        public int Submit(int amount) {
            int newAmount = Mathf.Min(_capacity, Value + amount);

            int difference = newAmount - Value;

            Value = newAmount;

            return difference;
        }
        
        private void OnUnitInfoDisplayed(UnitInfoEvent _event) {
            UnitResourceStorageInfo info = _event.Info.Module<UnitResourceStorageInfo>("storage");
            info.SetStorage(this);
        }
    }
}