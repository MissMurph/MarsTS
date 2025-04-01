using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Player.Input {

    public class InputHandler : MonoBehaviour {

		public InputActionReference[] inputsToBind;
		private Dictionary<string, InputAction> inputs;

		public DefaultEntry[] defaultsToBind;
		private Dictionary<string, DefaultEntry> defaults;
		private Dictionary<string, ListenerBinding> active;

		private void Awake () {
			inputs = new Dictionary<string, InputAction>();
			defaults = new Dictionary<string, DefaultEntry>();
			active = new Dictionary<string, ListenerBinding>();

			foreach (InputAction action in inputsToBind) {
				inputs.Add(action.name, action);
			}

			foreach (DefaultEntry def in defaultsToBind) {
				defaults.Add(def.Action.name, def);
				active.Add(def.Action.name, def);
				//Debug.Log(def.Action.name);
			}
		}

		public void Hook (string name, Action<InputAction.CallbackContext> function) {
			if (inputs.TryGetValue(name, out InputAction _action)) {
				ListenerBinding hook = new ListenerBinding() { Action = _action, Function = function };
				//SetListener(name, hook);
				active.Remove(name);
				active.Add(name, hook);
			}
		}

		public void Release (string name) {
			if (inputs.TryGetValue(name, out InputAction _action) && defaults.TryGetValue(name, out DefaultEntry entry)) {
				//SetListener(name, entry);
				active.Remove(name);
				active.Add(name, entry);
			}
		}

		//The single function the Input System will initially communicate through
		//Do not use this
		public void Input (InputAction.CallbackContext context) {
			if (active.TryGetValue(context.action.name, out ListenerBinding current)) {
				current.Function.Invoke(context);
			}
		}

		public void PointerInput (InputAction.CallbackContext context) {
			if (active.TryGetValue(context.action.name, out ListenerBinding current)) {
				if (UIController.Hovering) return;

				current.Function.Invoke(context);
			}
		}

		[Serializable]
		public class DefaultEntry : ListenerBinding {
			[SerializeField] 
			private InputActionReference action;
			[SerializeField] 
			private UnityEvent<InputAction.CallbackContext> target;
			public override InputAction Action { get { return action.action; } }

			public DefaultEntry () {
				Function = (context) => target.Invoke(context);
			}
		}

		public class ListenerBinding {
			public virtual InputAction Action { get; set; }
			public Action<InputAction.CallbackContext> Function { get; set; }
		}
	}
}