using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands
{
    [Serializable]
    public class CommandPage : IEnumerable<(string key, ICommandReceiver receiver)>
    {
        public static CommandPage Empty => new CommandPage();
        
        public string PageKey;
        public event Action OnCommandPageUpdated;
        public (string key, ICommandReceiver receiver) this[int index] => (_commandKeys[index], _receivers[index]);
        public ICommandReceiver this[string key] => _commandKeysToReceivers[key];
        public int Length => _commandKeys.Length;
        public string[] CommandKeys => _commandKeys;

        // 0 = (1, 1)
        // 2 = (3, 1)
        // 3 = (1, 2)
        // 5 = (3, 2)
        // 6 = (1, 3)
        // 8 = (3, 3)
        [SerializeField] private string[] _commandKeys = new string[9];
        [SerializeField] private ICommandReceiver[] _receivers = new ICommandReceiver[9];

        private Dictionary<string, Vector2Int> _commandKeysToPositions = new Dictionary<string, Vector2Int>();
        private Dictionary<string, ICommandReceiver> _commandKeysToReceivers = new Dictionary<string, ICommandReceiver>();

        public void RegisterReceiver(string commandKey, ICommandReceiver receiver) {
            for (var i = 0; i < _commandKeys.Length; i++) {
                /*string[] splitKey = _commandKeys[i].Split('/');
                string actualKey = splitKey[0];*/

                if (_commandKeys[i] != commandKey) continue;
                
                _receivers[i] = receiver;
                _commandKeysToReceivers[_commandKeys[i]] = receiver;
                return;
            }
            
            RatLogger.Error?.Log($"Couldn't find matching command key {commandKey} in Command Page {PageKey}!");
        }

        public void AddCommand(string commandKey, ICommandReceiver receiver, Vector2Int position) {
            int index = GetIndexFromPosition(position);

            if (index == -1) {
                RatLogger.Error?.Log($"Can't add command {commandKey} to command page at {position}, position is out of bounds!");
                return;
            }

            if (!string.IsNullOrEmpty(_commandKeys[index])) 
                RatLogger.Message?.Log($"Command {_commandKeys[index]} at {position} being replaced with {commandKey}");

            _commandKeys[index] = commandKey;
            _receivers[index] = receiver;
            _commandKeysToReceivers[commandKey] = receiver;
            // _commandKeysToPositions[commandKey] = position;
            
            OnCommandPageUpdated?.Invoke();
        }

        public (string key, ICommandReceiver receiver) GetCommandAtPosition(Vector2Int position) {
            int index = GetIndexFromPosition(position);

            if (index == -1) {
                RatLogger.Error?.Log($"Can't get command from {position}, position is out of bounds!");
                return (string.Empty, null);
            }

            string key = _commandKeys[index];
            ICommandReceiver receiver = _receivers[index];

            return (key, receiver);
        }

        private static int GetIndexFromPosition(Vector2Int position) {
            if (position.x <= 0 || position.x > 3 || position.y <= 0 || position.y > 3) {
                return -1;
            }
            
            // minus x & y by 1
            // each y adds 3
            // each x adds 1
            position.x--;
            position.y--;

            int index = (position.y * 3) + position.x;
            return index;
        }

        public IEnumerator<(string key, ICommandReceiver receiver)> GetEnumerator() {
            var output = new (string key, ICommandReceiver receiver)[_commandKeys.Length];

            for (int i = 0; i < _commandKeys.Length; i++) {
                output[i] = (_commandKeys[i], _receivers[i]);
            }

            return output.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}