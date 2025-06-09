using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.UI.Unit_Pane
{
    public class UnitPane : MonoBehaviour
    {
        private Dictionary<string, UnitCard> _cardMap;

        [FormerlySerializedAs("cardPrefab")]
        [SerializeField]
        private GameObject _cardPrefab;

        private UnitInfoCard _infoCard;

        private void Awake() {
            _cardMap = new Dictionary<string, UnitCard>();
            _infoCard = transform.Find("UnitInfo").GetComponent<UnitInfoCard>();
        }

        private void Start() {
            Player.Player.Selection.OnPlayerSelectionChanged += UpdateDisplayedUnits;
            Player.Player.Selection.OnPrimarySelectionChanged += UpdatePrimarySelectedCard;
        }

        private void UpdateDisplayedUnits() {
            
        }

        private void UpdatePrimarySelectedCard() {
            
        }

        public void UpdateUnits(List<Roster> rosters) {
            /*Dictionary<string, int> translation = new();

            foreach (Roster units in rosters.Values) {
                translation.Add(units.RegistryKey, units.Count);
            }

            UpdateUnits(translation);*/

            ClearSelection();

            if (rosters.Count == 1 && rosters[0].Count == 1)
                foreach (Roster typeEntry in rosters) {
                    _infoCard.DisplayInfo(typeEntry.GetFirst());
                }
            else
                foreach (Roster typeEntry in rosters) {
                    UnitCard component = Instantiate(_cardPrefab, transform).GetComponent<UnitCard>();

                    RectTransform rect = component.transform as RectTransform;
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 0);
                    rect.anchoredPosition = new Vector3(45 + 80 * _cardMap.Count, 75, 0);

                    //component.UpdateUnit(UnitRegistry.Prefab(typeEntry.Key).name, typeEntry.Value);
                    component.UpdateUnit(typeEntry.RegistryKey, typeEntry.Count);

                    _cardMap.Add(typeEntry.RegistryKey, component);
                }
        }

        public UnitCard Card(string key) {
            if (_cardMap.TryGetValue(key, out UnitCard output)) return output;

            return null;
        }

        private void ClearSelection() {
            foreach (UnitCard card in _cardMap.Values) {
                Destroy(card.gameObject);
            }

            _infoCard.Deactivate();

            _cardMap.Clear();
        }
    }
}