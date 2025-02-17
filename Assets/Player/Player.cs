using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players.Input;
using MarsTS.Research;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Commands;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace MarsTS.Players {

	public class Player : MonoBehaviour {
		public static Player Main { get; private set; }

		public static Faction Commander => Main._commander;
		private Faction _commander;

		public static Dictionary<string, Roster> Selected => Main._selected;
		private readonly Dictionary<string, Roster> _selected = new Dictionary<string, Roster>();
		
		public static List<string> ListSelected {
			get {
				var outputList = new List<string>();

				foreach (Roster typeRoster in Selected.Values) {
					foreach (ISelectable unit in typeRoster.List()) {
						outputList.Add(unit.GameObject.name);
					}
				}

				return outputList;
			}
		}

		public static Camera ViewPort => Main._view;
		private Camera _view;

		public static InputHandler Input => Main._inputController;
		private InputHandler _inputController;

		public static Vector2 MousePos => Main._cursorPos;
		private Vector2 _cursorPos;

		public static ViewportController PlayerControls => Main._cameraControls;
		private ViewportController _cameraControls;

		public static bool Include => Main._alternate;
		private bool _alternate;

		public static EventAgent EventAgent => Main._bus;
		private EventAgent _bus;

		public static UIController UI => Main._uiController;
		private UIController _uiController;

		private ISelectable _currentHover;

		private void Awake () {
			Main = this;

			_bus = GetComponent<EventAgent>();

			_inputController = GetComponent<InputHandler>();
			_uiController = GetComponent<UIController>();
			_cameraControls = GetComponent<ViewportController>();

			_view = GetComponentInChildren<Camera>();
		}

		private void Start () {
			EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);

			_alternate = false;
		}

		public void SetCommander(Faction commander) {
			_commander = commander;
			
			_bus.Global(new PlayerInitEvent(_bus));
		}

		private void Update () {
			Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
				if (EntityCache.TryGet(hit.transform.root.name, out ISelectable unit) && _currentHover != unit) {
					if (_currentHover != null) _currentHover.Hover(false);
					_currentHover = unit;
					unit.Hover(true);
				}
			}
			else if (_currentHover != null) {
				_currentHover.Hover(false);
				_currentHover = null;
			}
		}

		/*	Input Functions	*/

		public void Look (InputAction.CallbackContext context) {
			_cursorPos = context.ReadValue<Vector2>();
		}

		public void Select (InputAction.CallbackContext context) {
			if (context.phase == InputActionPhase.Canceled) {
				if (UI.IsHovering) return;
				if (!Include) ClearSelection();

				Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
					ISelectable hitUnit = hit.collider.gameObject.GetComponentInParent<ISelectable>();
					SelectUnit(hitUnit);
				}
			}
		}

		public bool HasSelected (ISelectable unit) {
			if (Selected.TryGetValue(unit.RegistryKey, out Roster typeRoster)) {
				return typeRoster.Contains(unit.Id);
			}

			return false;
		}

		public void SelectUnit (params ISelectable[] selection) {
			foreach (ISelectable target in selection) {
				Roster units = GetRoster(target.RegistryKey);

				if (!units.TryAdd(target)) {
					units.Remove(target.Id);
					target.Select(false);
					if (units.Count == 0) _selected.Remove(units.RegistryKey);
				}
				else {
					target.Select(true);
				}
			}

			EventBus.Global(new PlayerSelectEvent(Selected));
		}

		public void ClearSelection () {
			foreach (Roster units in _selected.Values) {
				foreach (ISelectable unit in units.List()) {
					unit.Select(false);
				}

				units.Clear();
			}

			_selected.Clear();
			EventBus.Global(new PlayerSelectEvent(Selected));
		}

		private Roster GetRoster (string key) {
			Roster map = _selected.GetValueOrDefault(key, new Roster());
			_selected.TryAdd(key, map);
			return map;
		}

		public void Command (InputAction.CallbackContext context) {
			if (context.canceled && !UI.IsHovering) {
				Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

				Physics.Raycast(ray, out RaycastHit walkableHit, 1000f, GameWorld.WalkableMask);
				Physics.Raycast(ray, out RaycastHit selectableHit, 1000f, GameWorld.SelectableMask);

				if (selectableHit.collider != null && EntityCache.TryGet(selectableHit.collider.transform.root.name, out ISelectable target)) {
					if (Selected[UIController.instance.PrimarySelected].Get() is ICommandable commandable) {
						commandable.AutoCommand(target);
						return;
					}
				}
				else if (walkableHit.collider != null) {
					Vector3 hitPos = walkableHit.point;

					CommandPrimer.Get<Move>("move").Construct(hitPos);
					return;
				}
			}
		}

		public void DeliverCommand (Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				foreach (ISelectable unit in entry.Value.List()) {
					if (unit is ICommandable orderable) {
						orderable.Order(packet, inclusive);
					}
				}
			}
		}

		public void DistributeCommand (Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				int lowestAmount = 999;
				ICommandable lowestOrderable = null;

				foreach (ICommandable orderable in entry.Value.Orderable) {
					if (!orderable.CanCommand(packet.Command.Name)) continue;
					if (orderable.Count < lowestAmount) {
						lowestAmount = orderable.Count;
						lowestOrderable = orderable;
					}
				}

				if (lowestOrderable != null) {
					lowestOrderable.Order(packet, inclusive);
				}
			}
		}

		public void Alternate (InputAction.CallbackContext context) {
			if (context.performed) _alternate = true;
			if (context.canceled) _alternate = false;
		}

		private void OnEntityDeath (UnitDeathEvent _event) {
			string key = _event.Unit.RegistryKey;

			if (Selected.TryGetValue(key, out Roster unitRoster) && unitRoster.Contains(_event.Unit.Id)) {
				unitRoster.Remove(_event.Unit.Id);

				if (unitRoster.Count == 0) Selected.Remove(key);

				// TODO: Revisit this
				//This isn't the best method to update selection, as when units die we don't want the 
				//primary selected to be jumping around a lot, will have to come up with something better
				EventBus.Global(new PlayerSelectEvent(Selected));
			}
		}

		private void OnDestroy () {
			Main = null;
		}
	}
}