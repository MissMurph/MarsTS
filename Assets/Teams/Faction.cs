using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Teams {

    public class Faction : MonoBehaviour {

        private Dictionary<string, Roster> ownedUnits;

        public Team Allegiance { get { return TeamCache.Team(this); } }

        private int minerals;
        private int oil;

		private void Awake () {
            ownedUnits = new Dictionary<string, Roster>();
		}

		public Relationship GetRelationship (Faction other) {
			if (other.name == gameObject.name) return Relationship.Owned;
			if (TeamCache.Team(other).Id == 0) return Relationship.Neutral;
			if (TeamCache.Team(other).Id == Allegiance.Id) return Relationship.Friendly;
			return Relationship.Hostile;
		}
	}
}