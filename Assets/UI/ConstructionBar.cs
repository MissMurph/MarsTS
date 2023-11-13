using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class ConstructionBar : UnitBar {

		private void Start () {
			FillLevel = 0f;

			barRenderer.enabled = true;

			GetComponentInParent<EventAgent>().AddListener<EntityHurtEvent>((_event) => {
				FillLevel = (float)_event.Unit.Health / _event.Unit.MaxHealth;
				if (FillLevel >= 1f) Destroy(gameObject);
			});
		}
	}
}