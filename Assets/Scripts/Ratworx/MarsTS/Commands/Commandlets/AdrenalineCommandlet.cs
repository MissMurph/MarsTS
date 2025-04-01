using System;
using MarsTS.Events;
using MarsTS.Teams;
using UnityEngine;

namespace MarsTS.Commands {
    public class AdrenalineCommandlet : Commandlet<bool> {
        [SerializeField]
        private float duration;

        [SerializeField]
        private float cooldown;

        public override string SerializerKey => "adrenaline";

        public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) 
            => queue.Cooldown(this, _event.Activity ? duration : cooldown);

        public override Commandlet Clone()
        {
            throw new NotImplementedException();
        }

        protected override void Deserialize(SerializedCommandWrapper _data) {
            SerializedBoolCommandlet deserialized = (SerializedBoolCommandlet)_data.commandletData;

            Name = _data.Name;
            Commander = TeamCache.Faction(_data.Faction);
            _target = deserialized.Status;
        }
    }
}