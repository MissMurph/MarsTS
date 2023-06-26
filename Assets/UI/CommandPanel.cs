using MarsTS.Players;
using MarsTS.Units.Commands;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.UI {

	public class CommandPanel : MonoBehaviour {

		public Button[] registeredButton;
		public TextMeshProUGUI[] registeredText;
		private string[] boundCommands;
		private int buttonCount;

		private void Awake () {
			buttonCount = registeredButton.Length;
			boundCommands = new string[buttonCount];
		}

		public void Press (int index) {
			if (boundCommands[index] is null) return;

			Command bound = Commands.Get(boundCommands[index]);
			bound.StartSelection();
		}

		public void UpdateCommands (params string[] commands) {
			for (int i = 0; i < buttonCount; i++) {
				if (i >= commands.Length) {
					boundCommands[i] = null;
					registeredText[i].text = "";
					continue;
				}

				boundCommands[i] = commands[i];
				registeredText[i].text = boundCommands[i];
			}
		}
	}
}