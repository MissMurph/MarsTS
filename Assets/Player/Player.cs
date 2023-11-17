using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players.Input;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
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

		private ISelectable currentHover;

		private void Awake () {
			instance = this;
			view = GetComponentInChildren<Camera>();
			inputController = GetComponent<InputHandler>();
			eventAgent = GetComponent<EventAgent>();
			uiController = GetComponent<UIController>();
			alternate = false;
		}

		private void Start () {
			EventBus.AddListener<EntityDeathEvent>(OnEntityDeath);
		}

		private void Update () {
			transform.position = transform.position + (cameraSpeed * Time.deltaTime * new Vector3(moveDirection.x, 0, moveDirection.y));

			Ray ray = ViewPort.ScreenPointToRay(cursorPos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
				if (EntityCache.TryGet(hit.transform.root.name, out ISelectable unit) && currentHover != unit) {
					if (currentHover != null) currentHover.Hover(false);
					currentHover = unit;
					unit.Hover(true);
				}
			}
			else if (currentHover != null) {
				currentHover.Hover(false);
				currentHover = null;
			}
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

		public bool HasSelected (ISelectable unit) {
			Roster units = GetRoster(unit.RegistryKey);

			return units.Contains(unit.ID);
		}

		public void SelectUnit (params ISelectable[] selection) {
			foreach (ISelectable target in selection) {
				Roster units = GetRoster(target.RegistryKey);

				if (!units.TryAdd(target)) {
					units.Remove(target.ID);
					target.Select(false);
					if (units.Count == 0) selected.Remove(units.RegistryKey);
				}
				else {
					target.Select(true);
				}
			}

			EventBus.Global(new PlayerSelectEvent(Selected));
		}

		public void ClearSelection () {
			foreach (Roster units in selected.Values) {
				foreach (ISelectable unit in units.List()) {
					unit.Select(false);
				}

				units.Clear();
			}

			selected.Clear();
			EventBus.Global(new PlayerSelectEvent(Selected));
		}

		private Roster GetRoster (string key) {
			Roster map = selected.GetValueOrDefault(key, new Roster());
			if (!selected.ContainsKey(key)) selected.Add(key, map);
			return map;
		}

		public void Command (InputAction.CallbackContext context) {
			if (context.canceled && !UI.Hovering) {
				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

				Physics.Raycast(ray, out RaycastHit walkableHit, 1000f, GameWorld.WalkableMask);
				Physics.Raycast(ray, out RaycastHit selectableHit, 1000f, GameWorld.SelectableMask);

				if (selectableHit.collider != null && EntityCache.TryGet(selectableHit.collider.transform.root.name, out ISelectable target)) {
					if (Selected[UIController.instance.PrimarySelected].Get() is ICommandable commandable) {
						Commandlet constructed = commandable.Auto(target);
						DeliverCommand(constructed, Include);
						return;
					}
				}
				else if (walkableHit.collider != null) {
					Vector3 hitPos = walkableHit.point;

					DeliverCommand(CommandRegistry.Get<Move>("move").Construct(hitPos), Include);
					return;
				}
			}
		}

		public void DeliverCommand (Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				foreach (ISelectable unit in entry.Value.List()) {
					if (unit is ICommandable orderable) {
						if (inclusive) orderable.Enqueue(packet);
						else orderable.Execute(packet);
					}
				}
			}
		}

		public void Alternate (InputAction.CallbackContext context) {
			if (context.performed) alternate = true;
			if (context.canceled) alternate = false;
		}

		private void OnEntityDeath (EntityDeathEvent _event) {
			string key = _event.Unit.RegistryKey;

			if (Selected.TryGetValue(key, out Roster unitRoster) && unitRoster.Contains(_event.Unit.ID)) {
				unitRoster.Remove(_event.Unit.ID);

				//This isn't the best method to update selection, as when units die we don't want the 
				//primary selected to be jumping around a lot, will have to come up with something better
				EventBus.Global(new PlayerSelectEvent(Selected));
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}