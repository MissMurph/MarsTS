using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Units {
	public interface ISelectable : IUnitInterface {
		Faction Owner { get; }
		void Select (bool status);
		void Hover (bool status);
		Sprite Icon { get; }
		Relationship GetRelationship(Faction other);
	}
}