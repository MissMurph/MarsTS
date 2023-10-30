﻿using MarsTS.Events;
using MarsTS.Players.Input;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.Units.Commands;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Players {

	public class Player : Faction {

		public static Player Main { get { return instance; } }
		private static Player instance;

		public static Dictionary<string, Roster> Selected { get { return instance.selected; } }
		private Dictionary<string, Roster> selected = new Dictionary<string, Roster>();

		public static Camera ViewPort { get { return instance.view; } }
		private Camera view;

		public static InputHandler Input { get { return instance.inputController; } }
		private InputHandler inputController;

		public static Vector2 MousePos { get { return instance.cursorPos; } }
		private Vector2 cursorPos;

		public static bool Include { get { return instance.alternate; } }
		private bool alternate;

		public static EventAgent EventAgent { get { return instance.eventAgent; } }
		private EventAgent eventAgent;

		public static UIController UI { get { return instance.uiController; } }
		private UIController uiController;

		[SerializeField]
		private float cameraSpeed;

		private Vector2 moveDirection;

		private void Awake () {
			instance = this;
			view = GetComponent<Camera>();
			inputController = GetComponent<InputHandler>();
			eventAgent = GetComponent<EventAgent>();
			uiController = GetComponent<UIController>();
			alternate = false;
		}

		private void Update () {
			transform.position = transform.position + (cameraSpeed * Time.deltaTime * new Vector3(moveDirection.x, 0, moveDirection.y));
		}

		/*	Input Functions	*/

		public void Move (InputAction.CallbackContext context) {
			moveDirection = context.ReadValue<Vector2>();
		}

		public void Look (InputAction.CallbackContext context) {
			cursorPos = context.ReadValue<Vector2>();
		}

		public void Select (InputAction.CallbackContext context) {
			if (context.phase == InputActionPhase.Canceled) {
				if (UI.Hovering) return;
				if (!Include) ClearSelection();

				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
					ISelectable hitUnit = hit.collider.gameObject.GetComponentInParent<ISelectable>();
					SelectUnit(hitUnit);
				}
			}
		}

		public void SelectUnit (params ISelectable[] selection) {
			foreach (ISelectable target in selection) {
				Roster units = GetRoster(target.Name());

				if (!units.TryAdd(target)) {
					units.Remove(target.ID);
					target.Select(false);
					if (units.Count == 0) selected.Remove(units.Type);
				}
				else {
					target.Select(true);
				}
			}

			EventBus.Global(new SelectEvent(Selected));
		}

		public void ClearSelection () {
			foreach (Roster units in selected.Values) {
				foreach (Unit unit in units.List()) {
					unit.Select(false);
				}

				units.Clear();
			}

			selected.Clear();
			EventBus.Global(new SelectEvent(Selected));
		}

		private Roster GetRoster (string type) {
			Roster map = selected.GetValueOrDefault(type, new Roster(type));
			if (!selected.ContainsKey(type)) selected.Add(type, map);
			return map;
		}

		public void Command (InputAction.CallbackContext context) {
			if (context.canceled && !UI.Hovering) {
				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

				Physics.Raycast(ray, out RaycastHit walkableHit, 1000f, GameWorld.WalkableMask);
				Physics.Raycast(ray, out RaycastHit selectableHit, 1000f, GameWorld.SelectableMask);

				if (selectableHit.collider != null && selectableHit.collider.gameObject.TryGetComponent(out ISelectable unit)) {
					if (unit.GetRelationship(this) == Relationship.Hostile) {
						DeliverCommand(Commands.Get<Attack>("attack").Construct(unit), Include);
						return;
					}
				}
				else if (walkableHit.collider != null) {
					Vector3 hitPos = walkableHit.point;

					DeliverCommand(Commands.Get<Move>("move").Construct(hitPos), Include);
					return;
				}
			}
		}

		public void DeliverCommand (Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				foreach (ISelectable unit in entry.Value.List()) {
					if (inclusive) unit.Enqueue(packet);
					else unit.Execute(packet);
				}
			}
		}

		public void Alternate (InputAction.CallbackContext context) {
			if (context.performed) alternate = true;
			if (context.canceled) alternate = false;
		}

		private void OnTriggerEnter (Collider other) {
			Debug.Log(other.name);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}