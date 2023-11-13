using MarsTS.Commands;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {
	public interface ISelectable {
		void Enqueue (Commandlet order);
		void Execute (Commandlet order);
		GameObject GameObject { get; }
		string[] Commands ();
		void Select (bool status);
		void Hover (bool status);
		int ID { get; }
		string UnitType { get; }
		Relationship GetRelationship (Faction player);
		bool SetOwner (Faction player);
		int Health { get; }
		int MaxHealth { get; }
		void Attack (int damage);
		string RegistryKey { get; }
	}
}