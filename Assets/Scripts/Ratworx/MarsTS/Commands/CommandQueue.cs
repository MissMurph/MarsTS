using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands.Cache;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Commands.UI;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands
{
    public class CommandQueue : NetworkBehaviour,
                                IEntityComponent<CommandQueue>,
                                ICommandable,
                                IEntityServerUpdate,
                                IEntityClientUpdate
    {
        public event Action OnCommandsStateChanged;
        public event Action OnCommandListChanged;
        
        public virtual string Key => "commandQueue";

        public Commandlet CurrentCommand => Current;
        private Commandlet Current { get; set; }

        public Commandlet[] Queue => _commandQueue.ToArray();
        private Queue<Commandlet> _commandQueue;

        public List<ICommandReceiver> ActiveCommands => _activeCommands.Values.ToList();
        private Dictionary<string, ICommandReceiver> _activeCommands;

        public List<Timer> Cooldowns => _activeCooldowns.Values.ToList();

        private Dictionary<string, Timer> _activeCooldowns;
        private List<Timer> _completedCooldowns;

        public int QueueCount => Current != null ? 1 + _commandQueue.Count : 0;

        private EventAgent _eventAgent;

        public Entity Entity { get; private set; }
        
        private Dictionary<string, ICommandReceiver> _commands;
        
        public Dictionary<string, ICommandReceiver> Commands() => _commands;

        private List<ICommandReceiver> _receiversByEvalPriority;

        public CommandQueue Get() => this;
        public GameObject GameObject => gameObject;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();

            _commandQueue = new Queue<Commandlet>();

            _activeCommands = new Dictionary<string, ICommandReceiver>();
            _commands = new Dictionary<string, ICommandReceiver>();
            _receiversByEvalPriority = new List<ICommandReceiver>();

            _activeCooldowns = new Dictionary<string, Timer>();
            _completedCooldowns = new List<Timer>();
            
            foreach (ICommandReceiver receiver in GetComponentsInChildren<ICommandReceiver>()) {
                _commands[receiver.CommandKey] = receiver;
                _receiversByEvalPriority.Add(receiver);
            }

            _receiversByEvalPriority.Sort(CompareReceiversByEvaluationPriority);
        }

        public void UpdateServer() {
            if (Current is null && _commandQueue.Count > 0) {
                Dequeue();
            }
            
            UpdateTimers();
        }

        public void UpdateClient() {
            UpdateTimers();
        }

        private void UpdateTimers() {
            foreach (Timer cooldown in _activeCooldowns.Values) {
                cooldown.timeRemaining -= Time.deltaTime;

                if (cooldown.timeRemaining <= 0) {
                    _completedCooldowns.Add(cooldown);
                    continue;
                }

                _eventAgent.PostGlobal(new CooldownEvent(cooldown.command, Entity, cooldown.timeRemaining));
                OnCommandsStateChanged?.Invoke();
            }

            foreach (Timer expiredCooldown in _completedCooldowns) {
                _activeCooldowns.Remove(expiredCooldown.commandName);
                _eventAgent.PostGlobal(new CooldownEvent(expiredCooldown.command, Entity, expiredCooldown.timeRemaining));
                OnCommandsStateChanged?.Invoke();
            }

            _completedCooldowns = new();
        }

        /*	Dequeueing Commands	*/

        protected virtual void Dequeue() {
            Commandlet order = _commandQueue.Dequeue();

            Current = order;
            order.OnCommandComplete.AddListener(OnOrderComplete);

            order.StartCommand(this);
            OnCommandListChanged?.Invoke();

            if (NetworkManager.Singleton.IsServer) 
                DequeueClientRpc();
        }

        [Rpc(SendTo.NotServer)]
        private void DequeueClientRpc() {
            if (NetworkManager.IsHost) return;
            Dequeue();
        }

        /*	Completing Commands	*/

        [Rpc(SendTo.NotServer)]
        private void CompleteCommandClientRpc(bool _cancelled) {
            CompleteCurrentCommand(_cancelled);
        }

        private void CompleteCurrentCommand(bool _cancelled) {
            // This is 
            // Current.CompleteCommand(bus, orderSource, _cancelled);

            if (NetworkManager.Singleton.IsServer)
                CompleteCommandClientRpc(_cancelled);
        }

        private void OnOrderComplete(CommandCompleteEvent evnt) {
            evnt.Command.OnCommandComplete.RemoveListener(OnOrderComplete);
            Current = null;
            _eventAgent.PostGlobal(evnt);
            OnCommandListChanged?.Invoke();
        }

        /*	Executing Commands	*/
        private void ExecuteCommand(Commandlet order) {
            if (!CanCommand(order.Command.Name)) return;
            _commandQueue.Clear();

            if (Current != null) {
                // if (!Current.CanInterrupt()) return;

                Current.CompleteCommand(this, true);
            }
            
            // Current = null;
            _commandQueue.Enqueue(order);

            if (NetworkManager.Singleton.IsServer) ExecuteClientRpc(order.gameObject);
        }

        [Rpc(SendTo.NotServer)]
        private void ExecuteClientRpc(NetworkObjectReference orderReference) {
            if (NetworkManager.Singleton.IsHost) return;

            ExecuteCommand(orderReference.GameObject().GetComponent<Commandlet>());
        }

        /*	Enqueueing Commands	*/

        private void EnqueueCommand(Commandlet order) {
            if (!CanCommand(order.Command.Name)) return;
            _commandQueue.Enqueue(order);
            OnCommandListChanged?.Invoke();

            if (NetworkManager.Singleton.IsServer) EnqueueClientRpc(order.gameObject);
        }

        [Rpc(SendTo.NotServer)]
        private void EnqueueClientRpc(NetworkObjectReference orderReference) {
            if (NetworkManager.Singleton.IsHost) return;

            EnqueueCommand(orderReference.GameObject().GetComponent<Commandlet>());
        }

        /*	Activating Commands	*/
        public void ActivateCommand(ICommandReceiver order, bool status) {
            if (status) {
                _activeCommands[order.CommandKey] = order;
            }
            else if (_activeCommands.TryGetValue(order.CommandKey, out ICommandReceiver toDeactivate)) {
                _activeCommands.Remove(toDeactivate.CommandKey);
            }
            
            _eventAgent.PostGlobal(new CommandActiveEvent(this, order, status));
            OnCommandsStateChanged?.Invoke();
            
            if (NetworkManager.Singleton.IsServer)
                ActivateCommandClientRpc(order.CommandKey, status);
        }

        [Rpc(SendTo.NotServer)]
        private void ActivateCommandClientRpc(string commandKey, bool status) {
            ActivateCommand(_commands[commandKey], status);
        }

        public void DeactivateCommand(string key) {
            if (!_activeCommands.TryGetValue(key, out ICommandReceiver toDeactivate)) return;

            CommandActiveEvent evnt = new CommandActiveEvent(this, toDeactivate, false);
            _activeCommands.Remove(toDeactivate.CommandKey);
            _eventAgent.PostGlobal(evnt);
            OnCommandsStateChanged?.Invoke();

            if (NetworkManager.Singleton.IsServer)
                DeactivateCommandClientRpc(key);
        }

        [Rpc(SendTo.NotServer)]
        private void DeactivateCommandClientRpc(string key) {
            DeactivateCommand(key);
        }

        /*	Cooldowns	*/

        public void Cooldown(Commandlet order, float time) {
            _activeCooldowns[order.Name] = new Timer { commandName = order.Name, duration = time, timeRemaining = time };
            OnCommandsStateChanged?.Invoke();

            if (NetworkManager.Singleton.IsServer) CooldownClientRpc(order.Id, time);
        }

        [Rpc(SendTo.NotServer)]
        private void CooldownClientRpc(int id, float time) {
            if (!CommandletsCache.TryGet(id, out Commandlet order)) {
                RatLogger.Error?.Log($"Couldn't find Commandlet {id}, cannot start Cooldown");
                return;
            }

            Cooldown(order, time);
        }

        /*	Misc.	*/

        public void Clear() {
            foreach (Commandlet order in _commandQueue)
                order.CompleteCommand(this, true);

            _commandQueue.Clear();

            if (Current != null)
                Current.CompleteCommand(this, true);

            // Current = null;

            if (NetworkManager.Singleton.IsServer) ClearClientRpc();
        }

        [Rpc(SendTo.NotServer)]
        private void ClearClientRpc() {
            Clear();
        }

        public virtual bool CanCommand(string commandKey) {
            return !_activeCooldowns.ContainsKey(commandKey);
        }

        public void Order(Commandlet order, bool inclusive) {
            if (inclusive)
                EnqueueCommand(order);
            else
                ExecuteCommand(order);
        }

        public ICommandInterface EvaluateCommand(Entity target) {
            for (int i = 0; i < _receiversByEvalPriority.Count; i++) {
                (bool result, ICommandInterface command) = _receiversByEvalPriority[i].EvaluateCommand(target);
                if (!result) continue;
                return command;
            }

            return CommandPrimer.GetInterface("move");
        }

        public void AddCommand(ICommandReceiver receiver) {
            if (_commands.ContainsKey(receiver.CommandKey)) 
                RatLogger.Warning?.Log($"Receiver already bound to {receiver.CommandKey}! Replacing with new receiver, is this intended?");

            _commands[receiver.CommandKey] = receiver;
            OnCommandListChanged?.Invoke();
        }

        public void RemoveCommand(string commandKey) {
            _commands.Remove(commandKey);
            OnCommandListChanged?.Invoke();
        }
        
        private int CompareReceiversByEvaluationPriority(ICommandReceiver a, ICommandReceiver b) {
            if (a.EvaluationPriority > b.EvaluationPriority)
                return 1;
            if (a.EvaluationPriority < b.EvaluationPriority)
                return -1;
            return 0;
        }
    }

    public class Timer
    {
        public ICommandReceiver command;
        public string commandName;
        public float duration;
        public float timeRemaining;
    }
}