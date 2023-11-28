using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Rendering.CameraUI;

namespace MarsTS.Players {

    public class SelectionCollider : MonoBehaviour {

		private List<ISelectable> hitUnits = new List<ISelectable>();
		private List<ISelectable> hitUnitsLastFrame = new List<ISelectable>();
		private List<ISelectable> newlyHitUnits = new List<ISelectable>();

		private void FixedUpdate () {
			newlyHitUnits = new List<ISelectable>();
		}

		private void OnTriggerStay (Collider other) {
			if (EntityCache.TryGet(other.transform.parent.name, out ISelectable target)) {
				newlyHitUnits.Add(target);
				target.Hover(true);
			}
		}

		private void Update () {
			foreach (ISelectable unit in hitUnitsLastFrame) {
				if (!newlyHitUnits.Contains(unit)) 
					unit.Hover(false);

			}

			//Debug.Log("Last Frame: " + hitUnitsLastFrame.Count + " This Frame: " + newlyHitUnits.Count);

			hitUnitsLastFrame = newlyHitUnits;
			hitUnits = newlyHitUnits;

			
		}

		private void OnDestroy () {
			Dictionary<Relationship, List<ISelectable>> factionMap = new Dictionary<Relationship, List<ISelectable>>();

			foreach (ISelectable hit in hitUnits) {
				Relationship relation = hit.GetRelationship(Player.Main);
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