using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players.Teams {

	public class PlayerCache : MonoBehaviour {

		private static PlayerCache instance;

		[SerializeField]
		private Team[] startingTeams;

		[SerializeField]
		private Faction[] startingObservers;

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

		public static Team Observer {
			get {
				return instance.teamMap[0];
			}
		}

		private void Awake () {
			instance = this;
			playerMap = new Dictionary<Faction, int>();
			teamMap = new Dictionary<int, Team>();
			teamMap[0] = new Team { Id = 0, Members = new List<Faction>() };
			teamMap[0].Members.AddRange(startingObservers);

			foreach (Team team in startingTeams) {
				team.Id = teamMap.Count;
				teamMap[team.Id] = team;

				foreach (Player member in team.Members) {
					playerMap[member] = team.Id;
				}
			}
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
	}
}