using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandQueue : MonoBehaviour {
        
        public Commandlet Current { get; protected set; }

        public Commandlet[] Queue { get { return commandQueue.ToArray(); } }
        protected Queue<Commandlet> commandQueue;

        public List<string> Active { get { return activeCommands.Keys.ToList();  } }
        protected Dictionary<string, Commandlet> activeCommands;

		public List<Cooldown> Cooldowns { get { return activeCooldowns.Values.ToList(); } }
		protected Dictionary<string, Cooldown> activeCooldowns;
		protected List<Cooldown> completedCooldowns;

		public int Count { get { return Current is not null ? 1 + commandQueue.Count : 0; } }

		protected ISelectable parent;
		protected ICommandable orderSource;
		protected EventAgent bus;

		protected virtual void Awake () {
			parent = GetComponent<ISelectable>();
			orderSource = parent as ICommandable;
			bus = GetComponent<EventAgent>();

			commandQueue = new Queue<Commandlet>();
			activeCommands = new Dictionary<string, Commandlet>();
			activeCooldowns = new Dictionary<string, Cooldown>();
			completedCooldowns = new List<Cooldown>();
		}

		protected virtual void Update () {
			if (Current is null && commandQueue.TryDequeue(out Commandlet order)) {
				Current = order;
				order.Callback.AddListener(OrderComplete);
				CommandStartEvent _event = new CommandStartEvent(bus, order, parent);
				order.OnStart(this, _event);
				bus.Local(_event);

				return;
			}

			if (Current is IWorkable workOrder) {
				workOrder.CurrentWork += Time.deltaTime;
				bus.Global(new CommandWorkEvent(bus, Current.Name, parent, workOrder));

				if (workOrder.CurrentWork >= workOrder.WorkRequired) {
					Current.OnComplete(this, new CommandCompleteEvent(bus, Current, false, parent));
				}
			}

			foreach (Cooldown timer in activeCooldowns.Values) {
				timer.timeRemaining -= Time.deltaTime;

				if (timer.timeRemaining <= 0) completedCooldowns.Add(timer);

				bus.Global(new CooldownEvent(bus, timer.commandName, parent, timer));
			}

			foreach (Cooldown toRemove in completedCooldowns) {
				activeCooldowns.Remove(toRemove.commandName);
				bus.Global(new CooldownEvent(bus, toRemove.commandName, parent, toRemove));
			}

			completedCooldowns = new();
		}

		protected virtual void OrderComplete (CommandCompleteEvent _event) {
			if (_event.Unit != parent) return;
			Current = null;
			bus.Global(_event);
		}

		public virtual void Execute (Commandlet order) {
			if (!orderSource.CanCommand(order.Command.Name)) return;
			commandQueue.Clear();

			if (Current != null) {
				if (!Current.CanInterrupt()) return;

				CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, true, parent);
				Current.OnComplete(this, _event);
			}

			Current = null;
			commandQueue.Enqueue(order);
		}

		public virtual void Enqueue (Commandlet order) {
			if (!orderSource.CanCommand(order.Command.Name)) return;
			commandQueue.Enqueue(order);
		}

		public virtual void Activate (Commandlet order, bool status) {
			if (status) {
				activeCommands[order.Name] = order;
				order.OnActivate(this, new CommandActiveEvent(bus, parent, order, status));
			}
			else if (activeCommands.TryGetValue(order.Name, out Commandlet toDeactivate)) {
				CommandActiveEvent _event = new CommandActiveEvent(bus, parent, toDeactivate, status);
				toDeactivate.OnActivate(this, _event);
				activeCommands.Remove(toDeactivate.Name);
			}

			bus.Global(new CommandActiveEvent(bus, parent, order, status));
		}

		public virtual void Deactivate (string key) {
			if (activeCommands.TryGetValue(key, out Commandlet toDeactivate)) {
				CommandActiveEvent _event = new CommandActiveEvent(bus, parent, toDeactivate, false);
				toDeactivate.OnActivate(this, _event);
				activeCommands.Remove(toDeactivate.Name);
			}
		}

		public virtual void Cooldown (Commandlet order, float time) {
			activeCooldowns[order.Name] = new Cooldown { commandName = order.Name, duration = time, timeRemaining = time };
		}

		public virtual void Clear () {
			foreach (Commandlet order in commandQueue) {
				order.OnComplete(this, new CommandCompleteEvent(bus, order, false, parent));
			}

			commandQueue.Clear();

			CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, false, parent);
			Current.OnComplete(this, _event);
			Current = null;
		}

		public virtual bool CanCommand (string key) {
			return !activeCooldowns.ContainsKey(key);
		}
	}

	public class Cooldown {
		public string commandName;
		public float duration;
		public float timeRemaining;
	}
}