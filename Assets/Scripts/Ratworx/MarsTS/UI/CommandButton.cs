using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI
{
    public class CommandButton : MonoBehaviour
    {
        private CommandFactory current;

        private Image _icon;
        private Image _cooldown;
        private GameObject _usable;
        private GameObject _activity;
        private TextMeshProUGUI _cooldownText;

        private void Awake() {
            _icon = transform.Find("Icon").GetComponent<Image>();
            _cooldown = transform.Find("CooldownOverlay").GetComponent<Image>();
            _usable = transform.Find("UsableOverlay").gameObject;
            _activity = transform.Find("ActivityBorder").gameObject;
            _cooldownText = _cooldown.GetComponentInChildren<TextMeshProUGUI>();

            Deactivate();
        }

        private void Start() {
            EventBus.AddListener<CommandActiveEvent>(OnCommandActivate);
            EventBus.AddListener<CooldownEvent>(OnCooldownUpdate);
        }

        public void UpdateCommand(string key) {
            if (key == "") {
                Deactivate();
                return;
            }

            if (!CommandPrimer.TryGet(key, out CommandFactory factory))
                return;

            current = factory;

            _icon.sprite = current.Icon;
            _icon.gameObject.SetActive(true);

            EvaluateActivity();
            EvaluateUsability();
            EvaluateCooldown();
        }

        public void Press() { }

        public void Deactivate() {
            current = null;
            _icon.gameObject.SetActive(false);
            _cooldown.gameObject.SetActive(false);
            _usable.SetActive(false);
            _activity.SetActive(false);
        }

        public void OnPointerEnterButton() { }

        public void OnPointerExitButton() { }

        private void OnCommandStart(CommandStartEvent _event) {
            if (current is null) return; 
            if (_event.Command.Command.Name == current.Name) {
                if (!Player.Player.HasSelected(_event.Unit as ISelectable)) return;
                if (Player.Player.UI.PrimarySelected != (_event.Unit as ISelectable)?.RegistryKey) return;

                EvaluateUsability();
                EvaluateActivity();
            }
        }

        private void OnCommandActivate(CommandActiveEvent _event) {
            if (current is null) return;
            if (_event.CommandReceiver.Name == current.Name
                && Player.Player.HasSelected(_event.Unit as ISelectable)
                && Player.Player.UI.PrimarySelected == (_event.Unit as ISelectable)?.RegistryKey)
                _activity.SetActive(_event.Activity);
        }

        private void OnCooldownUpdate(CooldownEvent evnt) {
            if (current is null) return;
            if (evnt.Command.CommandKey == current.Name
                && Player.Player.HasSelected(evnt.Entity)
                && Player.Player.UI.PrimarySelected == evnt.Unit.RegistryKey) {
                EvaluateCooldown();
                EvaluateUsability();
            }
        }

        private void EvaluateUsability() {
            foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].GetCommandables()) {
                if (unit.CanCommand(current.Name)) {
                    _usable.SetActive(false);
                    return;
                }
            }

            _usable.SetActive(true);
        }

        private void EvaluateActivity() {
            foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].GetCommandables()) {
                if (unit.ActiveCommands.Contains(current.Name)) {
                    _activity.SetActive(true);
                    return;
                    
                }
            }

            _activity.SetActive(false);
        }

        private void EvaluateCooldown() {
            bool coolingDown = false;
            float lowestCooldown = 999f;
            float cooldownDuration = 0f;

            foreach (ICommandable unit in Player.Player.Selected[Player.Player.UI.PrimarySelected].GetCommandables()) {
                foreach (Timer activeCooldown in unit.Cooldowns) {
                    if (activeCooldown.commandName == current.Name) {
                        coolingDown = true;
                        cooldownDuration = activeCooldown.duration;

                        if (activeCooldown.timeRemaining < lowestCooldown)
                            lowestCooldown = activeCooldown.timeRemaining;
                    }
                }
            }

            if (coolingDown) {
                float progress = lowestCooldown / cooldownDuration;
                _cooldown.fillAmount = progress;
                _cooldownText.text = ((int)lowestCooldown).ToString();
                _cooldown.gameObject.SetActive(true);
            }
            else {
                _cooldown.gameObject.SetActive(false);
            }
        }
    }
}