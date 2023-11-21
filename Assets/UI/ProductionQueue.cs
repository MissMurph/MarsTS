using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

    public class ProductionQueue : MonoBehaviour {

        public int CurrentProduction;

        public int MaxProduction;

		private float fillLevel;

		private TextMeshProUGUI amountText;
		private RectTransform barRect;

		private void Awake () {
			amountText = GetComponentInChildren<TextMeshProUGUI>();
			barRect = transform.Find("Production").Find("ProductionBar") as RectTransform;
		}

		private void Start () {
			
		}
	}
}