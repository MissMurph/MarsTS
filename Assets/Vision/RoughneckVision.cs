using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class RoughneckVision : EntityVision {

        private SelectableSensor stealthSensor;

		[SerializeField]
        private bool isSneaking;

		protected override void Awake () {
			base.Awake();

			stealthSensor = transform.Find("SneakRange").GetComponent<SelectableSensor>();

			bus.AddListener<SneakEvent>(OnSneak);
		}

		protected override void Start () {
			base.Start();
		}

		protected override void OnVisionUpdate (VisionUpdateEvent _event) {
			

			if (isSneaking) {
				int sneakMask = owner.VisionMask;

				foreach (ISelectable unit in stealthSensor.InRange) {
					sneakMask |= unit.Owner.VisionMask;
				}

				visibleTo = sneakMask;
			}
			else {
				visibleTo = GameVision.VisibleTo(gameObject);
			}

			bus.Global(new UnitVisibleEvent(bus, parent, (visibleTo & Player.Main.VisionMask) > 0));
		}

		private void OnSneak (SneakEvent _event) {
			isSneaking = _event.IsSneaking;
		}
	}
}