using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace MarsTS.UI {

    public class UnitPane : MonoBehaviour {

        private Dictionary<string, UnitCard> cardMap;

		[SerializeField]
		private GameObject cardPrefab;

		private EventAgent bus;

		private void Awake () {
			cardMap = new Dictionary<string, UnitCard>();
			bus = GetComponent<EventAgent>();
		}

		public void UpdateUnits (Dictionary<string, Roster> rosters) {
			/*Dictionary<string, int> translation = new();

			foreach (Roster units in rosters.Values) {
				translation.Add(units.RegistryKey, units.Count);
			}

			UpdateUnits(translation);*/

			ClearCards();

			if (rosters.Count == 1) {
				foreach (KeyValuePair<string, Roster> typeEntry in rosters) {
					if (typeEntry.Value.Count == 1) {
						UnitInfoEvent _event = new UnitInfoEvent(bus, typeEntry.Value.Get());
						bus.Global(_event);
					}
				}
			}
			//else {
				foreach (KeyValuePair<string, Roster> typeEntry in rosters) {
					UnitCard component = Instantiate(cardPrefab, transform).GetComponent<UnitCard>();

					RectTransform rect = component.transform as RectTransform;
					rect.anchorMin = new Vector2(0, 0);
					rect.anchorMax = new Vector2(0, 0);
					rect.anchoredPosition = new Vector3(45 + (80 * cardMap.Count), 180, 0);

					//component.UpdateUnit(UnitRegistry.Prefab(typeEntry.Key).name, typeEntry.Value);

					component.UpdateUnit(typeEntry.Key, typeEntry.Value.Count);

					cardMap.Add(typeEntry.Key, component);
				}
			//}
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