using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.UI;
using Ratworx.MarsTS.Extensions;
using UnityEngine;

namespace Ratworx.MarsTS.UI {

	public class CommandPanel : MonoBehaviour {

		private CommandButton[] _registeredButtons;

		private string[] _boundCommands;
		private int _buttonCount;

		private CommandTooltip _tooltip;
		private int _currentTooltip;

		private string _currentlyTargetingCommand;

		private void Awake () {
			//buttonCount = registeredButtons.Length;

			_registeredButtons = GetComponentsInChildren<CommandButton>();
			_buttonCount = _registeredButtons.Length;

			_boundCommands = new string[_buttonCount];

			_tooltip = GetComponentInChildren<CommandTooltip>();
		}

		private void Start () {
			_tooltip.gameObject.SetActive(false);

			GameInit.OnSpawnSystems += OnSystemsSpawned;
		}

		private void OnSystemsSpawned()
		{
			Player.Player.Selection.OnPlayerSelectionChanged += UpdateSelectedCommands;
			Player.Player.Selection.OnPrimarySelectionChanged += UpdateSelectedCommands;
			Player.Player.Selection.OnPrimarySelectedCommandsChanged += UpdateSelectedCommands;
		}

		private void UpdateSelectedCommands() {
			List<string> commands = Player.Player.Selection.PrimarySelection is not null
				? Player.Player.Selection.PrimarySelection.GetCommandKeys()
				: new List<string>();
			
			for (int i = 0; i < _buttonCount; i++) {
				if (i >= commands.Count) {
					_boundCommands[i] = string.Empty;
					_registeredButtons[i].UpdateCommand(_boundCommands[i]);
					continue;
				}

				_boundCommands[i] = commands[i];
				_registeredButtons[i].UpdateCommand(commands[i]);
			}

			if (_currentTooltip > -1 && !string.IsNullOrEmpty(_boundCommands[_currentTooltip])) {
				_tooltip.ShowCommand(_boundCommands[_currentTooltip]);
				_tooltip.gameObject.SetActive(true);
			}
			else {
				_tooltip.gameObject.SetActive(false);
			}
		}

		public void Press (int index) {
			if (string.IsNullOrEmpty(_boundCommands[index])) return;

			if (!string.IsNullOrEmpty(_currentlyTargetingCommand)) 
				CommandPrimer.GetInterface(_currentlyTargetingCommand).CancelSelection();

			_currentlyTargetingCommand = _boundCommands[index];

			ICommandInterface bound = CommandPrimer.GetInterface(_boundCommands[index]);
			bound.StartSelection();
		}

		public void LoadCommandPage (CommandPage page) {
			string[] commands = page.Commands;

			for (int i = 0; i < commands.Length; i++) {
				if (string.IsNullOrEmpty(commands[i])) {
					_boundCommands[i] = null;
					_registeredButtons[i].UpdateCommand("");
					continue;
				}

				_boundCommands[i] = commands[i];
				_registeredButtons[i].UpdateCommand(commands[i]);
			}
		}

		public void OnPointerEnterButton (int index) {
			if (!string.IsNullOrEmpty(_boundCommands[index])) {
				_currentTooltip = index;
				_tooltip.ShowCommand(_boundCommands[index]);
				_tooltip.gameObject.SetActive(true);
			}
		}

		public void OnPointerExitButton (int index) {
			_tooltip.gameObject.SetActive(false);
			_currentTooltip = -1;
		}
	}
}