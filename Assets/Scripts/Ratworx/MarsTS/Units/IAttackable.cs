using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public interface IAttackable : IUnit {
		int Health { get; }
		int MaxHealth { get; }
		void Attack (int damage);
		Relationship GetRelationship (Faction player);
	}
}