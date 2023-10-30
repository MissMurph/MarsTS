using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class SelectionCollider : MonoBehaviour {

		public List<Unit> hitUnits = new List<Unit>();

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.parent.name, out Unit target)) hitUnits.Add(target);
		}

		private void OnDestroy () {
			Dictionary<Faction, List<Unit>> factionMap = new Dictionary<Faction, List<Unit>>();

			foreach (Unit hit in hitUnits) {
				List<Unit> rollup = factionMap.GetValueOrDefault(hit.Owner, new List<Unit>());
				if (!factionMap.ContainsKey(hit.Owner)) factionMap[hit.Owner] = rollup;
				rollup.Add(hit);
			}

			List<Unit> outPut = new();

			foreach (Faction player in factionMap.Keys) {
				if (factionMap[player].Count > outPut.Count) outPut = factionMap[player];
			}

			if (factionMap.ContainsKey(Player.Main)) outPut = factionMap[Player.Main];

			Player.Main.SelectUnit(outPut.ToArray());
		}
	}
}