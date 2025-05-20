using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Infantry
{
    // [RequireComponent(NetworkTransform)]
    public class SquadColliderTracker : MonoBehaviour,
                                        IEntityServerUpdate,
                                        IEntityClientUpdate
    {
        [FormerlySerializedAs("_trackedMember")] [SerializeField] private InfantryMembership _trackedMembership;
        [SerializeField] private bool _updateWithVision;

        private bool _isInitialized = false;
        private bool _isSelfDestructing = false;
        private Collider _collider;
        private EventAgent _memberBus;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void Init(InfantryMembership membership)
        {
            _trackedMembership = membership;

            _memberBus = membership.GetComponent<EventAgent>();

            transform.position = membership.transform.position;

            AttachListenersToMember();
            
            _isInitialized = true;
        }

        private void AttachListenersToMember()
        {
            _memberBus.AddListener<UnitDeathEvent>(SelfDestruct);

            if (_updateWithVision)
                _memberBus.AddListener<EntityVisibleEvent>(UpdateVisibility);
        }

        private void UpdateVisibility(EntityVisibleEvent evnt)
        {
            if (!_isInitialized
            || _isSelfDestructing
            || !_updateWithVision
            || evnt.Phase == Phase.Pre) 
                return;

            _collider.enabled = evnt.Visible;
        }

        public void UpdateServer() {
            if (!_isInitialized
                || _isSelfDestructing
                || !_trackedMembership) return;

            transform.position = _trackedMembership.transform.position;
        }

        public void UpdateClient() {
            // We don't want this to run twice on the server (I mean it's not that bad...)
            if (NetworkManager.Singleton.IsServer
                || !_isInitialized
                || _isSelfDestructing
                || !_trackedMembership) return;

            transform.position = _trackedMembership.transform.position;
        }

        public void SetUpdatingWithVision(bool status)
        {
            _updateWithVision = true;

            if (_isInitialized)
                _memberBus.AddListener<EntityVisibleEvent>(UpdateVisibility);
        }

        private void SelfDestruct(UnitDeathEvent evnt)
        {
            _isSelfDestructing = true;
            
            transform.position -= Vector3.down * 1000f;
            Destroy(gameObject, 0.1f);
        }
    }
}