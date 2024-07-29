using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Events;
using UnityEngine;

namespace MarsTS.Commands
{
    public class CommandCache : MonoBehaviour
    {
        private static CommandCache _instance;

        private static int _instanceCount;

        private Dictionary<int, Commandlet> activeCommands;
        private Dictionary<int, Commandlet> staleCommands;

        public static int Count => _instance.activeCommands.Count;

        public Commandlet this[int id] => _instance.activeCommands[id];

        private void Awake()
        {
            _instance = this;

            _instanceCount = 0;
            
            activeCommands = new Dictionary<int, Commandlet>();
            staleCommands = new Dictionary<int, Commandlet>();
        }

        private void Start()
        {
            EventBus.AddListener<CommandCompleteEvent>(OnCommandComplete);
        }

        public static int Register(Commandlet commandlet)
        {
            _instanceCount++;
            return _instance.activeCommands.TryAdd(_instanceCount, commandlet) ? _instanceCount : -1;
        }

        public static bool TryGet(int id, out Commandlet output)
        {
            throw new NotImplementedException();
        }

        public static bool TryGet<T>(int id, Commandlet<T> output)
        {
            throw new NotImplementedException();
        }

        private static void OnCommandComplete(CommandCompleteEvent _event)
        {
            int id = _event.Command.Id;
            
            if (!_instance.activeCommands.ContainsKey(id)
                || _event.Command.commandedUnits.Count > 0) return;
            
            if (_instance.staleCommands.TryAdd(id, _event.Command))
            {
                _instance.activeCommands.Remove(id);
            }
            else Debug.LogError($"Error marking command {_event.Command.Name}:{_event.Command.Id} as stale");
        }

        public static bool IsStale(int id) => _instance.staleCommands.ContainsKey(id);

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}