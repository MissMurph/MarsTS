using System;
using System.Collections.Generic;
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

        private Roster _primarySelection;

        private Dictionary<string, Roster> _registryKeysToRosters = new Dictionary<string, Roster>();

        public void SelectUnits(ICollection<ISelectable> toSelect) {
            
        }
    }
}