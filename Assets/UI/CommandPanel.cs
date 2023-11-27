using MarsTS.Players;
using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace MarsTS.UI {

	public class CommandPanel : MonoBehaviour {

		public Button[] registeredButton;
		private Image[] registeredIcons;
		private string[] boundCommands;
		private int buttonCount;

		private CommandTooltip tooltip;

		private void Awake () {
			buttonCount = registeredButton.Length;
			boundCommands = new string[buttonCount];

			tooltip = GetComponentInChildren<CommandTooltip>();

			registeredIcons = new Image[buttonCount];

			for (int i = 0; i < registeredButton.Length; i++) {
				registeredIcons[i] = registeredButton[i].transform.Find("Icon").GetComponent<Image>();
				registeredIcons[i].gameObject.SetActive(false);
			}
		}

		private void Start () {
			tooltip.gameObject.SetActive(false);
		}

		public void Press (int index) {
			if (boundCommands[index] is null) return;

			Command bound = CommandRegistry.Get(boundCommands[index]);
			bound.StartSelection();
		}

		public void UpdateCommands (params string[] commands) {
			for (int i = 0; i < buttonCount; i++) {
				if (i >= commands.Length) {
					boundCommands[i] = null;
					registeredIcons[i].gameObject.SetActive(false);
					continue;
				}

				boundCommands[i] = commands[i];
				registeredIcons[i].gameObject.SetActive(true);
				registeredIcons[i].sprite = CommandRegistry.Get(boundCommands[i]).Icon;
			}
		}

		public void LoadCommandPage (CommandPage page) {
			string[] commands = page.Commands;

			for (int i = 0; i < commands.Length; i++) {
				if (string.IsNullOrEmpty(commands[i])) {
					registeredIcons[i].gameObject.SetActive(false);
					boundCommands[i] = null;
					continue;
				}

				boundCommands[i] = commands[i];
				registeredIcons[i].gameObject.SetActive(true);
				registeredIcons[i].sprite = CommandRegistry.Get(boundCommands[i]).Icon;
			}
		}

		public void OnPointerEnterButton (int index) {
			if (boundCommands[index] != null) {
				tooltip.ShowCommand(boundCommands[index]);
				tooltip.gameObject.SetActive(true);
			}
		}

		public void OnPointerExitButton (int index) {
			tooltip.gameObject.SetActive(false);
		}
	}
}