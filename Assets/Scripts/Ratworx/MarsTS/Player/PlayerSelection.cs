using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Player
{
    public class PlayerSelection
    {
        public event Action OnPlayerSelectionChanged;
        public event Action OnPrimarySelectedCommandsStateChanged;
        public event Action OnPrimarySelectedCommandsChanged;
        public event Action OnPrimarySelectionChanged;
        public Roster PrimarySelection => _primarySelection;
        public List<string> SelectedTypes => _registryKeysToRosters.Keys.ToList();
        public Dictionary<string, Roster> Selected => _registryKeysToRosters;
        public int SelectedCount => _registryKeysToRosters.Values.Sum(roster => roster.Count);

        private Roster _primarySelection;

        private Dictionary<string, Roster> _registryKeysToRosters = new Dictionary<string, Roster>();

        public void SelectUnits(ICollection<ISelectable> toSelect) {
            
        }
    }
}