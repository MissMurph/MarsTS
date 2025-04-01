using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.UI {

    public class GameStartButton : MonoBehaviour {

		Button startButton;

		public delegate void StartHandler ();

		public event StartHandler StartGame;

		private bool started = false;

		private void Awake () {
			startButton = GetComponent<Button>();
		}

		public void Init () {
			startButton.interactable = NetworkManager.Singleton.IsHost;
		}

		public void OnPress () {
			if (!started) {
				StartGame.Invoke();
				started = true;
			}
		}
	}
}