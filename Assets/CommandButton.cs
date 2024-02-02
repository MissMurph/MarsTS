using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.UI {

    public class CommandButton : MonoBehaviour {

        private Command current;

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
        }

		public void UpdateCommand (string key) {
            if (key == "") {
                Deactivate();
                return;
            }

            current = CommandRegistry.Get(key);

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

        private void OnCommandActivate (CommandActiveEvent _event) {
            if (current is null) return;
            if (_event.Command.Name == current.Name
                && Player.Main.HasSelected(_event.Unit)
                && Player.UI.PrimarySelected == _event.Unit.RegistryKey) {
				activity.SetActive(_event.Activity);
            }
        }

        private void OnCooldownUpdate (CooldownEvent _event) {
			if (current is null) return;
			if (_event.CommandKey == current.Name
                && Player.Main.HasSelected(_event.Unit)
                && Player.UI.PrimarySelected == _event.Unit.RegistryKey) {

                EvaluateCooldown();
                EvaluateUsability();
            }
        }

        private void EvaluateUsability () {
			foreach (ICommandable unit in Player.Selected[Player.UI.PrimarySelected].Orderable) {
                if (unit.CanCommand(current.Name)) {
                    usable.SetActive(false);
                    return;
                }
			}

			usable.SetActive(true);
		}

        private void EvaluateActivity () {
            foreach (ICommandable unit in Player.Selected[Player.UI.PrimarySelected].Orderable) {
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

			foreach (ICommandable unit in Player.Selected[Player.UI.PrimarySelected].Orderable) {
                foreach (Cooldown activeCooldown in unit.Cooldowns) {
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