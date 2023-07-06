using MarsTS.Players.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class Faction : MonoBehaviour {

        private Dictionary<string, Roster> ownedUnits;

        public Team Allegiance { get { return PlayerCache.Team(this); } }

        private int minerals;
        private int oil;

		private void Awake () {
            ownedUnits = new Dictionary<string, Roster>();
		}

		public Relationship GetRelationship (Faction other) {
			if (PlayerCache.Team(other).Id == 0) return Relationship.Neutral;
			if (PlayerCache.Team(other).Id.Equals(Allegiance.Id)) return Relationship.Friendly;
			return Relationship.Hostile;
		}
	}
}