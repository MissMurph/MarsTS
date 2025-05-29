using System.Linq;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class FlareReceiver : AbstractCommandReceiver<MoveCommandlet>,
                                 IEntityServerUpdate,
                                 ICostingCommand
    {
        [SerializeField] private ResourceCost[] _cost;
        [SerializeField] private int _cooldown;
        [SerializeField] private int _flareRange;
        // TODO: Replace below with getting from the registry
        [SerializeField] private GameObject _flarePrefab;

        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;

        private ResourceCost[] _resourceSpendOnCommand;
        private MoveCommandlet _command;

        public override void ReceiveCommand(MoveCommandlet command) {
            _command = command;
            UnitPathing.FindPathTo(_command.Target);

            _command.OnCommandComplete.AddListener(OnCommandComplete);
            
            if (!NetworkManager.Singleton.IsServer) return;

            _resourceSpendOnCommand = _cost;
        }

        public void UpdateServer() {
            if (_command is null) return;

            if ((transform.position - _command.Target).sqrMagnitude < _flareRange * _flareRange) 
                FireFlare();
        }

        private void FireFlare() {
            GameObject firedFlare = Instantiate(_flarePrefab, _command.Target, Quaternion.Euler(Vector3.zero));
            NetworkObject networkObject = firedFlare.GetComponent<NetworkObject>();
            networkObject.Spawn();
            
            UnitOwnership flareOwnership = firedFlare.GetComponent<UnitOwnership>();
            flareOwnership.SetOwner(Ownership.Owner);
            
            UnitPathing.ClearPath();
            _command.CompleteCommand(CommandQueue);
            _command = null;
        }

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            evnt.Command.OnCommandComplete.RemoveListener(OnCommandComplete);
            
            if (evnt.IsCancelled) {
                if (!NetworkManager.Singleton.IsServer) return;
                
                foreach (ResourceCost costing in _resourceSpendOnCommand) {
                    Ownership.Owner.GetResource(costing.key).Deposit(costing.amount);
                }
            }
            else
                CommandQueue.Cooldown(evnt.Command, _cooldown);
        }

        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) => throw new System.NotImplementedException();

        public ResourceCost[] GetCost() => _cost.Append(new ResourceCost{ key = "time", amount = _cooldown}).ToArray();
    }
}