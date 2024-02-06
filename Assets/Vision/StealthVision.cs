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

		protected override void Start () {
			base.Start();

			bus.AddListener<SneakEvent>(OnSneak);

			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);

			rangeIndicator.SetActive(false);
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

			bus.Global(new UnitVisibleEvent(bus, parent, (visibleTo & Player.Main.VisionMask) > 0));
		}

		private void OnSneak (SneakEvent _event) {
			isSneaking = _event.IsSneaking;
			rangeIndicator.SetActive(_event.IsSneaking);
		}

		private void OnSelect (UnitSelectEvent _event) {
			/*if (isSneaking && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}*/
		}

		private void OnHover (UnitHoverEvent _event) {
			/*if (isSneaking && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}*/
		}
	}
}