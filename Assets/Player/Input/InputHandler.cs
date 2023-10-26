using MarsTS.Events;
using MarsTS.Units;
using MarsTS.Units.Commands;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static MarsTS.Players.Input.InputHandler;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace MarsTS.Players.Input {

    public class InputHandler : MonoBehaviour {

		//private event Action<InputAction.CallbackContext> Move;

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

		private void Update () {
			
		}

		//This will clear the function of all events and then set new ones
		//This is a simple but effective way of quickly changing listeners
		private void SetListener (string name, ListenerBinding newListener) {
			if (inputs.TryGetValue(name, out InputAction action)) {
				active.TryGetValue(name, out ListenerBinding current);

				//We need to set to each individual phase event, so we quickly do a manual loop
				/*for (int i = 0; i < 3; i++) {
					switch (i) {
						case (0):
							action.started -= current.Function;
							action.started += newListener.Function;
							continue;
						case (1):
							action.performed -= current.Function;
							action.performed += newListener.Function;
							continue;
						case (2):
							action.canceled -= current.Function;
							action.canceled += newListener.Function;
							continue;
					}
				}*/

				
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

		/*public void PointerInput (InputAction.CallbackContext context) {
			if (active.TryGetValue(context.action.name, out ListenerBinding current)) {
				if ((context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed) && isHoveringUi) return;

				current.Function.Invoke(context);
			}
		}*/

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