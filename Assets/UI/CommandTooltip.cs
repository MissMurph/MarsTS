using MarsTS.Commands;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.UI {

    public class CommandTooltip : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI commandName;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI commandDescription;

        [SerializeField]
        private GameObject costPrefab;

        private GameObject[] costModules;
        private Image[] costIcons;
        private TextMeshProUGUI[] costText;

		private void Awake () {
            costModules = new GameObject[3];
            costIcons = new Image[3];
            costText = new TextMeshProUGUI[3];
		}

		public void ShowCommand (string commandKey) {
            Command source = CommandRegistry.Get(commandKey);

            commandName.text = source.name;
            icon.sprite = source.Icon;
            commandDescription.text = source.Description;

            CostEntry[] commandCost = source.GetCost();

            foreach (GameObject instantiated in costModules) {
                Destroy(instantiated);
            }

			if (commandCost.Length == 0) {
                RectTransform rect = commandDescription.transform as RectTransform;
                //rect.anchoredPosition = new Vector3(-135, -65, 0);

				float descSize = (commandDescription.textBounds.extents * 2).y;

				RectTransform wholeTooltip = transform as RectTransform;

				wholeTooltip.sizeDelta = new Vector2(270, descSize + 70);
			}
            else {
				RectTransform rect = commandDescription.transform as RectTransform;
				//rect.anchoredPosition = new Vector3(-135, -100, 0);

				float descSize = (commandDescription.textBounds.extents * 2).y;

                RectTransform wholeTooltip = transform as RectTransform;

                wholeTooltip.sizeDelta = new Vector2(270, descSize + 35 + 70);

				for (int i = 0; i < commandCost.Length; i++) {
                    RectTransform newCost = Instantiate(costPrefab, transform).transform as RectTransform;
                    newCost.anchoredPosition = new Vector3(42.5f + (80 * i), 0, 0);
                    costModules[i] = newCost.gameObject;

					Image costIcon = newCost.Find("Icon").GetComponent<Image>();
                    TextMeshProUGUI costAmount = newCost.Find("Amount").GetComponent<TextMeshProUGUI>();

                    costIcon.sprite = ResourceRegistry.Get(commandCost[i].key).Icon;
                    costAmount.text = commandCost[i].amount.ToString();
                }
            }
        }
    }
}