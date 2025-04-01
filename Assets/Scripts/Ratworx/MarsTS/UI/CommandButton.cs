using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Units;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI {

    public class CommandButton : MonoBehaviour {

        private CommandFactory current;

        private Image icon;
        private Image cooldown;
        private GameObject usable;
        private GameObject activity;
        private TextMeshProUGUI cooldownText;

		private void Awake () {
            icon = transform.Find("Icon").GetComponent<Image>();
			cooldown = transform.Find("CooldownOverlay").GetComponent<Image>();
			usable = transform.Find("UsableOverlay").gameObject;
			activity = transform.Find("ActivityBorder").gameObject;
			cooldownText = cooldown.GetComponentInChildren<TextMeshProUGUI>();

			Deactivate();
		}

        private void Start () {
            EventBus.AddListener<CommandActiveEvent>(OnCommandActivate);
            EventBus.AddListener<CooldownEvent>(OnCooldownUpdate);
            EventBus.AddListener<CommandStartEvent>(OnCommandStart);

		}

		public void UpdateCommand (string key) {
            if (key == "") {
                Deactivate();
                return;
            }

            if (!CommandPrimer.TryGet(key, out CommandFactory factory)) 
	            return;

            current = factory;

            icon.sprite = current.Icon;
            icon.gameObject.SetActive(true);

            EvaluateActivity();
            EvaluateUsability();
            EvaluateCooldown();
        }

        public void Press () {

        }

        public void Deactivate () {
            current = null;
            icon.gameObject.SetActive(false);
			cooldown.gameObject.SetActive(false);
            usable.SetActive(false);
            activity.SetActive(false);
		}

        public void OnPointerEnterButton () {

        }

        public void OnPointerExitButton () {

        }

        private void OnCommandStart (CommandStartEvent _event) {
			if (current is null) return;
            if (_event.Command.Command.Name == current.Name) {

                if (!Player.Player.HasSelected(_event.Unit as ISelectable)) return;
                if (Player.Player.UI.PrimarySelected != (_event.Unit as ISelectable)?.RegistryKey) return;

				EvaluateUsability();
                EvaluateActivity();
			}
		}

        private void OnCommandActivate (CommandActiveEvent _event) {
            if (current is null) return;
            if (_event.Command.Name == current.Name
                && Player.Player.HasSelected(_event.Unit as ISelectable)
                && Player.Player.UI.PrimarySelected == (_event.Unit as ISelectable)?.RegistryKey) {
				activity.SetActive(_event.Activity);
            }
        }

        private void OnCooldownUpdate (CooldownEvent _event) {
			if (current is null) return;
			if (_event.CommandKey == current.Name
                && Player.Player.HasSelected(_event.Unit)
                && Player.Player.UI.PrimarySelected == _event.Unit.RegistryKey) {

                EvaluateCooldown();
                EvaluateUsability();
            }
        }

        private void EvaluateUsability () {
			foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].Orderable) {
                if (unit.CanCommand(current.Name)) {
                    usable.SetActive(false);
                    return;
                }
			}

			usable.SetActive(true);
		}

        private void EvaluateActivity () {
            foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].Orderable) {
                if (unit.Active.Contains(current.Name)) {
                    activity.SetActive(true);
                    return;
                }
			}

			activity.SetActive(false);
		}

        private void EvaluateCooldown () {
			bool coolingDown = false;
			float lowestCooldown = 999f;
            float cooldownDuration = 0f;

			foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].Orderable) {
                foreach (Timer activeCooldown in unit.Cooldowns) {
					if (activeCooldown.commandName == current.Name) {
                        coolingDown = true;
                        cooldownDuration = activeCooldown.duration;

						if (activeCooldown.timeRemaining < lowestCooldown) lowestCooldown = activeCooldown.timeRemaining;
					}
				}
			}

            if (coolingDown) {
                float progress = lowestCooldown / cooldownDuration;
                cooldown.fillAmount = progress;
                cooldownText.text = ((int) lowestCooldown).ToString();
                cooldown.gameObject.SetActive(true);
            }
            else {
                cooldown.gameObject.SetActive(false);
            }
		}
    }
}