using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class UnitPane : MonoBehaviour {

        private Dictionary<Type, UnitCard> cardMap;

		[SerializeField]
		private GameObject cardPrefab;

		private void Awake () {
			cardMap = new Dictionary<Type, UnitCard>();
		}

		public void UpdateUnits (Dictionary<Type, Dictionary<int, ISelectable>> instances) {
			ClearCards();
			int count = 0;

			foreach (KeyValuePair<Type, Dictionary<int, ISelectable>> typeMap in instances) {
				ISelectable unit = null;

				foreach (ISelectable selected in typeMap.Value.Values) {
					unit = selected;
					break;
				}

				UnitCard component = Instantiate(cardPrefab, transform).GetComponent<UnitCard>();

				RectTransform rect = component.transform as RectTransform;
				rect.anchorMin = new Vector2(0, 0);
				rect.anchorMax = new Vector2(0, 0);
				rect.position = new Vector3(45 + (80 * count), 180, 0);

				component.UpdateUnit(unit, instances.Count);
				count++;
			}
        }

		private void ClearCards () {
			foreach (UnitCard card in cardMap.Values) {
				Destroy(card.gameObject);
			 }

			cardMap.Clear();
		}
    }
}