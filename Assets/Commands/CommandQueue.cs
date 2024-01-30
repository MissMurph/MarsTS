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

        public Commandlet[] Active { get { return activeCommands.ToArray();  } }
        protected List<Commandlet> activeCommands;

		protected ISelectable parent;
		protected EventAgent bus;

		protected virtual void Awake () {
			parent = GetComponent<ISelectable>();
			bus = GetComponent<EventAgent>();

			commandQueue = new Queue<Commandlet>();
			activeCommands = new List<Commandlet>();
		}

		protected virtual void Update () {
			if (Current is null && commandQueue.TryDequeue(out Commandlet order)) {
				Current = order;
				order.Callback.AddListener(OrderComplete);
				bus.Local(new CommandStartEvent(bus, order));

				return;
			}

			
		}

		protected virtual void OrderComplete (CommandCompleteEvent _event) {
			Current = null;
			bus.Global(_event);
		}

		public virtual void Execute (Commandlet order) {
			commandQueue.Clear();

			if (Current != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, true, parent);
				Current.Callback.Invoke(_event);
				bus.Global(_event);
			}

			Current = null;
			commandQueue.Enqueue(order);
		}

		public virtual void Enqueue (Commandlet order) {
			commandQueue.Enqueue(order);
		}

		public virtual void Activate (Commandlet order) {

		}

		public virtual void Clear () {
			CommandCompleteEvent _event = new CommandCompleteEvent(bus, Current, false, parent);
			bus.Global(_event);
			Current = null;
		}
	}
}