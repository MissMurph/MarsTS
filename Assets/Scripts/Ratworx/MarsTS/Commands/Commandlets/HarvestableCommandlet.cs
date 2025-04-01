using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.World;
using UnityEngine;

namespace MarsTS.Commands
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
            EntityCache.TryGet(deserialized.TargetUnit, out IHarvestable unit);
            _target = unit;
            
            _targetGameObj = _target.GameObject;
        }
    }
}