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

		private void Awake () {
			bus = GetComponent<EventAgent>();

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		private void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Pre) {
				GameVision.Register(gameObject.name, this);

				owner = _event.ParentEntity.Get<ISelectable>("selectable").Owner;
			}
		}

		public VisionEntry Collect () {
			return new VisionEntry {
				gridPos = GameVision.GetGridPosFromWorldPos(transform.position),
				range = visionRange,
				height = Mathf.RoundToInt(transform.position.y),
				mask = Mask
			};
		}

		private void OnDrawGizmos () {
			if (GameVision.Initialized && GameVision.DrawGizmos) {
				Gizmos.DrawWireSphere(transform.position, visionRange * GameVision.NodeSize);
			}
		}
	}
}