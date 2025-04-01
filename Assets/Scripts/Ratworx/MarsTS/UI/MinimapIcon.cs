using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;
using UnityEngine;

namespace Ratworx.MarsTS.UI {

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

			//bus.AddListener<EntityInitEvent>(OnEntityInit);
			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
			bus.AddListener<UnitOwnerChangeEvent>(OnTeamChange);
		}

		private void Start () {
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		private void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Pre) return;

			iconRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Player.Commander).Colour());
			iconRenderer.SetPropertyBlock(matBlock);
		}

		private void OnTeamChange (UnitOwnerChangeEvent _event) {
			iconRenderer.GetPropertyBlock(matBlock);
			matBlock.SetColor("_Color", parent.GetRelationship(Player.Player.Commander).Colour());
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