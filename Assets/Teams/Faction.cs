using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Teams {

    public class Faction : MonoBehaviour {

        private Dictionary<string, Roster> ownedUnits;

        public Team Allegiance { get { return TeamCache.Team(this); } }

		public int Mask { get; private set; }

		private int id;

		public List<PlayerResource> Resources {
			get {
				List<PlayerResource> output = new List<PlayerResource>();

				foreach (PlayerResource registered in resources.Values) {
					output.Add(registered);
				}

				return output;
			}
		}

		private Dictionary<string, PlayerResource> resources;

		protected virtual void Awake () {
            ownedUnits = new Dictionary<string, Roster>();
			resources = new Dictionary<string, PlayerResource>();

			foreach (PlayerResource toRegister in GetComponents<PlayerResource>()) {
				resources[toRegister.Key] = toRegister;
			}
		}

		protected virtual void Start () {
			id = TeamCache.RegisterPlayer(this);
		}

		public PlayerResource Resource (string key) {
			if (resources.TryGetValue(key, out PlayerResource bank)) {
				return bank;
			}

			Debug.LogWarning("Player Resource with key: " + key + " not found on Player " + name);
			return null;
		}

		public Relationship GetRelationship (Faction other) {
			if (other.name == gameObject.name) return Relationship.Owned;
			if (TeamCache.Team(other).Id == 0) return Relationship.Neutral;
			if (TeamCache.Team(other).Id == Allegiance.Id) return Relationship.Friendly;
			return Relationship.Hostile;
		}
	}
}