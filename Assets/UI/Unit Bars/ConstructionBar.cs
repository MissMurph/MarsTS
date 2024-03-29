using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class ConstructionBar : UnitBar {

		private void Start () {
			IAttackable parent = GetComponentInParent<IAttackable>();

			if (parent.Health <= 1) {
				FillLevel = 0f;

				barRenderer.enabled = true;

				GetComponentInParent<EventAgent>().AddListener<UnitHurtEvent>((_event) => {
					FillLevel = (float)_event.Targetable.Health / _event.Targetable.MaxHealth;
					if (FillLevel >= 1f) gameObject.SetActive(false);
				});
			}
			else {
				barRenderer.enabled = false;
			}
		}
	}
}