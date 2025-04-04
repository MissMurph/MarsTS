using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets
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
            EntityCache.TryGetEntityComponent(deserialized.TargetUnit, out IDepositable unit);
            _target = unit;

            _targetGameObj = _target.GameObject;
        }
    }
}