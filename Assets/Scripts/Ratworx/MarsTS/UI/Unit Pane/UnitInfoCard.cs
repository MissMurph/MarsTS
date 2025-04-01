using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI.Unit_Pane {

    public class UnitInfoCard : MonoBehaviour {

        private static UnitInfoCard instance;

        private Dictionary<string, IInfoModule> registered;

        private Image icon;

		private EventAgent bus;

        private ISelectable currentUnit;

		private void Awake () {
            bus = GetComponentInParent<EventAgent>();
			registered = new Dictionary<string, IInfoModule>();
            icon = transform.Find("Icon").GetComponent<Image>();

			foreach (IInfoModule toRegister in GetComponentsInChildren<IInfoModule>()) {
                registered[toRegister.Name] = toRegister;
            }
		}

		private void Start () {
            EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);
            Deactivate();
		}

		public void DisplayInfo (ISelectable unit) {
			UnitInfoEvent _event = new UnitInfoEvent(bus, unit, this);
			bus.Global(_event);

            icon.sprite = unit.Icon;
			icon.gameObject.SetActive(true);

			UnitName name = registered["name"].Get<UnitName>();
			name.Text(unit.UnitType);
            name.gameObject.SetActive(true);
		}

        private void OnEntityDeath (UnitDeathEvent _event) {
			if (ReferenceEquals(_event.Unit, currentUnit)) {
				currentUnit = null;
                Deactivate();
			}
		}

        public T Module<T> (string key) {
            if (registered.TryGetValue(key, out IInfoModule found)) {
                T output = found.Get<T>();

                found.GameObject.SetActive(true);

                return output;
            }

            return default;
        }

        public void SetHide (string key) {

        }

        public void SetShow (string key) {

        }

        public void Deactivate () {
            foreach (IInfoModule module in registered.Values) {
	            module.Deactivate();
            }

			registered["name"].GameObject.SetActive(false);
			icon.gameObject.SetActive(false);
        }
    }
}