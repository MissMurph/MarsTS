using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class SelectionCircle : MonoBehaviour {

        private SpriteRenderer circleRenderer;
		private SpriteMask mask;

		private MaterialPropertyBlock matBlock;

		private EventAgent bus;
		private ISelectable parent;

		private void Awake () {
			circleRenderer = GetComponent<SpriteRenderer>();
			bus = GetComponentInParent<EventAgent>();
			mask = GetComponentInChildren<SpriteMask>();
			parent = GetComponentInParent<ISelectable>();
			matBlock = new MaterialPropertyBlock();

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		private void Start () {
			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);
			bus.AddListener<UnitOwnerChangeEvent>(OnTeamChange);

			circleRenderer.enabled = false;
			mask.enabled = false;
		}

		private void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Pre) return;

			circleRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Commander).Colour());
			circleRenderer.SetPropertyBlock(matBlock);
		}

		private void OnTeamChange (UnitOwnerChangeEvent _event) {
			circleRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Commander).Colour());
			circleRenderer.SetPropertyBlock(matBlock);
		}

		private void OnSelect (UnitSelectEvent _event) {
			circleRenderer.enabled = _event.Status;
			mask.enabled = _event.Status;
		}

		private void OnHover (UnitHoverEvent _event) {
			circleRenderer.enabled = _event.Status;
			mask.enabled = _event.Status;
		}
	}
}