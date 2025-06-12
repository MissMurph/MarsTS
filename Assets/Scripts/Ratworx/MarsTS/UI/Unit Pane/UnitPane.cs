using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.UI.Unit_Pane
{
    // TODO: Refactor this to be smarter
    public class UnitPane : MonoBehaviour
    {
        private Dictionary<string, UnitCard> _cardMap;

        [FormerlySerializedAs("cardPrefab")]
        [SerializeField]
        private GameObject _cardPrefab;

        private UnitInfoCard _infoCard;

        private UnitCard _currentPrimary;

        private void Awake() {
            _cardMap = new Dictionary<string, UnitCard>();
            _infoCard = transform.Find("UnitInfo").GetComponent<UnitInfoCard>();
        }

        private void Start() => GameInit.OnSpawnSystems += OnSystemsSpawned;

        private void OnSystemsSpawned() {
            Player.Player.Selection.OnPlayerSelectionChanged += UpdateDisplayedUnits;
            Player.Player.Selection.OnPrimarySelectionChanged += UpdatePrimarySelectedCard;
        }

        private void UpdateDisplayedUnits() {
            ClearSelection();

            if (Player.Player.Selection.Selected.Count <= 0) return;

            if (Player.Player.Selection.SelectedTypes.Count == 1 
                && Player.Player.Selection.SelectedCount == 1)
                foreach (Roster typeEntry in Player.Player.Selection.Selected.Values) {
                    _infoCard.DisplayInfo(typeEntry.GetSelectables()[0]);
                }
            else {
                foreach (Roster typeEntry in Player.Player.Selection.Selected.Values) {
                    UnitCard component = Instantiate(_cardPrefab, transform).GetComponent<UnitCard>();

                    RectTransform rect = component.transform as RectTransform;
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(0, 0);
                    rect.anchoredPosition = new Vector3(45 + 80 * _cardMap.Count, 75, 0);

                    //component.UpdateUnit(UnitRegistry.Prefab(typeEntry.Key).name, typeEntry.Value);
                    component.UpdateUnit(typeEntry.RegistryKey, typeEntry.Count);

                    _cardMap.Add(typeEntry.RegistryKey, component);
                }

                var primaryCard = _cardMap[Player.Player.Selection.PrimarySelection.RegistryKey];
                primaryCard.Selected = true;
                _currentPrimary = primaryCard;
            }
        }

        private void UpdatePrimarySelectedCard() {
            if (_currentPrimary is not null) {
                _currentPrimary.Selected = false;
            }

            if (Player.Player.Selection.PrimarySelection is null) 
                return;
            
            if (_cardMap.TryGetValue(Player.Player.Selection.PrimarySelection.RegistryKey, out UnitCard card)) {
                card.Selected = true;
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