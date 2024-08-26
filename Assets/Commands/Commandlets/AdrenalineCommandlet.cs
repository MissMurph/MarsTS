using System;
using MarsTS.Events;
using MarsTS.Teams;

namespace MarsTS.Commands {
    public class AdrenalineCommandlet : Commandlet<bool> {

        private float duration;

        private float cooldown;

        public override string Key => Name;

        public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) 
            => queue.Cooldown(this, _event.Activity ? duration : cooldown);

        public override Commandlet Clone()
        {
            throw new NotImplementedException();
        }

        protected override void Deserialize(SerializedCommandWrapper _data) {
            SerializedAdrenalineCommandlet deserialized = (SerializedAdrenalineCommandlet)_data.commandletData;

            Name = _data.Key;
            Commander = TeamCache.Faction(_data.Faction);
            target = deserialized.Status;
        }
    }
}