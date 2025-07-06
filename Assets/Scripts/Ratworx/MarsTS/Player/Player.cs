using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Player.Input;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Player {

	public class Player : MonoBehaviour {
		public static Player Main { get; private set; }

		public static Faction Commander => Main._commander;
		private Faction _commander;

		public static Dictionary<string, Roster> Selected => Main._selected;
		private readonly Dictionary<string, Roster> _selected = new Dictionary<string, Roster>();
		
		public static List<int> ListSelected {
			get {
				var outputList = new List<int>();

				foreach (Roster typeRoster in Selected.Values) {
					foreach (Entity unit in typeRoster.List()) {
						outputList.Add(unit.Id);
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

		public static PlayerSelection Selection => Main._playerSelection;
		private PlayerSelection _playerSelection;

		private ISelectable _currentHover;

		private void Awake () {
			Main = this;

			_bus = GetComponent<EventAgent>();

			_inputController = GetComponent<InputHandler>();
			_uiController = GetComponent<UIController>();
			_cameraControls = GetComponent<ViewportController>();
			_playerSelection = GetComponent<PlayerSelection>();

			_view = GetComponentInChildren<Camera>();
		}

		private void Start () {
			EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);

			_alternate = false;
		}

		public void SetCommander(Faction commander) {
			_commander = commander;
			
			_bus.PostGlobal(new PlayerInitEvent());
		}

		private void Update () {
			Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
				if (EntityCache.TryGetEntityComponent(hit.transform.root.name, out ISelectable unit) && _currentHover != unit) {
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
				if (UI.IsHovering
					|| UI.IsDrawingSelection) return;
				if (!Include) _playerSelection.ClearSelection();

				Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
					ISelectable hitUnit = hit.collider.gameObject.GetComponentInParent<ISelectable>();
					_playerSelection.SelectUnits(hitUnit);
				}
			}
		}

		public static bool HasSelected (ISelectable unit) {
			if (Selected.TryGetValue(unit.Entity.RegistryKey, out Roster typeRoster)) {
				return typeRoster.Contains(unit.Entity.Id);
			}

			return false;
		}

		public void Command (InputAction.CallbackContext context) {
			if (context.canceled && !UI.IsHovering) {
				Ray ray = ViewPort.ScreenPointToRay(_cursorPos);

				Physics.Raycast(ray, out RaycastHit walkableHit, 1000f, GameWorld.WalkableMask);
				Physics.Raycast(ray, out RaycastHit selectableHit, 1000f, GameWorld.SelectableMask);

				if (selectableHit.collider != null && EntityCache.TryGetEntity(selectableHit.rigidbody.transform.name, out Entity targetEntity)) {
					if (!Selection.PrimarySelection.IsCommandable()) return;
					
					List<ICommandable> commandables = Selection.PrimarySelection.GetCommandables();
					ICommandInterface command = commandables[0].EvaluateCommand(targetEntity);
					CommandPrimer.GetFactory(command.CommandKey).TryConstructCommandFromEntity(
						command.CommandKey,
						targetEntity,
						Commander,
						ListSelected,
						Include
					);
				}
				else if (walkableHit.collider != null) {
					Vector3 hitPos = walkableHit.point;

					CommandPrimer.GetFactory<CommandFactory<Vector3>>("move").ConstructCommand(
						"move",
						hitPos,
						Commander,
						ListSelected,
						Include
					);
				}
			}
		}

		public void DeliverCommand(Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				foreach (Entity unit in entry.Value.List()) {
					if (!unit.TryGetEntityComponent(out ICommandable commandable)) continue;

					commandable.Order(packet, inclusive);
				}
			}
		}

		public void DistributeCommand (Commandlet packet, bool inclusive) {
			foreach (KeyValuePair<string, Roster> entry in Selected) {
				int lowestAmount = 999;
				ICommandable lowestOrderable = null;

				foreach (ICommandable orderable in entry.Value.GetCommandables()) {
					if (!orderable.CanCommand(packet.Command.Name)) continue;
					if (orderable.QueueCount < lowestAmount) {
						lowestAmount = orderable.QueueCount;
						lowestOrderable = orderable;
					}
				}

				if (lowestOrderable != null) {
					lowestOrderable.Order(packet, inclusive);
				}
			}
		}
		
		public void Next (InputAction.CallbackContext context) {
			if (!context.performed || _playerSelection.SelectedTypesCount <= 1) return;

			List<string> types = _playerSelection.SelectedTypes;
			int currentIndex = types.IndexOf(_playerSelection.PrimarySelection.RegistryKey);
			Roster newCurrent = _playerSelection.Selected[types[currentIndex + 1]];
			
			_playerSelection.SetPrimarySelection(newCurrent);
		}

		public void Alternate (InputAction.CallbackContext context) {
			if (context.performed) _alternate = true;
			if (context.canceled) _alternate = false;
		}

		private void OnEntityDeath (UnitDeathEvent evnt) {
			string key = evnt.Entity.RegistryKey;

			if (Selected.TryGetValue(key, out Roster unitRoster) && unitRoster.Contains(evnt.Entity.Id)) {
				unitRoster.Remove(evnt.Entity.Id);

				if (unitRoster.Count == 0) Selected.Remove(key);

				// TODO: Revisit this
				//This isn't the best method to update selection, as when units die we don't want the 
				//primary selected to be jumping around a lot, will have to come up with something better
				EventBus.Post(new PlayerSelectEvent(Selected));
			}
		}

		private void OnDestroy () {
			Main = null;
		}
	}
}