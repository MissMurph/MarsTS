using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets
{
    public class HarvestableCommandlet : Commandlet<IHarvestable>
    {
        [SerializeField] private GameObject _targetGameObj;

        public override string SerializerKey => "harvest";

        public override Commandlet Clone() => throw new System.NotImplementedException();

        protected override void Deserialize(SerializedCommandWrapper data)
        {
            SerializedHarvestableCommandlet deserialized = (SerializedHarvestableCommandlet)data.commandletData;

            Name = data.Key;
            Commander = TeamCache.Faction(data.Faction);
            EntityCache.TryGetEntityComponent(deserialized.TargetUnit, out IHarvestable unit);
            _target = unit;
            
            _targetGameObj = _target.GameObject;
        }
    }
}