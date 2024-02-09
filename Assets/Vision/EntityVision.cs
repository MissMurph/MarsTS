using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class EntityVision : MonoBehaviour, ITaggable<EntityVision> {

		/*	ITaggable Properties	*/

		public string Key { get { return "vision"; } }

		public Type Type { get { return typeof(EntityVision); } }

		/*	Vision Properties	*/

		public int Mask { get { return owner.VisionMask; } }

		public int Range { get { return visionRange; } }

		public int VisibleTo { get { return visibleTo; } }

		[SerializeField]
		private int visionRange;

		protected int visibleTo;

		/*	Vision Fields	*/

		protected Faction owner;

		protected EventAgent bus;

		protected ISelectable parent;

		protected virtual void Awake () {
			bus = GetComponent<EventAgent>();

			bus.AddListener<EntityInitEvent>(OnEntityInit);
			bus.AddListener<UnitOwnerChangeEvent>(OnOwnerChange);
		}

		protected virtual void Start () {
			EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
		}

		private void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Pre) {
				GameVision.Register(gameObject.name, this);

				parent = _event.ParentEntity.Get<ISelectable>("selectable");
				owner = parent.Owner;
			}
		}

		protected virtual void OnVisionUpdate (VisionUpdateEvent _event) {
			visibleTo = GameVision.VisibleTo(gameObject);

			bus.Global(new EntityVisibleEvent(bus, parent, GameVision.IsVisible(gameObject)));
		}

		private void OnOwnerChange (UnitOwnerChangeEvent _event) {
			owner = _event.NewOwner;
		}

		public VisionEntry Collect () {
			return new VisionEntry {
				gridPos = GameVision.GetGridPosFromWorldPos(transform.position),
				range = Mathf.RoundToInt(visionRange / GameVision.NodeSize),
				height = Mathf.RoundToInt(transform.position.y),
				mask = Mask
			};
		}

		private void OnDrawGizmos () {
			if (GameVision.Initialized && GameVision.DrawGizmos) {
				Gizmos.DrawWireSphere(transform.position, visionRange * GameVision.NodeSize);
			}
		}

		public EntityVision Get () {
			return this;
		}
	}
}