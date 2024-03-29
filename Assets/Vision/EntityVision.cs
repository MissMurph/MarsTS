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

		public int VisibleTo { get { return visibleTo; } set { visibleTo = value; } }

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
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		private void OnVisionInit (VisionInitEvent _event) {
			visibleTo = GameVision.VisibleTo(gameObject);

			bus.Global(new EntityVisibleEvent(bus, parent, GameVision.IsVisible(gameObject)));
		}

		private void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) {
				GameVision.Register(gameObject.name, this);

				parent = _event.ParentEntity.Get<ISelectable>("selectable");
				owner = parent.Owner;
			}
		}

		protected virtual void OnVisionUpdate (VisionUpdateEvent _event) {
			if (_event.Phase == Phase.Pre) {
				int visibility = GameVision.VisibleTo(gameObject);

				EntityVisibleCheckEvent checkEvent = new EntityVisibleCheckEvent(bus, parent, visibility);
				checkEvent.Phase = Phase.Pre;

				//Here local components can intercept the visibility status and change it before it's applied
				bus.Global(checkEvent);
				
				checkEvent.Phase = Phase.Post;

				//Here global objects can intercept the visibilty status and modify it further
				bus.Global(checkEvent);

				visibleTo = checkEvent.VisibleTo;

				UpdateEntityVisibility();
			}
		}

		private void UpdateEntityVisibility () {
			EntityVisibleEvent entityEvent = new EntityVisibleEvent(bus, parent, GameVision.IsVisible(gameObject));

			entityEvent.Phase = Phase.Pre;

			//Posting here allows other components to modify the units visibility
			bus.Global(entityEvent);

			entityEvent.Phase = Phase.Post;

			//Posting here is where the entity updates all its objects
			bus.Global(entityEvent);
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