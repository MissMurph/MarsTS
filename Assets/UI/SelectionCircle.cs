using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class SelectionCircle : MonoBehaviour {

        private SpriteRenderer circleRenderer;
		private SpriteMask mask;

		private EventAgent bus;

		private void Awake () {
			circleRenderer = GetComponent<SpriteRenderer>();
			bus = GetComponentInParent<EventAgent>();
			mask = GetComponentInChildren<SpriteMask>();
		}

		private void Start () {
			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);

			circleRenderer.enabled = false;
			mask.enabled = false;
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