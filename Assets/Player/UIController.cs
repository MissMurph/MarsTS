using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.Units.Commands;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings.SplashScreen;

namespace MarsTS.UI {

	public class UIController : MonoBehaviour {

		private static UIController instance;

		public static CommandPanel Command {
			get {
				return instance.commandPanel;
			}
		}

		private CommandPanel commandPanel;
		private UnitPane unitPane;

		private Dictionary<string, List<string>> commandProfiles;
		private string[] profileIndex;

		private int primaryIndex;

		[SerializeField]
		private RectTransform selectionSquare;
		[SerializeField]
		private GameObject selectionPrefab;

		private bool mouseHeld = false;
		private Vector2 drawStart;
		private Vector2 drawMouse;

		[SerializeField]
		private CursorSprite defaultCursor;

		private CursorSprite activeCursor;

		[SerializeField]
		private CursorSprite defaultCursorSprite;

		private string PrimarySelected {
			get {
				return primarySelected;
			}
			set {
				if (primarySelected != null) unitPane.Card(primarySelected).Selected = false;

				primarySelected = value;

				if (primarySelected != null) {
					unitPane.Card(primarySelected).Selected = true;
					commandPanel.UpdateCommands(commandProfiles[primarySelected].ToArray());
				}
				else commandPanel.UpdateCommands(new List<string> { }.ToArray());
			}
		}

		private string primarySelected;

		[SerializeField]
		private GraphicRaycaster canvasRaycaster;

		public bool Hovering {
			get {
				Vector2 mousePos = Player.MousePos;

				PointerEventData pointData = new PointerEventData(null);
				pointData.position = mousePos;
				List<RaycastResult> results = new List<RaycastResult>();

				canvasRaycaster.Raycast(pointData, results);

				return results.Count > 0;
			}
		}

		private void Awake () {
			instance = this;
			commandProfiles = new();
			ResetCursor();
		}

		private void Start () {
			commandPanel = GameObject.Find("Command Zone").GetComponent<CommandPanel>();
			unitPane = GameObject.Find("Unit Pane").GetComponent<UnitPane>();
			EventBus.AddListener<SelectEvent>(OnSelection);
		}

		private void Update () {
			if (mouseHeld) {
				drawMouse = Player.MousePos;

				if (!selectionSquare.gameObject.activeInHierarchy && Vector2.Distance(drawStart, drawMouse) > 2f) selectionSquare.gameObject.SetActive(true);
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

		public void ResetCursor () {
			activeCursor = null;
			Cursor.SetCursor(defaultCursor.texture, defaultCursor.target, CursorMode.Auto);
		}

		public void SetCursor (CursorSprite cursor) {
			activeCursor = cursor;

			Cursor.SetCursor(cursor.texture, cursor.target, CursorMode.Auto);
		}

		public void Select (InputAction.CallbackContext context) {
			switch (context.phase) {
				//Press Down
				case InputActionPhase.Started: {
					if (Hovering) return;
					drawStart = Player.MousePos;
					mouseHeld = true;
					break;
				}

				case InputActionPhase.Canceled: {
					if (selectionSquare.gameObject.activeSelf) {
						if (!Player.Include) Player.Main.ClearSelection();
						EndSelectionDraw();
						
					}

					mouseHeld = false;
					break;
				}
			}
		}

		public void Look (InputAction.CallbackContext context) {
			if (Hovering) {
				Cursor.SetCursor(defaultCursor.texture, defaultCursor.target, CursorMode.Auto);
				return;
			}

			Vector2 mousePos = context.ReadValue<Vector2>();

			Ray ray = Player.ViewPort.ScreenPointToRay(mousePos);

			//This is a temp method to get the cursor to reflect command. As more commands are added this'll be re-factored
			//to be modular
			if (Physics.Raycast(ray, out RaycastHit selectable, 1000f, GameWorld.SelectableMask)) {
				if (EntityCache.TryGet(selectable.collider.transform.parent.gameObject.name, out Unit target)) {
					Relationship allegiance = target.GetRelationship(Player.Main);

					switch (allegiance) {
						case Relationship.Friendly: {
							//We don't want to do anything here just yet
							break;
						}

						case Relationship.Hostile: {
							if (activeCursor == null) {
								CursorSprite sprite = Commands.Get("attack").Pointer;
								Cursor.SetCursor(sprite.texture, sprite.target, CursorMode.Auto);
							}
							
							return;
						}

						default: {
							//We don't want to do anything here just yet
							break;
						}
					}
				}
			}

			/*if (Physics.Raycast(ray, out RaycastHit ground, 1000f, GameWorld.WalkableMask)) {

			}*/

			if (activeCursor != null) {
				Cursor.SetCursor(activeCursor.texture, activeCursor.target, CursorMode.Auto);
			}
			else {
				Cursor.SetCursor(defaultCursor.texture, defaultCursor.target, CursorMode.Auto);
			}
		}

		private void EndSelectionDraw () {
			if (Vector2.Distance(drawStart, drawMouse) > 5f) {
				Vector3[] screenCorners = new Vector3[4];

				selectionSquare.GetWorldCorners(screenCorners);

				Vector3[] orderedCorners = new Vector3[4];

				Vector3[] rayVerts = new Vector3[4];
				Vector3[] dirVerts = new Vector3[4];

				orderedCorners[0] = screenCorners[1];
				orderedCorners[1] = screenCorners[2];
				orderedCorners[2] = screenCorners[0];
				orderedCorners[3] = screenCorners[3];

				for (int i = 0; i < orderedCorners.Length; i++) {
					Ray ray = Player.ViewPort.ScreenPointToRay(orderedCorners[i]);

					if (Physics.Raycast(ray, out RaycastHit hit, 50000f)) {
						rayVerts[i] = hit.point;
						dirVerts[i] = ray.origin - hit.point;
						//Debug.DrawLine(view.ScreenToWorldPoint(orderedCorners[i]), hit.point, Color.red, 1.0f);
					}
				}

				MeshCollider collider = Instantiate(selectionPrefab).GetComponent<MeshCollider>();
				collider.sharedMesh = SelectionMesh(rayVerts, dirVerts);
				Destroy(collider.gameObject, 0.2f);
			}

			selectionSquare.gameObject.SetActive(false);
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

		private void OnSelection (SelectEvent _event) {
			commandProfiles.Clear();
			profileIndex = new string[_event.Selected.Count];
			int index = 0;
			PrimarySelected = null;

			unitPane.UpdateUnits(_event.Selected);
			
			foreach (KeyValuePair<string, Roster> entry in _event.Selected) {
				List<string> availableCommands = new List<string>(entry.Value.Commands);
				commandProfiles.Add(entry.Key, availableCommands);
				profileIndex[index] = entry.Key;
				index++;
			}

			if (profileIndex.Length > 0) PrimarySelected = profileIndex[0];
			else PrimarySelected = null;
			primaryIndex = 0;
		}

		public void Next (InputAction.CallbackContext context) {
			if (context.performed && profileIndex.Length > 0) {
				int newIndex = primaryIndex >= profileIndex.Length - 1 ? 0 : primaryIndex + 1;
				if (profileIndex[newIndex] != null) {
					PrimarySelected = profileIndex[newIndex];
					primaryIndex = newIndex;
				}
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}