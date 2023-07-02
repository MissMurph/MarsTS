using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

	public class UnitCard : MonoBehaviour {

		[SerializeField]
		private TextMeshProUGUI nameText;

		[SerializeField]
		private TextMeshProUGUI counterText;

		[SerializeField]
		private GameObject counterPane;

		[SerializeField]
		private GameObject selectionBorder;

		public bool Selected {
			get {
				return selected;
			}

			set {
				selected = value;
				selectionBorder.SetActive(selected);
			}
		}

		private bool selected;

		private void Awake () {
			Selected = false;
			counterPane.SetActive(false);
		}

		public void UpdateUnit (string name, int count) {
			nameText.text = name;

			if (count > 1) {
				counterPane.SetActive(true);
				counterText.text = count.ToString();
			}
			else counterPane.SetActive(false);
		}
	}
}