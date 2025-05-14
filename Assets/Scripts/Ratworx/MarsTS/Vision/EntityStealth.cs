using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Vision
{
    [RequireComponent(typeof(UnitVision))]
    public class EntityStealth : MonoBehaviour, IEntityComponent<EntityStealth>
    {
        [SerializeField] private bool _isSneaking;
        public string Key => "stealth";

        private SelectableSensor _stealthSensor;
        private EventAgent _eventAgent;
        private UnitOwnership _ownership;

        private void Awake() {
            _eventAgent = GetComponentInParent<EventAgent>();
            _ownership = GetComponentInParent<UnitOwnership>();

            _stealthSensor = transform.Find("SneakRange").GetComponent<SelectableSensor>();
        }

        private void Start() {
            _eventAgent.AddListener<SneakEvent>(OnSneak);
            _eventAgent.AddListener<EntityVisibleCheckEvent>(OnVisionCheck);
        }

        private void OnVisionCheck(EntityVisibleCheckEvent _event) {
            if (_event.Phase == Phase.Post) return;
            if (!_isSneaking) return;
            
            int sneakMask = _ownership.Owner.VisionMask;

            foreach (ISelectable unit in _stealthSensor.InRange) {
                if (unit.Entity.RegistryKey.Contains("pumpjack")) continue;
                if (unit.Owner is null) continue;
                
                sneakMask |= unit.Owner.VisionMask;
            }

            _event.VisibleTo = sneakMask;
        }

        private void OnSneak(SneakEvent evnt) {
            _isSneaking = evnt.IsSneaking;
        }

        public EntityStealth Get() {
            return this;
        }
    }
}