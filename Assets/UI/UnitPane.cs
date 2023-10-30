using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class UnitPane : MonoBehaviour {

        private Dictionary<string, UnitCard> cardMap;

		[SerializeField]
		private GameObject cardPrefab;

		private void Awake () {
			cardMap = new Dictionary<string, UnitCard>();
		}

		public void UpdateUnits (Dictionary<string, int> instances) {
			ClearCards();

			foreach (KeyValuePair<string, int> typeEntry in instances) {
				UnitCard component = Instantiate(cardPrefab, transform).GetComponent<UnitCard>();

				RectTransform rect = component.transform as RectTransform;
				rect.anchorMin = new Vector2(0, 0);
				rect.anchorMax = new Vector2(0, 0);
				rect.position = new Vector3(45 + (80 * cardMap.Count), 180, 0);

				component.UpdateUnit(UnitRegistry.Prefab(typeEntry.Key).name, typeEntry.Value);
				cardMap.Add(typeEntry.Key, component);
			}
        }

		public void UpdateUnits (Dictionary<string, Roster> rosters) {
			Dictionary<string, int> translation = new();

			foreach (Roster units in rosters.Values) {
				translation.Add(units.Type, units.Count);
			}

			UpdateUnits(translation);
		}

		public UnitCard Card (string key) {
			if (cardMap.TryGetValue(key, out UnitCard output)) {
				return output;
			}

			return null;
		}

		private void ClearCards () {
			foreach (UnitCard card in cardMap.Values) {
				Destroy(card.gameObject);
			 }

			cardMap.Clear();
		}
    }
}