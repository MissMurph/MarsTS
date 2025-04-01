using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Units {
	public interface ISelectable : IUnit {
		int Id { get; }
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