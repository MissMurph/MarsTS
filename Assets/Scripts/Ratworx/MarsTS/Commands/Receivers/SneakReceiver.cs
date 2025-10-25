using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class SneakReceiver : AbstractCommandReceiver<BooleanCommandlet>
    {
        [SerializeField] private EntityAttribute _moveSpeedAttribute;
        [SerializeField] private GameObject _weapon;
        [Range(0f, 1f)] [SerializeField] private float _sneakingSpeedModifier;
        [SerializeField] private int _deactivateCooldown;
        [SerializeField] private int _reactivateCooldown;

        public override bool CanCommand => true;
        public override bool IsActive => _isSneaking;
        public override float Cooldown => 0f;

        private Entity _entity;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;
        private bool _isSneaking;
        private int _preModifiedMoveSpeed;

        protected override void Awake() {
            base.Awake();

            _entity = GetComponentInParent<Entity>();
            _eventAgent = GetComponentInParent<EventAgent>();
            _commandQueue = GetComponentInParent<CommandQueue>();
        }

        public override void ReceiveCommand(BooleanCommandlet command) {
            // TODO: Convert below into a cooldowns component maybe?
            _commandQueue.Cooldown(CommandKey, command.Target ? _deactivateCooldown : _reactivateCooldown);
            _isSneaking = command.Target;
            _eventAgent.PostLocal(new SneakEvent(_entity, _isSneaking));
            command.CompleteCommand(_commandQueue);
            
            if (!NetworkManager.Singleton.IsServer) return;

            // TODO: Implement below as a modifiers system to better track modified values
            if (command.Target) {
                _preModifiedMoveSpeed = _moveSpeedAttribute.Value;
                _moveSpeedAttribute.Value = Mathf.RoundToInt(_moveSpeedAttribute.Value * _sneakingSpeedModifier);
            }
            else {
                _moveSpeedAttribute.Value = _preModifiedMoveSpeed;
                _preModifiedMoveSpeed = 0;
            }
        }

        public override (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);
    }
}