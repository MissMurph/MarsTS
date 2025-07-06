using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    // TODO: Investigate adding this _after_ initialization
    public class AdrenalineReceiver : AbstractCommandReceiver<BooleanCommandlet>,
                                      IEntityServerUpdate,
                                      IEntityClientUpdate
    {
        [SerializeField] private int _duration;
        [SerializeField] private int _cooldown;
        [SerializeField] private float _moveSpeedModifier;
        [SerializeField] private EntityAttribute _moveSpeedAttribute;

        public override bool CanCommand => !_isActive;
        public override bool IsActive => _isActive;
        public override float Cooldown => _remainingBoostingTime;

        private int _preModifiedMoveSpeed;
        private bool _isActive;
        private float _remainingBoostingTime;
        private BooleanCommandlet _adrenalineCommandlet;
        private Entity _entity;

        protected override void Awake() {
            _entity = GetComponentInParent<Entity>();
        }

        // TODO: Simple command?
        public override void ReceiveCommand(BooleanCommandlet command) {
            _adrenalineCommandlet = command;
            _remainingBoostingTime = _duration;
            _adrenalineCommandlet.OnCommandComplete.AddListener(OnCommandComplete);

            if (!NetworkManager.Singleton.IsServer) return;

            _preModifiedMoveSpeed = _moveSpeedAttribute.Value;
            _moveSpeedAttribute.Value = Mathf.RoundToInt(_moveSpeedAttribute.Value * _moveSpeedModifier);
            _adrenalineCommandlet.CompleteCommand(CommandQueue);
        }

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _adrenalineCommandlet.OnCommandComplete.RemoveListener(OnCommandComplete);
            _adrenalineCommandlet = null;

            if (!NetworkManager.Singleton.IsServer) return;

            _moveSpeedAttribute.Value = _preModifiedMoveSpeed;
            _preModifiedMoveSpeed = 0;
        }

        public void UpdateServer() {
            if (_adrenalineCommandlet is null) return;

            _remainingBoostingTime -= Time.deltaTime;
            EventAgent.PostGlobal(new CooldownEvent(this, _entity, _remainingBoostingTime));

            // if (_remainingBoostingTime > 0f) return;
            // _adrenalineCommandlet.CompleteCommand(CommandQueue);
        }

        public void UpdateClient() {
            if (_adrenalineCommandlet is null) return;

            _remainingBoostingTime -= Time.deltaTime;
            EventAgent.PostGlobal(new CooldownEvent(this, _entity, _remainingBoostingTime));
        }

        // TODO: Investigate turning below into an optional interface, numerous receivers just do the below
        public override (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);
    }
}