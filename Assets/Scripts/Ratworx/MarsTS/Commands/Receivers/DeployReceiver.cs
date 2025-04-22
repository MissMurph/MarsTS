using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class DeployReceiver : AbstractCommandReceiver<BooleanCommandlet>
    {
        [SerializeField] private bool _deployed;
        // How many seconds it takes to deploy
        [SerializeField] private int _deployTime;
        [SerializeField] private int _undeployTime;
        [SerializeField] private EntityAttribute _moveSpeedAttribute;
        
        public override bool CanCommand { get; }
        public override bool IsActive { get; }
        public override float Cooldown { get; }

        private BooleanCommandlet _deployCommandlet;
        private int _undeployedMoveSpeed;
        private Entity _entity;
        
        private void Awake() {
            _entity = GetComponent<Entity>();
        }
        
        private void Start () {
            if (_deployed) EventAgent.PostLocal(new DeployEvent(_entity, _deployed));
        }
        
        public override void ReceiveCommand (BooleanCommandlet command) {
            if (command.Target) {
                _undeployedMoveSpeed = _moveSpeedAttribute.Value;
                _moveSpeedAttribute.Value = 0;
                // RigidBody.velocity = Vector3.zero;
                _deployed = true;
            }
            else {
                EventAgent.PostLocal(new DeployEvent(_entity, false));
            }

            EventAgent.AddListener<CommandCompleteEvent>(DeployComplete);
        }

        private void DeployComplete (CommandCompleteEvent evnt) {
            // Bus.RemoveListener<CommandCompleteEvent>(DeployComplete);

            // _deployed = (evnt.Command as Commandlet<bool>).Target;

            if (_deployed) {
                // TODO: Implement command swapping on the Queue
                // boundCommands[deployCommandIndex] = "undeploy";
                EventAgent.PostLocal(new DeployEvent(_entity, _deployed));
            }
            else {
                _moveSpeedAttribute.Value = _undeployedMoveSpeed;
                // TODO: Implement command swapping on the Queue
                // boundCommands[deployCommandIndex] = "deploy";
            }
			
            // EventAgent.PostGlobal(new CommandsUpdatedEvent(_entity, boundCommands));
        }
        
        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) 
            => throw new System.NotImplementedException();
    }
}