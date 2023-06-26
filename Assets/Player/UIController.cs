using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

	public class UIController : MonoBehaviour {

		private CommandPanel commandPanel;
		private UnitPane unitPane;

		private void Start () {
			commandPanel = GameObject.Find("Command Zone").GetComponent<CommandPanel>();
			unitPane = GameObject.Find("Unit Pane").GetComponent<UnitPane>();
			EventBus.AddListener<SelectEvent>(OnSelection);
		}

		private void OnSelection (SelectEvent _event) {
			unitPane.UpdateUnits(_event.Selected);

			//if (_event.Selected.Count > 0) unitCard.UpdateUnit(_event.Selected[0]);

			List<string> availableCommands = new List<string>();

			foreach (Dictionary<int, Unit> map in Player.Selected.Values) {
				foreach (Unit unit in map.Values) {
					availableCommands.AddRange(unit.Commands());
					break;
				}
				break;
			}

			commandPanel.UpdateCommands(availableCommands.ToArray());
		}
	}
}