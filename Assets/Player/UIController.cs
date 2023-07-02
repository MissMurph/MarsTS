using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.Units.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.UI {

	public class UIController : MonoBehaviour {

		private CommandPanel commandPanel;
		private UnitPane unitPane;

		private Dictionary<string, List<string>> commandProfiles;
		private string[] profileIndex;

		private int primaryIndex;

		private string PrimarySelected {
			get {
				return primarySelected;
			}
			set {
				if (primarySelected != null) unitPane.Card(primarySelected).Selected = false;

				primarySelected = value;

				if (primarySelected != null) {
					unitPane.Card(primarySelected).Selected = true;
					commandPanel.UpdateCommands(commandProfiles[primarySelected].ToArray());
				}
				else commandPanel.UpdateCommands(new List<string> { }.ToArray());
			}
		}

		private string primarySelected;

		private void Awake () {
			commandProfiles = new();
		}

		private void Start () {
			commandPanel = GameObject.Find("Command Zone").GetComponent<CommandPanel>();
			unitPane = GameObject.Find("Unit Pane").GetComponent<UnitPane>();
			EventBus.AddListener<SelectEvent>(OnSelection);
		}

		private void OnSelection (SelectEvent _event) {
			commandProfiles.Clear();
			profileIndex = new string[_event.Selected.Count];
			int index = 0;
			PrimarySelected = null;

			unitPane.UpdateUnits(_event.Selected);
			
			foreach (KeyValuePair<string, Roster> entry in _event.Selected) {
				List<string> availableCommands = new List<string>(entry.Value.Commands);
				commandProfiles.Add(entry.Key, availableCommands);
				profileIndex[index] = entry.Key;
				index++;
			}

			if (profileIndex.Length > 0) PrimarySelected = profileIndex[0];
			else PrimarySelected = null;
			primaryIndex = 0;
		}

		public void Next (InputAction.CallbackContext context) {
			if (context.performed && profileIndex.Length > 0) {
				int newIndex = primaryIndex >= profileIndex.Length - 1 ? 0 : primaryIndex + 1;
				if (profileIndex[newIndex] != null) {
					PrimarySelected = profileIndex[newIndex];
					primaryIndex = newIndex;
				}
			}
		}
	}
}