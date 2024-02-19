using MarsTS.Players;
using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using MarsTS.Events;

namespace MarsTS.UI {

	public class CommandPanel : MonoBehaviour {

		//public Button[] registeredButton;
		//private Image[] registeredIcons;

		private CommandButton[] registeredButtons;

		private string[] boundCommands;
		private int buttonCount;

		private CommandTooltip tooltip;
		private int currentTooltip;

		private string currentlyTargetingCommand;

		private void Awake () {
			//buttonCount = registeredButtons.Length;

			registeredButtons = GetComponentsInChildren<CommandButton>();
			buttonCount = registeredButtons.Length;

			boundCommands = new string[buttonCount];

			tooltip = GetComponentInChildren<CommandTooltip>();
		}

		private void Start () {
			tooltip.gameObject.SetActive(false);
		}

		public void Press (int index) {
			if (boundCommands[index] is null || boundCommands[index] == "") return;

			if (currentlyTargetingCommand != null) CommandRegistry.Get(currentlyTargetingCommand).CancelSelection();

			currentlyTargetingCommand = boundCommands[index];

			Command bound = CommandRegistry.Get(boundCommands[index]);
			bound.StartSelection();
		}

		public void UpdateCommands (params string[] commands) {
			for (int i = 0; i < buttonCount; i++) {
				if (i >= commands.Length) {
					boundCommands[i] = null;
					registeredButtons[i].UpdateCommand("");
					continue;
				}

				boundCommands[i] = commands[i];
				registeredButtons[i].UpdateCommand(commands[i]);
			}

			if (currentTooltip > -1 && boundCommands[currentTooltip] != null && boundCommands[currentTooltip] != "") {
				tooltip.ShowCommand(boundCommands[currentTooltip]);
				tooltip.gameObject.SetActive(true);
			}
			else {
				tooltip.gameObject.SetActive(false);
			}
		}

		public void LoadCommandPage (CommandPage page) {
			string[] commands = page.Commands;

			for (int i = 0; i < commands.Length; i++) {
				if (string.IsNullOrEmpty(commands[i])) {
					boundCommands[i] = null;
					registeredButtons[i].UpdateCommand("");
					continue;
				}

				boundCommands[i] = commands[i];
				registeredButtons[i].UpdateCommand(commands[i]);
			}
		}

		public void OnPointerEnterButton (int index) {
			if (boundCommands[index] != null && boundCommands[index] != "") {
				currentTooltip = index;
				tooltip.ShowCommand(boundCommands[index]);
				tooltip.gameObject.SetActive(true);
			}
		}

		public void OnPointerExitButton (int index) {
			tooltip.gameObject.SetActive(false);
			currentTooltip = -1;
		}
	}
}