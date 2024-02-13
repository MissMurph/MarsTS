using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.UI {

	public class StealthCircle : MonoBehaviour {

		private SpriteRenderer circleRenderer;
		private SpriteMask mask;
		private bool isSneaking;

		private EventAgent bus;

		private void Awake () {
			circleRenderer = GetComponent<SpriteRenderer>();
			bus = GetComponentInParent<EventAgent>();
			mask = GetComponentInChildren<SpriteMask>();

			isSneaking = false;
		}

		private void Start () {
			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);
			bus.AddListener<SneakEvent>(OnSneak);

			SetRendering(false);
		}

		private void OnSneak (SneakEvent _event) {
			isSneaking = _event.IsSneaking;

			SetRendering(true);
		}

		private void OnSelect (UnitSelectEvent _event) {
			if (isSneaking && _event.Status) {
				SetRendering(true);
			}
			else {
				SetRendering(false);
			}
		}

		private void OnHover (UnitHoverEvent _event) {
			if (isSneaking && _event.Status) {
				SetRendering(true);
			}
			else {
				SetRendering(false);
			}
		}

		private void SetRendering (bool status) {
			circleRenderer.enabled = status;
			mask.enabled = status;
		}
	}
}