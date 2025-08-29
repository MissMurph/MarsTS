using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Extensions;
using UnityEngine;

namespace Ratworx.MarsTS.UI {

	public class CommandPanel : MonoBehaviour {

		private CommandButton[] _registeredButtons;

		private (string key, ICommandReceiver receiver)[] displayedCommands;
		private int _buttonCount;

		private CommandTooltip _tooltip;
		private int _currentTooltip;

		private (string key, ICommandReceiver receiver) _currentlyTargetingCommand;

		private void Awake () {
			//buttonCount = registeredButtons.Length;

			_registeredButtons = GetComponentsInChildren<CommandButton>();
			_buttonCount = _registeredButtons.Length;

			displayedCommands = new (string key, ICommandReceiver receiver)[_buttonCount];

			_tooltip = GetComponentInChildren<CommandTooltip>();
		}

		private void Start () {
			_tooltip.gameObject.SetActive(false);

			GameInit.OnSpawnSystems += OnSystemsSpawned;
		}

		private void OnSystemsSpawned()
		{
			// Player.Player.Selection.OnPlayerSelectionChanged += UpdateSelectedCommands;
			// Player.Player.Selection.OnPrimarySelectionChanged += UpdateSelectedCommands;
			Player.Player.Selection.OnPrimarySelectedCommandsChanged += UpdateSelectedCommands;
		}

		private void UpdateSelectedCommands() {
			if (Player.Player.Selection.PrimarySelection is not null) {
				CommandPage page = Player.Player.Selection.PrimarySelection.GetCommands();

				if (page is not null) {
					LoadCommandPage(page);
					return;
				}
			}
			
			LoadCommandPage(CommandPage.Empty);
		}

		public void Press (int index) {
			if (string.IsNullOrEmpty(displayedCommands[index].key)) return;

			if (!string.IsNullOrEmpty(_currentlyTargetingCommand.key)
				&& CommandPrimer.TryGetInterface(_currentlyTargetingCommand.receiver.CommandKey,
					out ICommandInterface commandInterface))
				commandInterface.CancelSelection();

			_currentlyTargetingCommand = displayedCommands[index];
			
			var splitKey = _currentlyTargetingCommand.key.Split('/');
			string argument = splitKey.Length >= 2 ? splitKey[1] : string.Empty;
			
			_currentlyTargetingCommand.receiver.StartSelection(argument);
		}

		public void LoadCommandPage (CommandPage page) {
			// arbitrary numbers are arbitrary
			for (int i = 0; i < 9; i++) {
				if (i >= page.Length || string.IsNullOrEmpty(page[i].key)) {
					displayedCommands[i] = (string.Empty, null);
					_registeredButtons[i].UpdateCommand("", null);
					continue;
				}

				displayedCommands[i] = page[i];
				_registeredButtons[i].UpdateCommand(page[i].key, page[i].receiver);
			}
			
			if (_currentTooltip > -1 && !string.IsNullOrEmpty(displayedCommands[_currentTooltip].key)) {
				_tooltip.ShowCommand(displayedCommands[_currentTooltip].key, displayedCommands[_currentTooltip].receiver);
				_tooltip.gameObject.SetActive(true);
			}
			else {
				_tooltip.gameObject.SetActive(false);
			}
		}

		public void OnPointerEnterButton (int index) {
			if (!string.IsNullOrEmpty(displayedCommands[index].key)) {
				_currentTooltip = index;
				_tooltip.ShowCommand(displayedCommands[index].key, displayedCommands[index].receiver);
				_tooltip.gameObject.SetActive(true);
			}
		}

		public void OnPointerExitButton (int index) {
			_tooltip.gameObject.SetActive(false);
			_currentTooltip = -1;
		}
	}
}