using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System;
using UnityEngine;

namespace MarsTS.Vision {

	[RequireComponent(typeof(EntityVision))]
	public class EntityStealth : MonoBehaviour, ITaggable<EntityStealth> {

		/*	ITaggable Properties	*/

		public string Key { get { return "stealth"; } }

		public Type Type { get { return typeof(EntityStealth); } }

		/*	Stealth Fields	*/

        private SelectableSensor stealthSensor;

		[SerializeField]
        private bool isSneaking;

		private EntityVision visionComponent;

		private ISelectable parent;

		private EventAgent bus;

		private void Awake () {
			parent = GetComponent<ISelectable>();
			bus = GetComponent<EventAgent>();
			visionComponent = GetComponent<EntityVision>();

			stealthSensor = transform.Find("SneakRange").GetComponent<SelectableSensor>();
		}

		private void Start () {
			bus.AddListener<SneakEvent>(OnSneak);
			bus.AddListener<EntityVisibleCheckEvent>(OnVisionCheck);
		}

		private void OnVisionCheck (EntityVisibleCheckEvent _event) {
			if (_event.Phase == Phase.Post) return;

			if (isSneaking) {
				int sneakMask = parent.Owner.VisionMask;

				foreach (ISelectable unit in stealthSensor.InRange) {
					if (unit.UnitType == "pumpjack") continue;
					if (unit.Owner is null) continue;
					sneakMask |= unit.Owner.VisionMask;
				}

				_event.VisibleTo = sneakMask;
			}
		}

		private void OnSneak (SneakEvent _event) {
			isSneaking = _event.IsSneaking;
		}

		public EntityStealth Get () {
			return this;
		}
	}
}