using System.Collections.Generic;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Pane {

    public class UnitPane : MonoBehaviour {

        private Dictionary<string, UnitCard> cardMap;

		[SerializeField]
		private GameObject cardPrefab;

		private UnitInfoCard infoCard;

		private void Awake () {
			cardMap = new Dictionary<string, UnitCard>();
			infoCard = transform.Find("UnitInfo").GetComponent<UnitInfoCard>();
		}

		public void UpdateUnits (List<Roster> rosters) {
			/*Dictionary<string, int> translation = new();

			foreach (Roster units in rosters.Values) {
				translation.Add(units.RegistryKey, units.Count);
			}

			UpdateUnits(translation);*/

			ClearSelection();

			if (rosters.Count == 1 && rosters[0].Count == 1) {
				foreach (Roster typeEntry in rosters) {
					infoCard.DisplayInfo(typeEntry.Get());
				}
			}
			else {
				foreach (Roster typeEntry in rosters) {
					UnitCard component = Instantiate(cardPrefab, transform).GetComponent<UnitCard>();

					RectTransform rect = component.transform as RectTransform;
					rect.anchorMin = new Vector2(0, 0);
					rect.anchorMax = new Vector2(0, 0);
					rect.anchoredPosition = new Vector3(45 + (80 * cardMap.Count), 75, 0);

					//component.UpdateUnit(UnitRegistry.Prefab(typeEntry.Key).name, typeEntry.Value);

					component.UpdateUnit(typeEntry.RegistryKey, typeEntry.Count);

					cardMap.Add(typeEntry.RegistryKey, component);
				}
			}
		}

		public UnitCard Card (string key) {
			if (cardMap.TryGetValue(key, out UnitCard output)) {
				return output;
			}

			return null;
		}

		private void ClearSelection () {
			foreach (UnitCard card in cardMap.Values) {
				Destroy(card.gameObject);
			 }

			infoCard.Deactivate();

			cardMap.Clear();
		}
    }
}