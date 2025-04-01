using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitHurtEvent : SelectableEvent {

		public IAttackable Targetable { get; private set; }
		public int Damage { get; private set; }

		public UnitHurtEvent (EventAgent source, IAttackable unit, int damage) : base("Hurt", source, unit as ISelectable) {
			Targetable = unit;
			Damage = damage;
		}

		public void SetDamage(int newDamage)
		{
			Damage = newDamage;
		}
	}
}