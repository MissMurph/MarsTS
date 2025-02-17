using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Teams;
using UnityEngine;

namespace MarsTS.Commands
{
    public class DepositableCommandlet : Commandlet<IDepositable>
    {
        [SerializeField]
        private GameObject _targetGameObj;

        public override string SerializerKey => "deposit";

        public override Commandlet Clone() => throw new System.NotImplementedException();

        protected override void Deserialize(SerializedCommandWrapper data)
        {
            SerializedDepositableCommandlet deserialized = (SerializedDepositableCommandlet)data.commandletData;

            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            EntityCache.TryGet(deserialized.TargetUnit, out IDepositable unit);
            _target = unit;

            _targetGameObj = _target.GameObject;
        }
    }
}