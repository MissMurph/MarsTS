using MarsTS.Players;
using MarsTS.Commands;
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

			Command bound = CommandRegistry.Get(boundCommands[index]);
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

		public void LoadCommandPage (CommandPage page) {
			string[,] commands = page.Build();

			for (int y = 0; y < 3; y++) {
				for (int x = 0; x < 3; x++) {
					int i = ((y + 1) * (x + 1)) - 1;

					boundCommands[i] = commands[y, x];
					registeredText[i].text = commands[y, x];
				}
			}
		}
	}
}