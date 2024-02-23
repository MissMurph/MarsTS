using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.Vision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class MinimapIcon : MonoBehaviour {

		private SpriteRenderer iconRenderer;

		private MaterialPropertyBlock matBlock;

		private EventAgent bus;
		private ISelectable parent;

		private void Awake () {
			iconRenderer = GetComponent<SpriteRenderer>();
			bus = GetComponentInParent<EventAgent>();
			parent = GetComponentInParent<ISelectable>();
			matBlock = new MaterialPropertyBlock();
		}

		private void Start () {
			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
			bus.AddListener<UnitOwnerChangeEvent>(OnTeamChange);
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);

			iconRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Main).Colour());
			iconRenderer.SetPropertyBlock(matBlock);
		}

		private void OnTeamChange (UnitOwnerChangeEvent _event) {
			iconRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Main).Colour());
			iconRenderer.SetPropertyBlock(matBlock);
		}

		private void OnVisionInit (VisionInitEvent _event) {
			bool visible = GameVision.IsVisible(transform.root.gameObject);

			iconRenderer.enabled = visible;
		}

		private void OnVisionUpdate (EntityVisibleEvent _event) {
			iconRenderer.enabled = _event.Visible;
		}
	}
}