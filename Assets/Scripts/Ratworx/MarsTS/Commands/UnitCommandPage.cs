using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using UnityEngine;

namespace Ratworx.MarsTS.Commands
{
    public class UnitCommandPage : MonoBehaviour, 
                                   ICommandReceiver
    {
        public event Action OnCommandsChanged;
        
        [SerializeField] private CommandPage[] _pagesToLoad;
        [SerializeField] private PageOption[] _pageOptions;
        
        private readonly Dictionary<string, CommandPage> _commandPageKeysToPages = new Dictionary<string, CommandPage>();

        private CommandPage _currentPage;
        
        private void Awake() {
            foreach (CommandPage commandPage in _pagesToLoad) {
                _commandPageKeysToPages[commandPage.PageKey] = commandPage;
            }

            _currentPage = _commandPageKeysToPages["default"];

            ICommandReceiver[] allReceivers = GetComponentsInChildren<ICommandReceiver>();
            
            foreach (CommandPage page in _commandPageKeysToPages.Values) {
                foreach (string commandKey in page.CommandKeys) {
                    if (string.IsNullOrEmpty(commandKey)) continue;

                    string[] splitKey = commandKey.Split('/');
                    string actualKey = splitKey[0];
                    
                    foreach (ICommandReceiver receiver in allReceivers) {
                        if (receiver.CommandKey != actualKey) continue;
                        
                        page.RegisterReceiver(commandKey, receiver);
                        break;
                    }
                }
            }
        }

        public CommandPage GetCurrentPage() => _currentPage;
        
        public event Action OnCommandStateUpdated;
        public string CommandKey => "page";
        public bool CanCommand => true;
        public int EvaluationPriority => 0;
        public bool IsActive => false;
        public bool CanInterrupt => true;
        public float Cooldown => 0f;
        
        public void ReceiveCommand(Commandlet command) {
            throw new NotImplementedException($"{nameof(UnitCommandPage)} cannot receive commands! This should never be reached!");
        }

        public (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);

        public void StartSelection(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error loading command page; argument is empty!");
                return;
            }
            
            foreach (PageOption option in _pageOptions) {
                if (option.PageKey != argument) continue;
                if (!_commandPageKeysToPages.TryGetValue(option.PageKey, out CommandPage page)) {
                    RatLogger.Error?.Log($"Command page {option.PageKey} not found!");
                    return;
                }
                    
                Player.Player.UI.CommandPanel.LoadCommandPage(page);
                return;
            }
            
            RatLogger.Error?.Log($"Command option {argument} not found!");
        }

        public Sprite GetIcon(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error loading command option icon; argument is empty!");
                return null;
            }
            
            foreach (PageOption option in _pageOptions) {
                if (option.PageKey != argument) continue;
                return option.Icon;
            }
            
            RatLogger.Error?.Log($"Command option {argument} not found!");
            return null;
        }

        public string GetDescription(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error loading command option description; argument is empty!");
                return string.Empty;
            }
            
            foreach (PageOption option in _pageOptions) {
                if (option.PageKey != argument) continue;
                return option.Description;
            }
            
            RatLogger.Error?.Log($"Command option {argument} not found!");
            return string.Empty;
        }

        public string GetName(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error loading command option name; argument is empty!");
                return string.Empty;
            }
            
            foreach (PageOption option in _pageOptions) {
                if (option.PageKey != argument) continue;
                return option.Name;
            }
            
            RatLogger.Error?.Log($"Command option {argument} not found!");
            return string.Empty;
        }
    }

    [Serializable]
    public class PageOption
    {
        public string Name;
        public Sprite Icon;
        public string Description;
        public string PageKey;
    }
}