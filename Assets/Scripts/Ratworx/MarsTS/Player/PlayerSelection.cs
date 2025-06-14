using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Player
{
    public class PlayerSelection : MonoBehaviour
    {
        public event Action OnPlayerSelectionChanged;
        public event Action OnPrimarySelectedCommandsStateChanged;
        public event Action OnPrimarySelectedCommandsChanged;
        public event Action OnPrimarySelectionChanged;
        
        public Roster PrimarySelection => _primarySelection;
        public List<string> SelectedTypes => _selectedRegistryKeysToRosters.Keys.ToList();
        public Dictionary<string, Roster> Selected => _selectedRegistryKeysToRosters;
        public int SelectedCount => _selectedRegistryKeysToRosters.Values.Sum(roster => roster.Count);
        public int SelectedTypesCount => _selectedRegistryKeysToRosters.Count;

        private Roster _primarySelection;
        private readonly Dictionary<string, Roster> _selectedRegistryKeysToRosters = new Dictionary<string, Roster>();

        public void SelectUnits(ICollection<ISelectable> toSelect) {
            foreach (ISelectable target in toSelect) {
                Roster units = GetRoster(target.Entity.RegistryKey);

                if (!units.TryAdd(target.Entity)) {
                    units.Remove(target.Entity.Id);
                    target.Select(false);
                    if (units.Count == 0) _selectedRegistryKeysToRosters.Remove(units.RegistryKey);
                }
                else {
                    target.Select(true);
                }
            }

            // Don't want to change primary when adding selections
            if (_primarySelection is null) {
                SetPrimarySelection(_selectedRegistryKeysToRosters.First().Value);
            }

            EventBus.Post(new PlayerSelectEvent(Selected));
            OnPlayerSelectionChanged?.Invoke();
        }

        public void SelectUnits(ISelectable unit) => SelectUnits(new[] { unit });
        
        public void ClearSelection () {
            foreach (Roster units in _selectedRegistryKeysToRosters.Values) {
                foreach (Entity unit in units.List()) {
                    unit.GetEntityComponent<ISelectable>().Select(false);
                }

                units.Clear();
            }

            _selectedRegistryKeysToRosters.Clear();
            _primarySelection = null;
            
            EventBus.Post(new PlayerSelectEvent(Selected));
            OnPlayerSelectionChanged?.Invoke();
            OnPrimarySelectedCommandsStateChanged?.Invoke();
            OnPrimarySelectedCommandsChanged?.Invoke();
            OnPrimarySelectionChanged?.Invoke();
        }

        public void SetPrimarySelection(Roster roster) {
            if (_primarySelection is not null) {
                foreach (ICommandable commandable in roster.GetCommandables()) {
                    commandable.OnCommandsStateChanged -= OnPrimarySelectedCommandStateChanged;
                }
            }

            _primarySelection = roster;

            foreach (ICommandable commandable in roster.GetCommandables()) {
                commandable.OnCommandsStateChanged += OnPrimarySelectedCommandStateChanged;
                commandable.OnCommandListChanged += OnPrimarySelectedCommandsListChanged;
            }

            OnPrimarySelectedCommandsChanged?.Invoke();
            OnPrimarySelectionChanged?.Invoke();
            OnPrimarySelectedCommandsStateChanged?.Invoke();
        }

        private Roster GetRoster (string key) {
            Roster map = _selectedRegistryKeysToRosters.GetValueOrDefault(key, new Roster());
            _selectedRegistryKeysToRosters.TryAdd(key, map);
            return map;
        }

        private void OnPrimarySelectedCommandsListChanged() => OnPrimarySelectedCommandsChanged?.Invoke();
        private void OnPrimarySelectedCommandStateChanged() => OnPrimarySelectedCommandsStateChanged?.Invoke();
    }
}