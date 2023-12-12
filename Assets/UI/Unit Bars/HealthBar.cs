using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class HealthBar : UnitBar {

		private bool hurt;

		private void Start () {
			hurt = false;

			barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			IAttackable parent = GetComponentInParent<IAttackable>();

			FillLevel = (float)parent.Health / parent.MaxHealth;

			bus.AddListener<UnitHurtEvent>((_event) => {
				FillLevel = (float)_event.Targetable.Health / _event.Targetable.MaxHealth;

				if (FillLevel <= 1f) {
					hurt = true;
					barRenderer.enabled = true;
				}
			});

			bus.AddListener<UnitHoverEvent>((_event) => {
				if (_event.Status) barRenderer.enabled = true;
				else if (!hurt) barRenderer.enabled = false;
			});

			bus.AddListener<UnitSelectEvent>((_event) => {
				if (_event.Status) barRenderer.enabled = true;
				else if (!hurt) barRenderer.enabled = false;
			});
		}
	}
}