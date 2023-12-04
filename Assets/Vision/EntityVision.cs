using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class EntityVision : MonoBehaviour {
        
        public int Mask { get { return owner.ID; } }

		private Faction owner;

		private EventAgent bus;

		public int Range { get { return visionRange; } }

		[SerializeField]
		private int visionRange;

		//Will evenly divide the amount of raycasts in a circle around the entity
		[SerializeField]
		private int rayCount;

		[SerializeField]
		private bool drawGizmos;

		[SerializeField]
		private float visionUpdateTime;

		private void Awake () {
			bus = GetComponent<EventAgent>();

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		private void OnEntityInit (EntityInitEvent _event) {
			Vision.Register(gameObject.name, this);

			owner = _event.ParentEntity.Get<ISelectable>("selectable").Owner;
		}

		public VisionEntry Collect () {
			return new VisionEntry {
				gridPos = Vision.GetGridPosFromWorldPos(transform.position),
				range = visionRange,
				height = Mathf.RoundToInt(transform.position.y),
				mask = Mask
			};
		}
	}
}