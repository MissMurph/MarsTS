using MarsTS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.DefaultInputActions;

namespace MarsTS.Teams {
	public class TeamCache : NetworkBehaviour {

		internal static TeamCache instance;

		private Dictionary<int, Team> teamMap;
		private Dictionary<int, Faction> factions;
		private Dictionary<int, int> factionsToTeamsMap;
		private Dictionary<ulong, int> playersToFactionsMap;

		[SerializeField]
		private Faction factionPrefab;

		[SerializeField]
		private Faction dummyObserverPrefab;

		[SerializeField]
		private NetworkObject headquartersPrefab;

		[SerializeField]
		private Transform[] startPositions;

		private EventAgent bus;

		//This will output all Player teams, not the Observer team
		public static List<Team> Teams {
			get {
				List<Team> output = new List<Team>();

				for (int i = 1; i < instance.teamMap.Count; i++) {
					output.Add(instance.teamMap[i]);
				}

				return output;
			}
		}

		public static List<Faction> Players {
			get {
				return instance.factions.Values.ToList();
			}
		}

		public static Team Observer {
			get {
				return instance.teamMap[0];
			}
		}

		private void Awake () {
			instance = this;
			
			bus = GetComponent<EventAgent>();

			teamMap = new Dictionary<int, Team>();
			factions = new Dictionary<int, Faction>();
			factionsToTeamsMap = new Dictionary<int, int>();
			playersToFactionsMap = new Dictionary<ulong, int>();
		}

		public static void Init (ulong[] players) {
			instance.InitTeams(players);
		}

		private void InitTeams (ulong[] players) {
			TeamsInitEvent _event = new TeamsInitEvent(bus);

			_event.Phase = Phase.Pre;

			bus.Global(_event);
			Faction observerFaction = Instantiate(dummyObserverPrefab);

			NetworkObject networkObserver = observerFaction.GetComponent<NetworkObject>();
			networkObserver.Spawn();

			observerFaction.InitClientRpc(0, 0, 0);

			int count = 0;

			foreach (ulong id in players) {
				int factionID = Mathf.RoundToInt(Mathf.Pow(2, count));

				Faction playerFaction = Instantiate(factionPrefab);

				NetworkObject networkFaction = playerFaction.GetComponent<NetworkObject>();
				networkFaction.SpawnWithOwnership(id);

				playerFaction.InitClientRpc(id, factionID, teamMap.Count);

				count++;
			}

			_event.Phase = Phase.Post;
			bus.Global(_event);
		}

		/*[ClientRpc]
		private void ConfigureTeamClientRpc (ulong playerID, int factionID, int teamID) {
			CreateFactionInstance(playerID, factionID, teamID);
		}*/

		public void CreateFactionInstance (ulong playerID, Faction playerFaction, int teamID) {
			Team newTeam = new Team() { Id = teamID, Members = new List<int>() { playerFaction.ID } };

			teamMap[teamID] = newTeam;
			factions[playerFaction.ID] = playerFaction;
			if (playerID > 0) playersToFactionsMap[playerID] = playerFaction.ID;
			factionsToTeamsMap[playerFaction.ID] = newTeam.Id;
		}

		public static void RegisterFaction (ulong playerID, Faction playerFaction, int teamID) {
			instance.CreateFactionInstance(playerID, playerFaction, teamID);
		}

		/*public static int RegisterPlayer (Faction player) {
			int id = Mathf.RoundToInt(Mathf.Pow(2, instance.factions.Count));
			
			if (!instance.factions.TryAdd(id, player)) Debug.LogError("Player " + id + " could not be registered! Check the generated ID!");
			return id;
		}*/

		public static Team Team (Faction player) {
			return instance.teamMap[instance.factionsToTeamsMap[player.ID]];
		}

		public static Faction Faction (int factionID) {
			return instance.factions[factionID];
		}

		/*public static void SetTeams () {
			instance.RollTeams();
		}

		private void RollTeams () {
			foreach (Faction player in factions.Values) {
				int id = teamMap.Count;
				teamMap[id] = new Team { Id = id, Members = new List<Faction>() { player } };
				factionsToTeamsMap[player.ID] = id;
			}
		}*/

		public override void OnDestroy () {
			base.OnDestroy();
			instance = null;
		}
	}

	[Serializable]
	public struct Team {
		public int Id { get; internal set; }
		public List<int> Members;

		public int VisionMask {
			get {
				int output = 0;

				foreach (int id in Members) {
					output |= id;
				}

				return output;
			}
		}
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