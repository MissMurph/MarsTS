using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

	public class UnitName : MonoBehaviour, IInfoModule {
		public GameObject GameObject => gameObject;

		public string Name => "name";

		private TextMeshProUGUI text;

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}

		private void Awake () {
			text = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void Text (string name) {
			text.text = name;
		}
	}
}