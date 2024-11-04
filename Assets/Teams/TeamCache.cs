using MarsTS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Teams {
	public class TeamCache : NetworkBehaviour {
		
		private static TeamCache _instance;

		private Dictionary<int, Team> _teamMap;
		private Dictionary<int, Faction> _factions;
		private Dictionary<int, int> _factionsToTeamsMap;
		private Dictionary<ulong, int> _playersToFactionsMap;

		[FormerlySerializedAs("factionPrefab")] [SerializeField]
		private Faction _factionPrefab;

		[FormerlySerializedAs("dummyObserverPrefab")] [SerializeField]
		private Faction _dummyObserverPrefab;

		[FormerlySerializedAs("headquartersPrefab")] [SerializeField]
		private NetworkObject _headquartersPrefab;

		[FormerlySerializedAs("startPositions")] [SerializeField]
		private Transform[] _startPositions;

		private EventAgent _bus;

		//This will output all Player teams, not the Observer team
		public static List<Team> Teams {
			get {
				var output = new List<Team>();

				for (int i = 1; i < _instance._teamMap.Count; i++) {
					output.Add(_instance._teamMap[i]);
				}

				return output;
			}
		}

		public static List<Faction> Players => _instance._factions.Values.ToList();

		public static Team Observer => _instance._teamMap[0];

		private void Awake () {
			_instance = this;
			
			_bus = GetComponentInParent<EventAgent>();

			_teamMap = new Dictionary<int, Team>();
			_factions = new Dictionary<int, Faction>();
			_factionsToTeamsMap = new Dictionary<int, int>();
			_playersToFactionsMap = new Dictionary<ulong, int>();
		}

		public static void Init (ulong[] players) {
			_instance.InitTeams(players);
		}

		private void InitTeams (ulong[] players) {
			TeamsInitEvent @event = new TeamsInitEvent(_bus) {
				Phase = Phase.Pre
			};

			_bus.Global(@event);
			
			SpawnNewFaction(_dummyObserverPrefab, 0);

			foreach (ulong id in players) {
				int factionId = Mathf.RoundToInt(Mathf.Pow(2, _instance._factions.Count));
				Faction faction = SpawnNewFaction(_factionPrefab, factionId);
				_factions[factionId] = faction;
				
				int teamId = _teamMap.Count + 1;
				CreateTeamInstance(teamId);
				AddFactionToTeam(faction.Id, teamId);
				AssignPlayerToFaction(id, faction.Id);
			}

			@event.Phase = Phase.Post;
			_bus.Global(@event);
		}

		private void CreateTeamInstance(int teamId) {
			Team newTeam = new Team(teamId);

			_teamMap[teamId] = newTeam;

			if (NetworkManager.Singleton.IsServer) 
				CreateTeamInstanceClientRpc(teamId);
		}

		[Rpc(SendTo.NotServer)]
		private void CreateTeamInstanceClientRpc(int teamId) => CreateTeamInstance(teamId);

		private void AddFactionToTeam(int factionId, int teamId) {
			if (_factions.TryGetValue(factionId, out Faction faction)
			    && _teamMap.TryGetValue(teamId, out Team team)) {

				if (_factionsToTeamsMap.TryGetValue(factionId, out int currentTeam)) 
					_teamMap[currentTeam].Members.Remove(factionId);
				
				team.Members.Add(factionId);
				_factionsToTeamsMap[factionId] = teamId;
			}
			else
				Debug.LogError($"Unable to add Faction {factionId} to Team {teamId}");

			if (NetworkManager.Singleton.IsServer) 
				AddFactionToTeamClientRpc(factionId, teamId);
		}

		[Rpc(SendTo.NotServer)]
		private void AddFactionToTeamClientRpc(int factionId, int teamId) => AddFactionToTeam(factionId, teamId);

		private static Faction SpawnNewFaction(Faction prefab, int factionId) {
			Faction faction = Instantiate(prefab);

			NetworkObject networkFaction = faction.GetComponent<NetworkObject>();
			networkFaction.Spawn();
			
			faction.SetId(factionId);

			return faction;
		}

		public static int RegisterFaction(Faction faction) {
			if (NetworkManager.Singleton.IsServer) 
				return faction.Id;
			
			return _instance._factions.TryAdd(faction.Id, faction) ? faction.Id : -1;
		}

		private void AssignPlayerToFaction(ulong playerId, int factionId) {
			if (_factions.TryGetValue(factionId, out Faction faction)) {
				_playersToFactionsMap[playerId] = factionId;
			}
			else
				Debug.LogWarning($"Unable to find Faction {factionId} for player {playerId}!");

			if (NetworkManager.Singleton.IsServer) 
				AssignPlayerToFactionClientRpc(playerId, factionId);
		}

		[Rpc(SendTo.NotServer)]
		private void AssignPlayerToFactionClientRpc(ulong playerId, int factionId) =>
			AssignPlayerToFaction(playerId, factionId);

		public static Team Team (Faction player) {
			return _instance._teamMap[_instance._factionsToTeamsMap[player.Id]];
		}

		public static Faction Faction (int factionId) {
			return _instance._factions[factionId];
		}

		public static Faction GetAssignedFaction(ulong playerId) {
			if (_instance._playersToFactionsMap.TryGetValue(playerId, out int factionId))
				return _instance._factions[factionId];
			
			Debug.LogWarning($"Player {playerId} not assigned a faction!");

			return default;
		}

		public override void OnDestroy () {
			_instance = null;
		}
	}

	[Serializable]
	public class Team : IEquatable<Team> {
		public int Id { get;  }
		public List<int> Members = new List<int>();

		public int VisionMask {
			get {
				int output = 0;

				foreach (int id in Members) {
					output |= id;
				}

				return output;
			}
		}

		internal Team(int id) {
			Id = id;
		}

		public bool Equals(Team other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Team)obj);
		}

		public override int GetHashCode() => Id;
	}

	static class RelationshipExtensions {
		public static Color Colour (this Relationship relationship) {
			switch (relationship) {
				case Relationship.Owned: return Color.green;
				case Relationship.Friendly: return Color.blue;
				case Relationship.Hostile: return Color.red;
				default: return Color.yellow;
			}
		}
	}
}