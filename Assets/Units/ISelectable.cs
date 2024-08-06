using MarsTS.Commands;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {
	public interface ISelectable : IUnit {
		int ID { get; }
		string UnitType { get; }
		string RegistryKey { get; }
		Faction Owner { get; }
		void Select (bool status);
		void Hover (bool status);
		Sprite Icon { get; }
		Relationship GetRelationship (Faction player);
		bool SetOwner (Faction player);
	}
}