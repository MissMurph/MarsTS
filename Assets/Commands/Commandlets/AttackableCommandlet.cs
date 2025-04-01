using System;
using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Commands
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
            EntityCache.TryGet(deserialized.TargetUnit, out IAttackable unit);
            _target = unit;
            
            _targetGameObj = _target.GameObject;
        }
    }
}