using MarsTS.Events;
using MarsTS.Players.Input;
using MarsTS.Players.Teams;
using MarsTS.Units;
using MarsTS.Units.Commands;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

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

		[SerializeField]
		private float cameraSpeed;

		private Vector2 moveDirection;
		

		[SerializeField]
		private RectTransform selectionSquare;
		bool mouseHeld = false;
		private Vector2 drawStart;
		private Vector2 drawMouse;

		private void Awake () {
			instance = this;
			view = GetComponent<Camera>();
			inputController = GetComponent<InputHandler>();
			alternate = false;
		}

		private void Start () {
			
		}

		private void Update () {
			transform.position = transform.position + (cameraSpeed * Time.deltaTime * new Vector3(moveDirection.x, 0, moveDirection.y));
			if (mouseHeld) {
				drawMouse = cursorPos;

				Vector2 topLeft;
				Vector2 bottomRight;

				topLeft.x = drawMouse.x >= drawStart.x ? drawStart.x : drawMouse.x;
				topLeft.y = drawMouse.y >= drawStart.y ? drawMouse.y : drawStart.y;
				bottomRight.x = drawMouse.x >= drawStart.x ? drawMouse.x : drawStart.x;
				bottomRight.y = drawMouse.y >= drawStart.y ? drawStart.y : drawMouse.y;

				selectionSquare.offsetMin = new Vector2(topLeft.x, bottomRight.y);
				selectionSquare.offsetMax = new Vector2(bottomRight.x - Screen.width, topLeft.y - Screen.height);
			}
		}

		private void StartSelectionDraw () {
			drawStart = cursorPos;
			selectionSquare.gameObject.SetActive(true);
			mouseHeld = true;
		}

		private void EndSelectionDraw () {
			Vector3[] screenCorners = new Vector3[4];
			Vector3[] worldCorners = new Vector3[4];

			selectionSquare.GetWorldCorners(screenCorners);

			for (int i = 0; i < screenCorners.Length; i++) {
				worldCorners[i] = view.ScreenToWorldPoint(screenCorners[i]);
				Debug.Log(worldCorners[i]);
			}



			mouseHeld = false;
			selectionSquare.gameObject.SetActive(false);
		}

		/*	Input Functions	*/

		public void Move (InputAction.CallbackContext context) {
			moveDirection = context.ReadValue<Vector2>();
		}

		public void Look (InputAction.CallbackContext context) {
			cursorPos = context.ReadValue<Vector2>();
		}

		public void Select (InputAction.CallbackContext context) {switch (context.phase) {
				//Press Down
				case InputActionPhase.Started: {
					break;
				}

				//Hold Threshold
				//We want to start drawing the box here
				case InputActionPhase.Performed: {
					StartSelectionDraw();
					break;
				}

				//Release
				//If we've reached the held threshold, then we want to select everything within the drawn box
				//Otherwise its a normal click
				case InputActionPhase.Canceled: {
					if (mouseHeld) EndSelectionDraw();
					else {
						Ray ray = ViewPort.ScreenPointToRay(cursorPos);

						if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.SelectableMask)) {
							Unit hitUnit = hit.collider.gameObject.GetComponentInParent<ISelectable>().Get();
							SelectUnit(hitUnit);

							
						}
					}
					mouseHeld = false;
					break;
				}
			}
		}

		//If exclusive is true this will deselect every other selected unit
		private void SelectUnit (params Unit[] selection) {
			if (!Include) ClearSelection();
			foreach (Unit target in selection) {
				Roster units = GetRoster(target.Type());

				if (!units.TryAdd(target)) {
					units.Remove(target.Id());
					target.Select(false);
					if (units.Count == 0) selected.Remove(units.Type);
				}
				else {
					target.Select(true);
				}
			}

			EventBus.Post(new SelectEvent(this, true, Selected));
		}

		private void ClearSelection () {
			foreach (Roster units in selected.Values) {
				foreach (Unit unit in units.List()) {
					unit.Select(false);
				}

				units.Clear();
			}

			selected.Clear();
		}

		private Roster GetRoster (string type) {
			Roster map = selected.GetValueOrDefault(type, new Roster(type));
			if (!selected.ContainsKey(type)) selected.Add(type, map);
			return map;
		}

		public void Command (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = ViewPort.ScreenPointToRay(cursorPos);

				Physics.Raycast(ray, out RaycastHit walkableHit, 1000f, GameWorld.WalkableMask);
				Physics.Raycast(ray, out RaycastHit selectableHit, 1000f, GameWorld.SelectableMask);

				if (selectableHit.collider != null && selectableHit.collider.gameObject.TryGetComponent(out ISelectable unit)) {
					if (GetRelationship(unit.Get().Owner).Equals(Relationship.Hostile)) {
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

		private Mesh SelectionMesh (Vector3[] corners, Vector3[] vecs) {
			Vector3[] verts = new Vector3[8];
			int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 }; //map the tris of our cube

			for (int i = 0; i < 4; i++) {
				verts[i] = corners[i];
			}

			for (int j = 4; j < 8; j++) {
				verts[j] = corners[j - 4] + vecs[j - 4];
			}

			Mesh selectionMesh = new Mesh();
			selectionMesh.vertices = verts;
			selectionMesh.triangles = tris;

			return selectionMesh;
		}

		private void OnTriggerEnter (Collider other) {
			Debug.Log(other.name);
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}