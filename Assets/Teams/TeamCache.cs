using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MarsTS.Teams {
	public class TeamCache : MonoBehaviour {

		internal static TeamCache instance;

		[SerializeField]
		private Team[] startingTeams;

		[SerializeField]
		private Faction[] startingObservers;

		[SerializeField]
		internal Material ownedMaterial;
		[SerializeField]
		internal Material friendlyMaterial;
		[SerializeField]
		internal Material neutralMaterial;
		[SerializeField]
		internal Material enemyMaterial;

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

		private Dictionary<int, Team> teamMap;
		private Dictionary<Faction, int> playerMap;
		private Dictionary<int, Faction> players;

		public static Team Observer {
			get {
				return instance.teamMap[0];
			}
		}

		private void Awake () {
			instance = this;
			playerMap = new Dictionary<Faction, int>();
			teamMap = new Dictionary<int, Team>();
			players = new Dictionary<int, Faction>();
			teamMap[0] = new Team { Id = 0, Members = new List<Faction>() };
			teamMap[0].Members.AddRange(startingObservers);

			foreach (Team team in startingTeams) {
				team.Id = teamMap.Count;
				teamMap[team.Id] = team;

				foreach (Faction member in team.Members) {
					playerMap[member] = team.Id;
				}
			}
		}

		public static int RegisterPlayer (Faction player) {
			int id = 2 ^ instance.players.Count;
			if (!instance.players.TryAdd(id, player)) Debug.LogError("Player " + id + " could not be registered! Check the generated ID!");
			return id;
		}

		public static Team Team (Faction player) {
			return instance.teamMap[instance.playerMap[player]];
		}

		private void OnDestroy () {
			instance = null;
		}
	}

	[Serializable]
	public class Team {
		public int Id { get; internal set; }
		public List<Faction> Members;

		public int VisionMask {
			get {
				int output = 0;

				foreach (Faction player in Members) {
					output |= player.ID;
				}

				return output;
			}
		}
	}

	static class RelationshipExtensions {
		public static Material Material (this Relationship relationship) {
			switch (relationship) {
				case Relationship.Owned: return TeamCache.instance.ownedMaterial;
				case Relationship.Friendly: return TeamCache.instance.friendlyMaterial;
				case Relationship.Hostile: return TeamCache.instance.enemyMaterial;
				default: return TeamCache.instance.neutralMaterial;
			}
		}

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