using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets {
    public class AdrenalineCommandlet : Commandlet<bool> {
        [SerializeField]
        private float duration;

        [SerializeField]
        private float cooldown;

        public override string SerializerKey => "adrenaline";

        public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) 
            => queue.Cooldown(this, _event.Activity ? duration : cooldown);

        protected override void Deserialize(SerializedCommandWrapper data) {
            SerializedBoolCommandlet deserialized = (SerializedBoolCommandlet)data.commandletData;

            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            _target = deserialized.Status;
        }
    }
}