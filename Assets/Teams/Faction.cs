using System;
using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.Research;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Teams {

    public class Faction : NetworkBehaviour, IEquatable<Faction> {

        private Dictionary<string, Roster> _ownedUnits;

        public Team Allegiance => TeamCache.Team(this);

        public int VisionMask => Allegiance.VisionMask;

		public int Id => _id;

		[FormerlySerializedAs("id")] [SerializeField]
		private int _id = -1;

		public bool IsHuman => true;

		public List<PlayerResource> Resources {
			get {
				var output = new List<PlayerResource>();

				foreach (PlayerResource registered in _resources.Values) 
					output.Add(registered);

				return output;
			}
		}

		private Dictionary<string, PlayerResource> _resources;

		private Dictionary<string, Technology> _research;

		private void Awake () {
            _ownedUnits = new Dictionary<string, Roster>();
			_resources = new Dictionary<string, PlayerResource>();
			_research = new Dictionary<string, Technology>();

			foreach (PlayerResource toRegister in GetComponents<PlayerResource>()) {
				_resources[toRegister.Key] = toRegister;
			}

			foreach (Technology startingTech in GetComponentsInChildren<Technology>()) {
				_research[startingTech.key] = startingTech;
			}
		}

		public void SetId(int factionId) {
			_id = factionId;
			name = $"{typeof(Faction)}:{Id}";
			SynchronizeClientRpc(Id);
		}

		[Rpc(SendTo.NotServer)]
		private void SynchronizeClientRpc(int id)
		{
			_id = id;
			name = $"{typeof(Faction)}:{Id}";
			TeamCache.RegisterFaction(this);
		}

		public PlayerResource GetResource (string key) {
			if (_resources.TryGetValue(key, out PlayerResource bank)) {
				return bank;
			}

			Debug.LogWarning("Player Resource with key: " + key + " not found on Player " + name);
			return null;
		}

		public Relationship GetRelationship (Faction other) {
			if (other == this) return Relationship.Owned;
			if (TeamCache.Team(other).Id == 0) return Relationship.Neutral;
			if (TeamCache.Team(other).Id == Allegiance.Id) return Relationship.Friendly;
			return Relationship.Hostile;
		}

		public bool IsResearched (string key) {
			return _research.ContainsKey(key);
		}

		public void SubmitResearch (Technology product) {
			_research[product.key] = product;
		}

		public bool Equals(Faction other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return base.Equals(other) && _id == other._id;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Faction)obj);
		}

		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _id);
    }
}