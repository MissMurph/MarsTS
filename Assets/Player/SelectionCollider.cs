using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.Players {

    public class SelectionCollider : MonoBehaviour {

		public List<ISelectable> hitUnits = new List<ISelectable>();

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.parent.name, out ISelectable target)) hitUnits.Add(target);
		}

		private void OnDestroy () {
			Dictionary<Relationship, List<ISelectable>> factionMap = new Dictionary<Relationship, List<ISelectable>>();

			foreach (ISelectable hit in hitUnits) {
				Relationship relation = hit.GetRelationship(Player.Main);
				List<ISelectable> rollup = factionMap.GetValueOrDefault(relation, new List<ISelectable>());
				if (!factionMap.ContainsKey(relation)) factionMap[relation] = rollup;
				rollup.Add(hit);
			}

			List<ISelectable> outPut = new();

			foreach (Relationship relation in factionMap.Keys) {
				if (factionMap[relation].Count > outPut.Count) outPut = factionMap[relation];
			}

			if (factionMap.ContainsKey(Relationship.Owned)) outPut = factionMap[Relationship.Owned];

			Player.Main.SelectUnit(outPut.ToArray());
		}
	}
}