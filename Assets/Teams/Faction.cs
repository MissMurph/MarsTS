using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.Research;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Teams {

    public class Faction : NetworkBehaviour {

        private Dictionary<string, Roster> ownedUnits;

        public Team Allegiance { get { return TeamCache.Team(this); } }

		public int VisionMask { get { return Allegiance.VisionMask; } }

		public int ID { get { return id; } }

		[SerializeField]
		private int id = -1;

		public bool IsHuman { get { return true; } }

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

		protected Dictionary<string, Technology> research;

		protected virtual void Awake () {
            ownedUnits = new Dictionary<string, Roster>();
			resources = new Dictionary<string, PlayerResource>();
			research = new Dictionary<string, Technology>();

			foreach (PlayerResource toRegister in GetComponents<PlayerResource>()) {
				resources[toRegister.Key] = toRegister;
			}

			foreach (Technology startingTech in GetComponentsInChildren<Technology>()) {
				research[startingTech.key] = startingTech;
			}
		}

		[Rpc(SendTo.NotServer)]
		public void InitClientRpc (ulong playerID, int factionID, int teamID) {
			InitClient(playerID, factionID, teamID);
		}

		public void InitClient (ulong playerID, int factionID, int teamID) {
			//Debug.Log("called rpc");

			id = factionID;

			TeamCache.RegisterFaction(playerID, this, teamID);
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

		public bool IsResearched (string key) {
			return research.ContainsKey(key);
		}

		public void SubmitResearch (Technology product) {
			research[product.key] = product;
		}
	}
}