using System;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Commandlets {
    public class SneakCommandlet : Commandlet<bool> {

        private NetworkVariable<float> _deactivateCooldown
            = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
        
        private NetworkVariable<float> _reactivateCooldown
            = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
        
        public void InitSneak (
            string commandName, 
            bool target, 
            Faction commander, 
            float deactivateCooldown, 
            float reactivateCooldown
        ) {
            _deactivateCooldown.Value = deactivateCooldown;
            _reactivateCooldown.Value = reactivateCooldown;
            
            Init(commandName, target, commander);
        }

        public override string SerializerKey => "simple";

        public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) {
            base.ActivateCommand(queue, _event);

            if (_event.Activity) 
                queue.Cooldown(this, _deactivateCooldown.Value);
            else 
                queue.Cooldown(this, _reactivateCooldown.Value);
        }

        public override Commandlet Clone()
        {
            throw new NotImplementedException();
        }
        
        protected override void Deserialize (SerializedCommandWrapper data) {
            base.Deserialize(data);

            var deserialized = (SerializedBoolCommandlet)data.commandletData;
			
            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            _target = deserialized.Status;
        }
    }
}