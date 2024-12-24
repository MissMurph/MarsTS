using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.World;
using UnityEngine;

namespace MarsTS.Commands
{
    public class HarvestableCommandlet : Commandlet<IHarvestable>
    {
        public override string Key => Name;
        
        [SerializeField] private GameObject _targetGameObj;
        
        public override Commandlet Clone() => throw new System.NotImplementedException();

        protected override void Deserialize(SerializedCommandWrapper data)
        {
            SerializedHarvestableCommandlet deserialized = (SerializedHarvestableCommandlet)data.commandletData;

            Name = data.Key;
            Commander = TeamCache.Faction(data.Faction);
            EntityCache.TryGet(deserialized.TargetUnit, out IHarvestable unit);
            target = unit;
            
            _targetGameObj = target.GameObject;
        }
    }
}