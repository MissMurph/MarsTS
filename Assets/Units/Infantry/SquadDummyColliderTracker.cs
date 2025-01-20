using MarsTS.Events;
using UnityEngine;

namespace MarsTS.Units
{
    public class SquadDummyColliderTracker : MonoBehaviour
    {
        [SerializeField] private InfantryMember _trackedMember;
        [SerializeField] private bool _updateWithVision;

        private bool _isInitialized = false;
        private bool _isSelfDestructing = false;
        private Collider _collider;
        private EventAgent _memberBus;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void Init(InfantryMember member)
        {
            _trackedMember = member;

            _memberBus = member.GetComponent<EventAgent>();

            transform.position = member.transform.position;

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

        private void Update()
        {
            if (!_isInitialized
                || _isSelfDestructing) return;

            transform.position = _trackedMember.transform.position;
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