using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Teams;
using UnityEngine;

namespace MarsTS.Commands
{
    public class DepositableCommandlet : Commandlet<IDepositable>
    {
        public override string Key => Name;

        [SerializeField] private GameObject _targetGameObj;
        
        public override Commandlet Clone() => throw new System.NotImplementedException();

        protected override void Deserialize(SerializedCommandWrapper data)
        {
            SerializedDepositableCommandlet deserialized = (SerializedDepositableCommandlet)data.commandletData;

            Name = data.Key;
            Commander = TeamCache.Faction(data.Faction);
            EntityCache.TryGet(deserialized.TargetUnit, out IDepositable unit);
            target = unit;

            _targetGameObj = target.GameObject;
        }
    }
}