using System;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets {
    public class DeployCommandlet : Commandlet<bool>, IWorkable {

        public int WorkRequired
        {
            get => _workRequired.Value;
            private set => _workRequired.Value = value;
        }

        public int CurrentWork
        {
            get => _workProgress.Value;
            set => _workProgress.Value = value;
        }
        
        public event Action<int, int> OnWork;
        public override string SerializerKey => "simple";

        [SerializeField]
        private NetworkVariable<int> _workProgress = new(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField]
        private NetworkVariable<int> _workRequired = new(writePerm: NetworkVariableWritePermission.Server);

        public void InitDeploy(string commandName, bool target, Faction commander, int timeRequired) {
            WorkRequired = timeRequired;
            CurrentWork = 0;

            Init(commandName, target, commander);
        }

        public override void StartCommand (EventAgent eventAgent, ICommandable unit) {
            base.StartCommand(eventAgent, unit);

            if (TryGetQueue(unit, out var queue))
                queue.Cooldown(this, WorkRequired);
        }

        public override void CompleteCommand (EventAgent eventAgent, ICommandable unit, bool isCancelled = false) {
            if (TryGetQueue(unit, out var queue)) {
                if (Target) 
                    queue.Activate(this, Target);
                else
                    queue.Deactivate("deploy");
            }
            base.CompleteCommand(eventAgent, unit, isCancelled);
        }

        public override bool CanInterrupt () {
            return false;
        }

        public override Commandlet Clone() => throw new NotImplementedException();
        
        protected override void Deserialize (SerializedCommandWrapper data) {
            base.Deserialize(data);

            var deserialized = (SerializedBoolCommandlet)data.commandletData;
			
            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            _target = deserialized.Status;
        }
    }
}