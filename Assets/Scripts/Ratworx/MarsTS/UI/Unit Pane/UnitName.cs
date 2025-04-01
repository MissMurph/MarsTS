using TMPro;
using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Pane {

	public class UnitName : MonoBehaviour, IInfoModule {
		public GameObject GameObject => gameObject;

		public string Name => "name";

		private TextMeshProUGUI _text;

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}

		public void Deactivate()
		{
			gameObject.SetActive(false);
		}

		private void Awake () {
			_text = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void Text (string name) {
			_text.text = name;
		}
	}
}