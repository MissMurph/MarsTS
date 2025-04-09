using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Vision
{
    public class UnitVision : MonoBehaviour,
                              IEntityComponent<UnitVision>,
                              IUnitInterface
    {
        public Action<bool> OnUnitVisionChange;
        public int Mask => _ownership.Owner.VisionMask;
        public int Range => _visionRange;

        public int VisibleTo {
            get => VisibilityMask;
            set => VisibilityMask = value;
        }

        public string Key => "vision";

        [FormerlySerializedAs("visionRange")]
        [SerializeField]
        private int _visionRange;

        protected int VisibilityMask;

        /*	Vision Fields	*/

        protected EventAgent Bus;

        private Entity _entity;
        private UnitOwnership _ownership;
        
        [SerializeField]
        private GameObject[] _hideables;

        protected virtual void Awake() {
            Bus = GetComponent<EventAgent>();
            _entity = GetComponent<Entity>();
            _ownership = GetComponent<UnitOwnership>();

            _entity.OnEntityInit += OnEntityInit;

            // Bus.AddListener<UnitOwnerChangeEvent>(OnOwnerChange);
        }

        private void OnVisionInit(VisionInitEvent _event) {
            VisibilityMask = GameVision.VisibleTo(gameObject);

            Bus.PostGlobal(new EntityVisibleEvent(this, GameVision.IsVisible(gameObject)));
        }

        private void OnEntityInit(Phase phase) {
            if (phase == Phase.Pre) return;

            GameVision.Register(gameObject.name, this);
            
            EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
            EventBus.AddListener<VisionInitEvent>(OnVisionInit);
        }

        protected virtual void OnVisionUpdate(VisionUpdateEvent evnt) {
            if (evnt.Phase != Phase.Pre) return;

            int visibility = GameVision.VisibleTo(gameObject);
            // Add owner bit to the vision mask
            visibility |= Mask;

            EntityVisibleCheckEvent checkEvent = new EntityVisibleCheckEvent(this, visibility);
            checkEvent.Phase = Phase.Pre;

            //Here local components can intercept the visibility status and change it before it's applied
            Bus.PostGlobal(checkEvent);

            checkEvent.Phase = Phase.Post;

            //Here global objects can intercept the visibilty status and modify it further
            Bus.PostGlobal(checkEvent);

            VisibilityMask = checkEvent.VisibleTo;

            UpdateEntityVisibility();
        }

        private void UpdateEntityVisibility() {
            EntityVisibleEvent entityEvent = new EntityVisibleEvent(this, GameVision.IsVisible(gameObject));

            entityEvent.Phase = Phase.Pre;

            //Posting here allows other components to modify the units visibility
            Bus.PostGlobal(entityEvent);
            
            foreach (GameObject hideable in _hideables)
            {
                hideable.SetActive(entityEvent.Visible);
            }

            entityEvent.Phase = Phase.Post;
            //Posting here is where the entity updates all its objects
            Bus.PostGlobal(entityEvent);
        }

        public VisionEntry Collect() =>
            new VisionEntry
            {
                gridPos = GameVision.GetGridPosFromWorldPos(transform.position),
                range = Mathf.RoundToInt(_visionRange / GameVision.NodeSize),
                height = Mathf.RoundToInt(transform.position.y),
                mask = Mask
            };

        private void OnDrawGizmos() {
            if (GameVision.Initialized && GameVision.DrawGizmos)
                Gizmos.DrawWireSphere(transform.position, _visionRange * GameVision.NodeSize);
        }

        public UnitVision Get() => this;
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
    }
}