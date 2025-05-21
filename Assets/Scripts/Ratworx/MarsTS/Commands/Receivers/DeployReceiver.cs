using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units.Turrets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class DeployReceiver : AbstractCommandReceiver<BooleanCommandlet>,
                                  IEntityServerUpdate,
                                  IEntityClientUpdate,
                                  ICostingCommand
    {
        [SerializeField] private bool _deployed;
        // How many seconds it takes to deploy
        [SerializeField] private int _deployTime;
        [SerializeField] private int _undeployTime;
        [SerializeField] private EntityAttribute _moveSpeedAttribute;
        [SerializeField] private ProjectileTurret _artilleryTurret;

        public override bool CanCommand => true;
        public override bool IsActive => _deployed;
        public override float Cooldown => _currentDeployTime;

        private bool _deploying;
        // private bool _undeploying;
        private float _currentDeployTime;
        private BooleanCommandlet _deployCommandlet;
        private int _undeployedMoveSpeed;
        
        private Entity _entity;
        
        private void Awake() {
            _entity = GetComponent<Entity>();

            _currentDeployTime = 0f;
        }
        
        private void Start () {
            if (_deployed) EventAgent.PostLocal(new DeployEvent(_entity, _deployed));
        }
        
        public override void ReceiveCommand (BooleanCommandlet command) {
            _deployCommandlet = command;
            _deployCommandlet.Callback.AddListener(OnCommandComplete);
            
            _deployCommandlet = command;

            if (!_deployCommandlet.Target) return;
            
            _undeployedMoveSpeed = _moveSpeedAttribute.Value;
            _deploying = command.Target;

            if (!NetworkManager.Singleton.IsServer) return;

            _moveSpeedAttribute.Value = 0;
        }

        public void UpdateServer() {
            if (_deployCommandlet is null) return;

            _currentDeployTime += Time.deltaTime;

            float deployTimer = _deploying ? _deployTime : _undeployTime;

            EventAgent.PostGlobal(new CommandWorkEvent(_deployCommandlet, CommandQueue,
                _currentDeployTime / deployTimer));

            if (_currentDeployTime < deployTimer) return;
            
            _deployCommandlet.CompleteCommand(CommandQueue);
            _currentDeployTime = 0f;
        }

        public void UpdateClient() {
            if (_deployCommandlet is null) return;

            _currentDeployTime += Time.deltaTime;

            float deployTimer = _deploying ? _deployTime : _undeployTime;

            EventAgent.PostGlobal(new CommandWorkEvent(_deployCommandlet, CommandQueue,
                _currentDeployTime / deployTimer));
        }

        private void OnCommandComplete (CommandCompleteEvent evnt) {
            EventAgent.PostGlobal(new CommandWorkEvent(_deployCommandlet, CommandQueue, 1f));
            _deployCommandlet.Callback.RemoveListener(OnCommandComplete);
            _deployCommandlet = null;
            
            if (!NetworkManager.Singleton.IsServer) return;
            
            if (_deployed) {
                // TODO: Implement command swapping on the Queue
                // boundCommands[deployCommandIndex] = "undeploy";
                _artilleryTurret.gameObject.SetActive(true);
                EventAgent.PostLocal(new DeployEvent(_entity, _deployed));
            }
            else {
                _moveSpeedAttribute.Value = _undeployedMoveSpeed;
                _artilleryTurret.gameObject.SetActive(false);
                // TODO: Implement command swapping on the Queue
                // boundCommands[deployCommandIndex] = "deploy";
            }
        }
        
        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) => (false, null);
        public CostEntry[] GetCost() => new CostEntry[1] { new CostEntry { key = "time", amount = 5 } };
    }
}