using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

	public class UnitCard : MonoBehaviour {
		private Unit currentUnit;
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI counterText;
		[SerializeField]
		private GameObject counterPane;

		private void Awake () {
			nameText = GetComponentInChildren<TextMeshProUGUI>();
		}

		public void UpdateUnit (ISelectable unit, int count) {
			currentUnit = unit.Get();
			nameText.text = currentUnit.name;

			if (count > 1) {
				counterPane.SetActive(true);
				counterText.text = count.ToString();
			}
			else counterPane.SetActive(false);
		}
	}
}