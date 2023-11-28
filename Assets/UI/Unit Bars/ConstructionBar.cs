using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class ConstructionBar : UnitBar {

		private void Start () {
			IAttackable parent = GetComponentInParent<IAttackable>();

			if (parent.Health == 1) {
				FillLevel = 0f;

				barRenderer.enabled = true;

				GetComponentInParent<EventAgent>().AddListener<EntityHurtEvent>((_event) => {
					FillLevel = (float)_event.Unit.Health / _event.Unit.MaxHealth;
					if (FillLevel >= 1f) Destroy(gameObject);
				});
			}
			else {
				barRenderer.enabled = false;
			}
		}
	}
}