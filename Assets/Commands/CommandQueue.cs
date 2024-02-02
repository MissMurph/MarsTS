using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandQueue : MonoBehaviour {
        
        public Commandlet Current { get; private set; }

        public Commandlet[] Queue { get { return commandQueue.ToArray(); } }
        protected Queue<Commandlet> commandQueue;

        public List<string> Active { get { return activeCommands.Keys.ToList();  } }
        protected Dictionary<string, Commandlet> activeCommands;

		public List<Cooldown> Cooldowns { get { return activeCooldowns.Values.ToList(); } }
		protected Dictionary<string, Cooldown> activeCooldowns;
		protected List<Cooldown> completedCooldowns;

		protected ISelectable parent;
		protected EventAgent bus;

		protected virtual void Awake () {
			parent = GetComponent<ISelectable>();
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
				order.OnStart(this, new CommandStartEvent(bus, order, parent));
				bus.Local(new CommandStartEvent(bus, order, parent));

				return;
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
			Current = null;
			bus.Global(_event);
		}

		public virtual void Execute (Commandlet order) {
			commandQueue.Clear();

			if (Current != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, true, parent);
				Current.OnComplete(this, _event);
				//bus.Global(_event);
			}

			Current = null;
			commandQueue.Enqueue(order);
		}

		public virtual void Enqueue (Commandlet order) {
			commandQueue.Enqueue(order);
		}

		public virtual void Activate (Commandlet order, bool status) {
			if (status) {
				activeCommands[order.Name] = order;
				order.OnStart(this, new CommandStartEvent(bus, order, parent));
			}
			else if (activeCommands.TryGetValue(order.Name, out Commandlet toDeactivate)) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, false, parent);
				toDeactivate.OnComplete(this, _event);
				activeCommands.Remove(order.Name);
			}

			bus.Global(new CommandActiveEvent(bus, parent, order, status));
		}

		public virtual void Cooldown (Commandlet order, float time) {
			activeCooldowns[order.Name] = new Cooldown { commandName = order.Name, duration = time, timeRemaining = time };
		}

		public virtual void Clear () {
			CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, false, parent);
			Current.OnComplete(this, _event);
			//bus.Global(_event);
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