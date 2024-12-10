using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Commands {

	public class AttackableCommandlet : Commandlet<IAttackable> {

		public override string Key => Name;

		[SerializeField]
		private GameObject targetGameObj;

		public override Commandlet Clone()
		{
			throw new System.NotImplementedException();
		}

		protected override void Deserialize (SerializedCommandWrapper data) {
			SerializedAttackableCommandlet deserialized = (SerializedAttackableCommandlet)data.commandletData;
			
			Name = data.Key;
			Commander = TeamCache.Faction(data.Faction);
			EntityCache.TryGet(deserialized.TargetUnit, out IAttackable unit);
			target = unit;
		}
	}
}