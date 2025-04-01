using System;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.UI
{
    public class SelectionCircle : MonoBehaviour
    {
        private SpriteRenderer _circleRenderer;
        private SpriteMask _mask;

        private MaterialPropertyBlock _matBlock;

        private EventAgent _bus;
        private ISelectable _parent;

        private void Awake()
        {
            _circleRenderer = GetComponent<SpriteRenderer>();
            _bus = GetComponentInParent<EventAgent>();
            _mask = GetComponentInChildren<SpriteMask>();
            _parent = GetComponentInParent<ISelectable>();
            _matBlock = new MaterialPropertyBlock();

            //bus.AddListener<EntityInitEvent>(OnEntityInit);
            _bus.AddListener<UnitSelectEvent>(OnSelect);
            _bus.AddListener<UnitHoverEvent>(OnHover);
            _bus.AddListener<UnitOwnerChangeEvent>(OnTeamChange);
        }

        private void Start()
        {
            _circleRenderer.enabled = false;
            _mask.enabled = false;
        }

        private void OnTeamChange(UnitOwnerChangeEvent _event)
        {
            _circleRenderer.GetPropertyBlock(_matBlock);
            _matBlock.SetColor("_Color", _parent.GetRelationship(Player.Commander).Colour());
            _circleRenderer.SetPropertyBlock(_matBlock);
        }

        private void OnSelect(UnitSelectEvent _event)
        {
            _circleRenderer.enabled = _event.Status;
            _mask.enabled = _event.Status;
        }

        private void OnHover(UnitHoverEvent _event)
        {
            _circleRenderer.enabled = _event.Status;
            _mask.enabled = _event.Status;
        }

        private void OnEnable() {
            bool status = Player.HasSelected(_parent);

            _circleRenderer.enabled = status;
            _mask.enabled = status;
        }

        private void OnDisable()
        {
            _circleRenderer.enabled = false;
            _mask.enabled = false;
        }
    }
}