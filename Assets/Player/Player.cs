using MarsTS.Buildings;
using MarsTS.Commands;
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
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

namespace MarsTS.Players {

	public class Player : MonoBehaviour {

		public static Player Main { get { return instance; } }
		private static Player instance;

		public static Faction Commander { get { return instance.commander; } }
		private Faction commander;

		public static Dictionary<string, Roster> Selected { get { return instance.selected; } }
		private Dictionary<string, Roster> selected = new Dictionary<string, Roster>();
		
		public static List<string> ListSelected {
			get {
				List<string> outputList = new List<string>();

				foreach (Roster typeRoster in Selected.Values) {
					foreach (ISelectable unit in typeRoster.List()) {
						outputList.Add(unit.GameObject.name);
					}
				}

				return outputList;
			}
		}

		// TODO: Replace with utilizing the EntityCache
		/*public static NetworkObjectReference[] SerializedSelected {
			get {
				List<NetworkObjectReference> outputList = new List<NetworkObjectReference>();

				foreach (Roster typeRoster in Selected.Values) {
					foreach (ISelectable unit in typeRoster.List()) {
						if (EntityCache.TryGet(unit.GameObject.name, out NetworkObject foundNetworking)) {
							outputList.Add(foundNetworking);
						}
					}
				}

				return outputList.ToArray();
			}
		}*/

		public static Camera ViewPort { get { return instance.view; } }
		private Camera view;

		public static InputHandler Input { get { return instance.inputController; } }
		private InputHandler inputController;

		public static Vector2 MousePos { get { return instance.cursorPos; } }
		private Vector2 cursorPos;

		public static ViewportController PlayerControls { get { return instance.cameraControls; } }
		private ViewportController cameraControls;

		public static bool Include { get { return instance.alternate; } }
		private bool alternate;

		public static EventAgent EventAgent { get { return instance.bus; } }
		private EventAgent bus;

		public static UIController UI { get { return instance.uiController; } }
		private UIController uiController;

		public static List<IDepositable> Depositables { get { return instance.depositables; } }
		private List<IDepositable> depositables = new List<IDepositable>();

		private ISelectable currentHover;

		private void Awake () {
			instance = this;

			bus = GetComponentInParent<EventAgent>();
			commander = GetComponentInParent<Faction>();

			inputController = GetComponent<InputHandler>();
			uiController = GetComponent<UIController>();
			cameraControls = GetComponent<ViewportController>();

			view = GetComponentInChildren<Camera>();
		}

		private void Start () {
			EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);
			EventBus.AddListener<UnitOwnerChangeEvent>(OnUnitOwnershipChange);

			alternate = false;

			bus.Global(new PlayerInitEvent(bus));
		}

		private void Update () {
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

		public void Look (InputAction.CallbackContext context) {
			cursorPos = context.ReadValue<Vector2>();
		}

		public void Select (InputAction.CallbackContext context) {
			if (context.phase == InputActionPhase.Canceled) {
				if (UI.IsHovering) return;
				if (!Include) ClearSelection();

				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
					ISelectable hitUnit = hit.collider.gameObject.GetComponentInParent<ISelectable>();
					SelectUnit(hitUnit);
				}
			}
		}

		public bool HasSelected (ISelectable unit) {
			if (Selected.TryGetValue(unit.RegistryKey, out Roster typeRoster)) {
				return typeRoster.Contains(unit.ID);
			}

			return false;
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
			if (context.canceled && !UI.IsHovering) {
				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

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

					CommandRegistry.Get<Move>("move").Construct(hitPos, ListSelected);
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
			if (context.performed) alternate = true;
			if (context.canceled) alternate = false;
		}

		private void OnEntityDeath (UnitDeathEvent _event) {
			string key = _event.Unit.RegistryKey;

			if (Selected.TryGetValue(key, out Roster unitRoster) && unitRoster.Contains(_event.Unit.ID)) {
				unitRoster.Remove(_event.Unit.ID);

				if (unitRoster.Count == 0) Selected.Remove(key);

				//This isn't the best method to update selection, as when units die we don't want the 
				//primary selected to be jumping around a lot, will have to come up with something better
				EventBus.Global(new PlayerSelectEvent(Selected));
			}

			if (_event.Unit.Owner.Id == Commander.Id
				&& _event.Unit is IDepositable deserialized
				&& depositables.Contains(deserialized)) {
				depositables.Remove(deserialized);
			}
		}

		private void OnUnitOwnershipChange (UnitOwnerChangeEvent _event) {
			if (_event.Unit.Owner.Id == Commander.Id
				&& _event.Unit is IDepositable deserialized) {
				depositables.Add(deserialized);
			}
		}

		public static void SubmitResearch (Technology product) {
			Commander.SubmitResearch(product);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}