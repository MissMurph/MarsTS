using System;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets
{
    public class AttackableCommandlet : Commandlet<IAttackable>
    {
        [SerializeField] private GameObject _targetGameObj;

        public override Commandlet Clone() => throw new NotImplementedException();

        public override string SerializerKey => "attack";

        protected override void Deserialize(SerializedCommandWrapper data)
        {
            SerializedAttackableCommandlet deserialized = (SerializedAttackableCommandlet)data.commandletData;

            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            EntityCache.TryGetEntityComponent(deserialized.TargetUnit, out IAttackable unit);
            _target = unit;
            
            _targetGameObj = _target.GameObject;
        }
    }
}