using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands.Cache;
using Ratworx.MarsTS.Commands.Factories;
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

        public Commandlet[] Queue => commandQueue.ToArray();
        private Queue<Commandlet> commandQueue;

        public List<string> Active => activeCommands.Keys.ToList();
        private Dictionary<string, Commandlet> activeCommands;

        public List<Timer> Cooldowns => activeCooldowns.Values.ToList();

        private Dictionary<string, Timer> activeCooldowns;
        private List<Timer> completedCooldowns;

        public int Count => Current != null ? 1 + commandQueue.Count : 0;

        private EventAgent _eventAgent;

        public Entity Entity { get; private set; }

        [SerializeField] private string[] _commands;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();

            commandQueue = new Queue<Commandlet>();

            activeCommands = new Dictionary<string, Commandlet>();

            activeCooldowns = new Dictionary<string, Timer>();
            completedCooldowns = new List<Timer>();
        }

        public void UpdateServer() {
            if (Current is null && commandQueue.Count > 0) {
                Dequeue();
            }
        }

        public void UpdateClient() {
            
        }

        protected virtual void Update() {
            foreach (Timer cooldown in activeCooldowns.Values) {
                cooldown.timeRemaining -= Time.deltaTime;

                if (cooldown.timeRemaining <= 0) {
                    completedCooldowns.Add(cooldown);
                    continue;
                }

                _eventAgent.PostGlobal(new CooldownEvent(cooldown.commandName, parent, cooldown));
            }

            foreach (Timer expiredCooldown in completedCooldowns) {
                activeCooldowns.Remove(expiredCooldown.commandName);
                _eventAgent.PostGlobal(new CooldownEvent(_eventAgent, expiredCooldown.commandName, parent, expiredCooldown));
            }

            completedCooldowns = new();
        }

        /*	Dequeueing Commands	*/

        protected virtual void Dequeue() {
            Commandlet order = commandQueue.Dequeue();

            Current = order;
            order.OnCommandComplete.AddListener(OnOrderComplete);

            order.StartCommand(this);
            
        }

        [Rpc(SendTo.NotServer)]
        protected virtual void DequeueClientRpc() {
            if (NetworkManager.IsHost) return;
            Dequeue();
        }

        /*	Completing Commands	*/

        [Rpc(SendTo.NotServer)]
        protected virtual void CompleteCommandClientRpc(bool _cancelled) {
            CompleteCurrentCommand(_cancelled);
        }

        protected virtual void CompleteCurrentCommand(bool _cancelled) {
            // Current.CompleteCommand(bus, orderSource, _cancelled);

            if (NetworkManager.Singleton.IsServer)
                CompleteCommandClientRpc(_cancelled);
        }

        protected virtual void OnOrderComplete(CommandCompleteEvent _event) {
            if (!ReferenceEquals(_event.Unit, orderSource)) return;
            Current = null;
            _eventAgent.PostGlobal(_event);
        }

        /*	Executing Commands	*/
        public void ExecuteCommand(Commandlet order) {
            if (!orderSource.CanCommand(order.Command.Name)) return;
            commandQueue.Clear();

            if (Current != null) {
                // if (!Current.CanInterrupt()) return;

                Current.CompleteCommand(orderSource, true);
            }

            Current = null;
            commandQueue.Enqueue(order);

            if (NetworkManager.Singleton.IsServer) ExecuteClientRpc(order.gameObject);
        }

        [Rpc(SendTo.NotServer)]
        private void ExecuteClientRpc(NetworkObjectReference orderReference) {
            if (NetworkManager.Singleton.IsHost) return;

            ExecuteCommand(orderReference.GameObject().GetComponent<Commandlet>());
        }

        /*	Enqueueing Commands	*/

        public void EnqueueCommand(Commandlet order) {
            if (!orderSource.CanCommand(order.Command.Name)) return;
            commandQueue.Enqueue(order);

            if (NetworkManager.Singleton.IsServer) EnqueueClientRpc(order.gameObject);
        }

        [Rpc(SendTo.NotServer)]
        protected virtual void EnqueueClientRpc(NetworkObjectReference orderReference) {
            if (NetworkManager.Singleton.IsHost) return;

            EnqueueCommand(orderReference.GameObject().GetComponent<Commandlet>());
        }

        /*	Activating Commands	*/
        public void ActivateCommand(Commandlet order, bool status) {
            if (status) {
                activeCommands[order.Name] = order;
            }
            else if (activeCommands.TryGetValue(order.Name, out Commandlet toDeactivate)) {
                activeCommands.Remove(toDeactivate.Name);
            }

            _eventAgent.PostGlobal(new CommandActiveEvent(this, order, status));

            if (NetworkManager.Singleton.IsServer)
                ActivateCommandClientRpc(order.Id, status);
        }

        [Rpc(SendTo.NotServer)]
        private void ActivateCommandClientRpc(int id, bool status) {
            if (!CommandletsCache.TryGet(id, out Commandlet order)) {
                RatLogger.Error?.Log($"Couldn't find commandlet {id}! Cannot activate");
                return;
            }

            ActivateCommand(order, status);
        }

        public void DeactivateCommand(string key) {
            if (!activeCommands.TryGetValue(key, out Commandlet toDeactivate)) return;

            CommandActiveEvent evnt = new CommandActiveEvent(this, toDeactivate, false);
            activeCommands.Remove(toDeactivate.Name);
            _eventAgent.PostGlobal(evnt);

            if (NetworkManager.Singleton.IsServer)
                DeactivateCommandClientRpc(key);
        }

        [Rpc(SendTo.NotServer)]
        private void DeactivateCommandClientRpc(string key) {
            DeactivateCommand(key);
        }

        /*	Cooldowns	*/

        public void Cooldown(Commandlet order, float time) {
            activeCooldowns[order.Name] = new Timer { commandName = order.Name, duration = time, timeRemaining = time };

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
            foreach (Commandlet order in commandQueue)
                order.CompleteCommand(this, true);

            commandQueue.Clear();

            if (Current != null)
                Current.CompleteCommand(this, true);

            Current = null;

            if (NetworkManager.Singleton.IsServer) ClearClientRpc();
        }

        [Rpc(SendTo.NotServer)]
        private void ClearClientRpc() {
            Clear();
        }

        public virtual bool CanCommand(string key) {
            return !activeCooldowns.ContainsKey(key);
        }

        public void Order(Commandlet order, bool inclusive) {
            if (inclusive)
                EnqueueCommand(order);
            else
                ExecuteCommand(order);
        }

        public CommandFactory Evaluate(ISelectable target) => throw new NotImplementedException();

        public void AutoCommand(ISelectable target) {
            throw new NotImplementedException();
        }

        public string[] Commands() => _commands;

        public CommandQueue Get() => this;
        public GameObject GameObject => gameObject;
    }

    public class Timer
    {
        public Commandlet command;
        public string commandName;
        public float duration;
        public float timeRemaining;
    }
}