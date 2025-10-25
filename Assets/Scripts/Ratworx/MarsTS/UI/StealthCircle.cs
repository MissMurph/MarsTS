using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.UI
{
    public class StealthCircle : MonoBehaviour
    {
        private SpriteMask _mask;
        private bool _isSneaking;

        private SpriteRenderer _circleRenderer;
        private EventAgent _bus;
        private UnitSelection _unitSelection;

        private void Awake() {
            _circleRenderer = GetComponent<SpriteRenderer>();
            _bus = GetComponentInParent<EventAgent>();
            _mask = GetComponentInChildren<SpriteMask>();
            _unitSelection = GetComponentInParent<UnitSelection>();

            _isSneaking = false;
        }

        private void Start() {
            _bus.AddListener<UnitSelectEvent>(OnSelect);
            _bus.AddListener<UnitHoverEvent>(OnHover);
            _bus.AddListener<SneakEvent>(OnSneak);

            SetRendering(false);
        }

        private void OnSneak(SneakEvent evnt) {
            _isSneaking = evnt.IsSneaking;

            if (!_unitSelection.IsSelected && _isSneaking) return;
            
            SetRendering(_isSneaking);
        }

        private void OnSelect(UnitSelectEvent evnt) => SetRendering(_isSneaking && evnt.Status);

        private void OnHover(UnitHoverEvent evnt) {
            if (_unitSelection.IsSelected) 
                return;
            
            SetRendering(_isSneaking && evnt.Status);
        }

        private void SetRendering(bool status) {
            _circleRenderer.enabled = status;
            _mask.enabled = status;
        }
    }
}