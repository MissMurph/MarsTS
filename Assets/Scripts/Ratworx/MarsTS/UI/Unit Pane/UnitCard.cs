using Ratworx.MarsTS.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI.Unit_Pane {

	public class UnitCard : MonoBehaviour {

		//[SerializeField]
		//private TextMeshProUGUI nameText;

		[SerializeField]
		private TextMeshProUGUI counterText;

		[SerializeField]
		private GameObject counterPane;

		[SerializeField]
		private GameObject selectionBorder;

		private Image icon;

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
			icon = transform.Find("Icon").GetComponent<Image>();
		}

		public void UpdateUnit (string name, int count) {
			//nameText.text = name;

			Registry.Registry.TryGetObject(name, out ISelectable unit);

			icon.sprite = unit.Icon;

			if (count > 1) {
				counterPane.SetActive(true);
				counterText.text = count.ToString();
			}
			else counterPane.SetActive(false);
		}
	}
}