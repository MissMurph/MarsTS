using System;
using MarsTS.Events;
using MarsTS.Networking;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Entities;
using MarsTS.Logging;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandQueue : NetworkBehaviour, ITaggable<CommandQueue>
    {
	    public virtual string Key => "commandQueue";
	    public Type Type => typeof(CommandQueue);
        
        public Commandlet Current { get; protected set; }

        public Commandlet[] Queue => commandQueue.ToArray();
        protected Queue<Commandlet> commandQueue;

        public List<string> Active { get { return activeCommands.Keys.ToList();  } }
		protected Dictionary<string, Commandlet> activeCommands;

		public List<Timer> Cooldowns { get { return activeCooldowns.Values.ToList(); } }
		protected Dictionary<string, Timer> activeCooldowns;
		protected List<Timer> completedCooldowns;

		public int Count { get { return Current != null ? 1 + commandQueue.Count : 0; } }

		protected ISelectable parent;
		protected ICommandable orderSource;
		protected EventAgent bus;

		protected bool isServer;

		private int workSpeed;
		private float workStepTime;
		private float workTimeToStep;

		protected virtual void Awake () {
			parent = GetComponent<ISelectable>();
			orderSource = parent as ICommandable;
			bus = GetComponent<EventAgent>();

			commandQueue = new Queue<Commandlet>();

			activeCommands = new Dictionary<string, Commandlet>();

			activeCooldowns = new Dictionary<string, Timer>();
			completedCooldowns = new List<Timer>();

			workStepTime = 1f / workSpeed;
			workTimeToStep = 0f;
		}

		public override void OnNetworkSpawn () 
		{
			base.OnNetworkSpawn();

			isServer = NetworkManager.IsServer;
		}

		protected virtual void Update () 
		{
			if (isServer && Current == null && commandQueue.Count > 0) {
				Dequeue();
				DequeueClientRpc(Current.gameObject);

				return;
			}

			if (isServer && Current is IWorkable workOrder) {
				workTimeToStep -= Time.deltaTime;

				if (workTimeToStep <= 0) {
					workOrder.CurrentWork++;
					workTimeToStep += workStepTime;

					bus.Global(new CommandWorkEvent(bus, Current, orderSource, workOrder));
					SendWorkEventToClientRpc();
				}

				if (workOrder.CurrentWork >= workOrder.WorkRequired) 
					CompleteCurrentCommand(false);
			}

			foreach (Timer cooldown in activeCooldowns.Values) {
				cooldown.timeRemaining -= Time.deltaTime;

				if (cooldown.timeRemaining <= 0) {
					completedCooldowns.Add(cooldown);
					continue;
				}

				bus.Global(new CooldownEvent(bus, cooldown.commandName, parent, cooldown));
			}

			foreach (Timer expiredCooldown in completedCooldowns) {
				activeCooldowns.Remove(expiredCooldown.commandName);
				bus.Global(new CooldownEvent(bus, expiredCooldown.commandName, parent, expiredCooldown));
			}

			completedCooldowns = new();
		}

		[Rpc(SendTo.NotServer)]
		private void SendWorkEventToClientRpc() {
			if (Current is IWorkable workOrder) {
				bus.Global(new CommandWorkEvent(bus, Current, orderSource, workOrder));
			}
			else
				RatLogger.Error?.Log($"Current command {Current.Name} is not {typeof(IWorkable)}! Cannot post work event");
		}

		/*	Dequeueing Commands	*/

		[Rpc(SendTo.NotServer)]
		protected virtual void DequeueClientRpc (NetworkObjectReference orderReference) {
			if (NetworkManager.IsHost) return;

			if (!ReferenceEquals(orderReference.GameObject(), commandQueue.Peek().gameObject)) {
				Debug.LogWarning("Potential Desync with client Command Queue! Check " + commandQueue.Peek().Name + "!");
			}

			Dequeue();
		}

		protected virtual void Dequeue () {
			Commandlet order = commandQueue.Dequeue();

			Current = order;
			order.Callback.AddListener(OnOrderComplete);

			if (order is IWorkable workable) 
				workable.OnWork += OnOrderWork;

			order.StartCommand(bus, orderSource);
		}

		/*	Completing Commands	*/

		[Rpc(SendTo.NotServer)]
		protected virtual void CompleteCommandClientRpc (bool _cancelled) {
			CompleteCurrentCommand(_cancelled);
		}

		protected virtual void CompleteCurrentCommand (bool _cancelled) 
		{
			Current.CompleteCommand(bus, orderSource, _cancelled);

			if (NetworkManager.Singleton.IsServer) 
				CompleteCommandClientRpc(_cancelled);
		}

		protected virtual void OnOrderComplete (CommandCompleteEvent _event) 
		{
			if (!ReferenceEquals(_event.Unit, orderSource)) return;
			Current = null;
			bus.Global(_event);
		}

		/*	Executing Commands	*/

		public virtual void Execute (Commandlet order) 
		{
			if (!orderSource.CanCommand(order.Command.Name)) return;
			commandQueue.Clear();

			if (Current != null) 
			{
				if (!Current.CanInterrupt()) return;

				Current.CompleteCommand(bus, orderSource, true);
			}

			Current = null;
			commandQueue.Enqueue(order);

			if (NetworkManager.Singleton.IsServer) ExecuteClientRpc(order.gameObject);
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void ExecuteClientRpc (NetworkObjectReference orderReference) 
		{
			if (NetworkManager.Singleton.IsHost) return;

			Execute(orderReference.GameObject().GetComponent<Commandlet>());
		}

		/*	Enqueueing Commands	*/

		public virtual void Enqueue (Commandlet order) {
			if (!orderSource.CanCommand(order.Command.Name)) return;
			commandQueue.Enqueue(order);

			if (NetworkManager.Singleton.IsServer) EnqueueClientRpc(order.gameObject);
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void EnqueueClientRpc (NetworkObjectReference orderReference) {
			if (NetworkManager.Singleton.IsHost) return;

			Enqueue(orderReference.GameObject().GetComponent<Commandlet>());
		}

		/*	Activating Commands	*/

		public void Activate (Commandlet order, bool status) {
			if (status) {
				activeCommands[order.Name] = order;
				order.ActivateCommand(this, new CommandActiveEvent(bus, orderSource, order, status));
			}
			else if (activeCommands.TryGetValue(order.Name, out Commandlet toDeactivate)) {
				CommandActiveEvent _event = new CommandActiveEvent(bus, orderSource, toDeactivate, status);
				toDeactivate.ActivateCommand(this, _event);
				activeCommands.Remove(toDeactivate.Name);
			}

			bus.Global(new CommandActiveEvent(bus, orderSource, order, status));

			if (NetworkManager.Singleton.IsServer)
				ActivateClientRpc(order.Id, status);
		}

		[Rpc(SendTo.NotServer)]
		private void ActivateClientRpc(int id, bool status) {
			if (!CommandletsCache.TryGet(id, out Commandlet order)) {
				RatLogger.Error?.Log($"Couldn't find commandlet {id}! Cannot activate");
				return;
			}
			
			Activate(order, status);
		}

		public void Deactivate (string key) {
			if (!activeCommands.TryGetValue(key, out Commandlet toDeactivate)) return;
			
			CommandActiveEvent _event = new CommandActiveEvent(bus, orderSource, toDeactivate, false);
			toDeactivate.ActivateCommand(this, _event);
			activeCommands.Remove(toDeactivate.Name);
			bus.Global(_event);

			if (NetworkManager.Singleton.IsServer) 
				DeactivateClientRpc(key);
		}

		[Rpc(SendTo.NotServer)]
		private void DeactivateClientRpc(string key) {
			Deactivate(key);
		}

		/*	Cooldowns	*/

		public void Cooldown (Commandlet order, float time) {
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

		public void Clear () {
			foreach (Commandlet order in commandQueue) 
				order.CompleteCommand(bus, orderSource, true);

			commandQueue.Clear();

			if (Current != null) 
				Current.CompleteCommand(bus, orderSource, true);

			Current = null;

			if (NetworkManager.Singleton.IsServer) ClearClientRpc();
		}

		[Rpc(SendTo.NotServer)]
		private void ClearClientRpc() {
			Clear();
		}

		public virtual bool CanCommand (string key) {
			return !activeCooldowns.ContainsKey(key);
		}

		protected virtual void OnOrderWork (int oldValue, int newValue) {
			if (Current is not IWorkable workOrder) return;

			if (workOrder.CurrentWork >= workOrder.WorkRequired) {
				workOrder.OnWork -= OnOrderWork;
				CompleteCurrentCommand(false);
			}
			else
				bus.Global(new WorkEvent(bus, parent, workOrder.WorkRequired, workOrder.CurrentWork));
		}

		public CommandQueue Get() => this;
    }

	public class Timer {
		public string commandName;
		public float duration;
		public float timeRemaining;
	}
}