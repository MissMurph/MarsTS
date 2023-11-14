using MarsTS.Commands;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {
	public interface ISelectable {
		GameObject GameObject { get; }
		void Select (bool status);
		void Hover (bool status);
		int ID { get; }
		string UnitType { get; }
		Relationship GetRelationship (Faction player);
		bool SetOwner (Faction player);
		string RegistryKey { get; }
	}
}