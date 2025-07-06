using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Receivers;
using UnityEngine;

namespace Ratworx.MarsTS.Commands
{
    public class UnitCommandPage : MonoBehaviour
    {
        public event Action OnCommandsChanged;
        
        [SerializeField] private CommandPage[] _pagesToLoad;
        
        private readonly Dictionary<string, CommandPage> _commandPageKeysToPages = new Dictionary<string, CommandPage>();

        private CommandPage _currentPage;
        
        private void Awake() {
            foreach (CommandPage commandPage in _pagesToLoad) {
                _commandPageKeysToPages[commandPage.PageKey] = commandPage;
            }

            _currentPage = _commandPageKeysToPages["default"];
        }

        public CommandPage GetCurrentPage() => _currentPage;
    }
}