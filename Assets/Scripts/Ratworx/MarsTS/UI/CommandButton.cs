using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;
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
        private ICommandReceiver _current;

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

        private void Start() => GameInit.OnSpawnSystems += OnSystemsSpawned;

        private void OnSystemsSpawned()
        {
            Player.Player.Selection.OnPrimarySelectedCommandsStateChanged += UpdateSelectedCommandState;
        }

        private void UpdateSelectedCommandState() {
            EvaluateActivity();
            EvaluateUsability();
            EvaluateCooldown();
        }

        public void UpdateCommand(string key, ICommandReceiver receiver) {
            if (string.IsNullOrEmpty(key) || receiver is null) {
                Deactivate();
                return;
            }

            // Arguments are placed after a / delimiter
            var splitKey = key.Split('/');
            string argument = splitKey.Length >= 2 ? splitKey[1] : string.Empty;
            
            _current = receiver;
            _icon.sprite = _current.GetIcon(argument);
            _icon.gameObject.SetActive(true);

            EvaluateActivity();
            EvaluateUsability();
            EvaluateCooldown();
        }

        public void Press() { }

        public void Deactivate() {
            _current = null;
            _icon.gameObject.SetActive(false);
            _cooldown.gameObject.SetActive(false);
            _usable.SetActive(false);
            _activity.SetActive(false);
        }

        public void OnPointerEnterButton() { }

        public void OnPointerExitButton() { }

        private void EvaluateUsability() {
            if (_current is not null
                && Player.Player.Selection.PrimarySelection is not null) {
                foreach (ICommandable unit in Player.Player.Selection.PrimarySelection.GetCommandables()) {
                    if (unit.CanCommand(_current.CommandKey)) {
                        _usable.SetActive(false);
                        return;
                    }
                }
            }

            _usable.SetActive(true);
        }

        private void EvaluateActivity() {
            if (_current is not null
                && Player.Player.Selection.PrimarySelection is not null) {
                foreach (ICommandable unit in Player.Player.Selection.PrimarySelection.GetCommandables()) {
                    if (unit.ActiveCommands.Any(receiver => receiver.CommandKey == _current.CommandKey)) {
                        _activity.SetActive(true);
                        return;
                    
                    }
                }
            }

            _activity.SetActive(false);
        }

        private void EvaluateCooldown() {
            bool coolingDown = false;
            float lowestCooldown = 999f;
            float cooldownDuration = 0f;

            if (_current is not null
                && Player.Player.Selection.PrimarySelection is not null) {
                foreach (ICommandable unit in Player.Player.Selection.PrimarySelection.GetCommandables()) {
                    foreach (Timer activeCooldown in unit.Cooldowns) {
                        if (activeCooldown.commandName == _current.CommandKey) {
                            coolingDown = true;
                            cooldownDuration = activeCooldown.duration;

                            if (activeCooldown.timeRemaining < lowestCooldown)
                                lowestCooldown = activeCooldown.timeRemaining;
                        }
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