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
            CommandFactory source = CommandRegistry.Get(commandKey);

            commandName.text = source.name;
            icon.sprite = source.Icon;
            commandDescription.text = source.Description;
            commandDescription.ForceMeshUpdate(true, true);

			float descSize = (commandDescription.textBounds.extents * 2).y + 5;

			descSize = Mathf.Max(descSize, 20f);

			RectTransform wholeTooltip = transform as RectTransform;

			CostEntry[] commandCost = source.GetCost();

            foreach (GameObject instantiated in costModules) {
                Destroy(instantiated);
            }

			if (commandCost.Length == 0) {
                RectTransform rect = commandDescription.transform as RectTransform;
                rect.anchoredPosition = new Vector3(0, -65, 0);

				wholeTooltip.sizeDelta = new Vector2(0, descSize + 70);
			}
            else {
				RectTransform rect = commandDescription.transform as RectTransform;
				rect.anchoredPosition = new Vector3(0, -100, 0);

                wholeTooltip.sizeDelta = new Vector2(0, descSize + 35 + 70);

				for (int i = 0; i < commandCost.Length; i++) {
                    RectTransform newCost = Instantiate(costPrefab, transform).transform as RectTransform;
                    newCost.anchoredPosition = new Vector3(42.5f + (80 * i), -85f, 0);
                    costModules[i] = newCost.gameObject;

					Image costIcon = newCost.Find("Icon").GetComponent<Image>();
                    TextMeshProUGUI costAmount = newCost.Find("Amount").GetComponent<TextMeshProUGUI>();

                    costIcon.sprite = ResourceRegistry.Get(commandCost[i].key).Icon;
                    costAmount.text = commandCost[i].amount.ToString();
                }
            }

            wholeTooltip.anchoredPosition = new Vector3(0, wholeTooltip.sizeDelta.y / 2, 0);
		}
    }
}