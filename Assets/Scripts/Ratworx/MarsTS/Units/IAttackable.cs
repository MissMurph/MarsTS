using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Units {

    public interface IAttackable : IUnitInterface {
		int Health { get; }
		int MaxHealth { get; }
		void Attack (int damage);
		Relationship GetRelationship (Faction player);
	}
}