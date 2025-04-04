using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Player {

    public class SelectionCollider : MonoBehaviour {

		private Dictionary<string, ISelectable> hitUnits = new Dictionary<string, ISelectable>();
		private Dictionary<string, ISelectable> hitUnitsLastFrame = new Dictionary<string, ISelectable>();
		private Dictionary<string, ISelectable> newlyHitUnits = new Dictionary<string, ISelectable>();

		private void FixedUpdate () {
			newlyHitUnits = new Dictionary<string, ISelectable>();
		}

		private void OnTriggerStay (Collider other) {
			if (EntityCache.TryGetEntityComponent(other.transform.root.name, out ISelectable target)) {
				newlyHitUnits[other.transform.root.name] = target;
				target.Hover(true);
			}
		}

		private void Update () {
			foreach (ISelectable unit in hitUnitsLastFrame.Values) {
				if (!newlyHitUnits.ContainsKey(unit.GameObject.name)) 
					unit.Hover(false);
			}

			//Debug.Log("Last Frame: " + hitUnitsLastFrame.Count + " This Frame: " + newlyHitUnits.Count);

			hitUnitsLastFrame = newlyHitUnits;
			hitUnits = newlyHitUnits;

			
		}

		private void OnDestroy () {
			Dictionary<Relationship, List<ISelectable>> factionMap = new Dictionary<Relationship, List<ISelectable>>();

			foreach (ISelectable hit in hitUnits.Values) {
				Relationship relation = hit.GetRelationship(Player.Commander);
				List<ISelectable> rollup = factionMap.GetValueOrDefault(relation, new List<ISelectable>());
				if (!factionMap.ContainsKey(relation)) factionMap[relation] = rollup;
				rollup.Add(hit);
				hit.Hover(false);
			}

			List<ISelectable> factionList = new();

			foreach (Relationship relation in factionMap.Keys) {
				if (factionMap[relation].Count > factionList.Count) factionList = factionMap[relation];
			}

			if (factionMap.ContainsKey(Relationship.Owned)) factionList = factionMap[Relationship.Owned];

			Dictionary<string, List<ISelectable>> typeDict = new Dictionary<string, List<ISelectable>>();

			foreach (ISelectable unit in factionList) {
				string[] splitKey = unit.RegistryKey.Split(':');
				List<ISelectable> typeList = typeDict.GetValueOrDefault(splitKey[0], new List<ISelectable>());
				if (!typeDict.ContainsKey(splitKey[0])) typeDict[splitKey[0]] = typeList;
				typeList.Add(unit);
			}

			if (typeDict.TryGetValue("unit", out List<ISelectable> unitOutput)) Player.Main.SelectUnit(unitOutput.ToArray());
			else if (typeDict.TryGetValue("building", out List<ISelectable> buildingOutput)) Player.Main.SelectUnit(buildingOutput.ToArray());



			//Player.Main.SelectUnit(outPut.ToArray());
		}
	}
}