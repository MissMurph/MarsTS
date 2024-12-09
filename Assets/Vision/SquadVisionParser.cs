using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.CanvasScaler;

namespace MarsTS.Vision {

    public class SquadVisionParser : EntityVision {

		private Dictionary<string, EntityVision> squadVision = new Dictionary<string, EntityVision>();

		protected override void Awake () {
			base.Awake();

			bus.AddListener<SquadRegisterEvent>(OnMemberRegister);
		}

		private void Start () {
			EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
		}

		public void OnMemberRegister (SquadRegisterEvent _event) {
			EventAgent unitEvents = _event.RegisteredMember.GameObject.GetComponent<EventAgent>();
			unitEvents.AddListener<UnitDeathEvent>(OnMemberDeath);
			unitEvents.AddListener<EntityInitEvent>(OnMemberInit);
		}

		protected override void OnVisionUpdate (VisionUpdateEvent _event) {
			if (_event.Phase == Phase.Post) {
				visibleTo = 0;

				foreach (EntityVision childVision in squadVision.Values) {
					visibleTo |= childVision.VisibleTo;
				}
			}
		}

		private void OnMemberDeath (UnitDeathEvent _event) {
			string deadKey = _event.Unit.GameObject.name;

			squadVision.Remove(deadKey);
		}

		private void OnMemberInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) return;
			squadVision[_event.ParentEntity.name] = _event.ParentEntity.Get<EntityVision>("vision");
		}
	}
}