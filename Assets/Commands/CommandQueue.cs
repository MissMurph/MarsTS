using System;
using MarsTS.Events;
using MarsTS.Networking;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Entities;
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

		protected virtual void Awake () {
			parent = GetComponent<ISelectable>();
			orderSource = parent as ICommandable;
			bus = GetComponent<EventAgent>();

			commandQueue = new Queue<Commandlet>();

			activeCommands = new Dictionary<string, Commandlet>();

			activeCooldowns = new Dictionary<string, Timer>();
			completedCooldowns = new List<Timer>();
		}

		public override void OnNetworkSpawn () 
		{
			base.OnNetworkSpawn();

			isServer = NetworkManager.IsServer;
		}

		protected virtual void Update () 
		{
			//if (!isServer) return;

			if (isServer && Current == null && commandQueue.Count > 0) {
				Dequeue();
				DequeueClientRpc(Current.gameObject);

				return;
			}

			if (isServer && Current is IWorkable workOrder) {
				workOrder.CurrentWork += Time.deltaTime;
				bus.Global(new CommandWorkEvent(bus, Current, orderSource, workOrder));

				if (workOrder.CurrentWork >= workOrder.WorkRequired) {
					CompleteCurrentCommand(false);
					//CompleteCommandClientRpc(false);
				}
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

		public virtual void Activate (Commandlet order, bool status) {
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
		}

		public virtual void Deactivate (string key) {
			if (activeCommands.TryGetValue(key, out Commandlet toDeactivate)) {
				CommandActiveEvent _event = new CommandActiveEvent(bus, orderSource, toDeactivate, false);
				toDeactivate.ActivateCommand(this, _event);
				activeCommands.Remove(toDeactivate.Name);
				bus.Global(_event);
			}
		}

		/*	Cooldowns	*/

		public virtual void Cooldown (Commandlet order, float time) {
			activeCooldowns[order.Name] = new Timer { commandName = order.Name, duration = time, timeRemaining = time };
		}

		/*	Misc.	*/

		public virtual void Clear () {
			foreach (Commandlet order in commandQueue) 
				order.CompleteCommand(bus, orderSource, true);

			commandQueue.Clear();

			if (Current != null) 
				Current.CompleteCommand(bus, orderSource, true);

			Current = null;
		}

		public virtual bool CanCommand (string key) {
			return !activeCooldowns.ContainsKey(key);
		}

		protected virtual void OnOrderWork (int oldValue, int newValue) {

		}

		public CommandQueue Get() => this;
    }

	public class Timer {
		public string commandName;
		public float duration;
		public float timeRemaining;
	}
}