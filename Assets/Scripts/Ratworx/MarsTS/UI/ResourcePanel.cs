using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI
{
    public class ResourcePanel : MonoBehaviour
    {
        private Dictionary<string, TextMeshProUGUI> counters;

        [SerializeField] private GameObject CounterPrefab;

        private void Awake()
        {
            counters = new Dictionary<string, TextMeshProUGUI>();
        }

        private void Start()
        {
            EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
        }

        private void OnPlayerInit(PlayerInitEvent _event)
        {
            foreach (PlayerResource used in Player.Player.Commander.Resources)
            {
                GameObject newCounter = Instantiate(CounterPrefab, transform);

                RectTransform counterRect = newCounter.transform as RectTransform;
                counterRect.anchoredPosition = new Vector3(55 + 105 * counters.Count, -25, 0);

                Image icon = newCounter.transform.Find("Icon").GetComponent<Image>();

                icon.sprite = ResourceRegistry.Get(used.Key).Icon;

                counters[used.Key] = newCounter.transform.Find("Counter").GetComponent<TextMeshProUGUI>();
                counters[used.Key].text = used.Amount.ToString();
            }

            RectTransform rect = transform as RectTransform;

            rect.sizeDelta = new Vector2(100 * counters.Count + 5 * (counters.Count + 1), 50);
            rect.anchoredPosition = new Vector3(rect.sizeDelta.x / 2, -25);

            EventBus.AddListener<ResourceUpdateEvent>(OnPlayerResourceUpdate);
        }

        private void OnPlayerResourceUpdate(ResourceUpdateEvent _event)
        {
            if (!Player.Player.Commander.Equals(_event.Player)) return;

            if (counters.TryGetValue(_event.Resource.Key, out TextMeshProUGUI counter))
                counter.text = _event.Amount.ToString();
        }
    }
}