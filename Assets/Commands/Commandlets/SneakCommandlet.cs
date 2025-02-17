using System;
using MarsTS.Events;
using MarsTS.Teams;

namespace MarsTS.Commands {
    public class SneakCommandlet : Commandlet<bool> {

        private float _deactivateCooldown;
        private float _reactivateCooldown;

        public void InitSneak (
            string commandName, 
            bool target, 
            Faction commander, 
            float deactivateCooldown, 
            float reactivateCooldown
        ) {
            _deactivateCooldown = deactivateCooldown;
            _reactivateCooldown = reactivateCooldown;
            
            Init(commandName, target, commander);
        }

        public override string SerializerKey => "simple";

        public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) {
            base.ActivateCommand(queue, _event);

            if (_event.Activity) 
                queue.Cooldown(this, _deactivateCooldown);
            else 
                queue.Cooldown(this, _reactivateCooldown);
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