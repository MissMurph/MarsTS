using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.Vision {

    public class StealthVision : EntityVision {

        private SelectableSensor stealthSensor;

		private GameObject rangeIndicator;

		[SerializeField]
        private bool isSneaking;

		protected override void Awake () {
			base.Awake();

			stealthSensor = transform.Find("SneakRange").GetComponent<SelectableSensor>();
			rangeIndicator = transform.Find("SneakRangeIndicator").gameObject;
		}

		protected override void OnVisionUpdate (VisionUpdateEvent _event) {
			if (isSneaking) {
				int sneakMask = owner.VisionMask;

				foreach (ISelectable unit in stealthSensor.InRange) {
					if (unit.UnitType == "pumpjack") continue;
					if (unit.Owner is null) continue;
					sneakMask |= unit.Owner.VisionMask;
				}

				visibleTo = sneakMask;
			}
			else {
				visibleTo = GameVision.VisibleTo(gameObject);
			}

			bus.Global(new EntityVisibleEvent(bus, parent, GameVision.IsVisible(gameObject)));
		}
	}
}