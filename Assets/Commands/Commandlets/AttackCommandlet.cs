using MarsTS.Entities;
using MarsTS.Networking;
using MarsTS.Teams;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Commands {

	public class AttackCommandlet : Commandlet<IAttackable> {

		public override string Key => Name;

		[SerializeField]
		private GameObject targetGameObj;

		public override Commandlet Clone()
		{
			throw new System.NotImplementedException();
		}

		protected override void Deserialize (SerializedCommandWrapper _data) {
			SerializedAttackCommandlet deserialized = (SerializedAttackCommandlet)_data.commandletData;

			Name = _data.Key;
			Commander = TeamCache.Faction(_data.Faction);
			EntityCache.TryGet(deserialized.targetUnit.GameObject().name, out IAttackable unit);
			target = unit;
		}
	}
}