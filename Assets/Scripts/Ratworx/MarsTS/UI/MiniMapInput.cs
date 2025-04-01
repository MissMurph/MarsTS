using Ratworx.MarsTS.Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI {

    public class MiniMapInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

		private bool isMoving;
		private RawImage imageComp;

		[SerializeField]
		private Camera mapCam;

		private void Awake () {
			imageComp = GetComponent<RawImage>();
		}

		private void Update () {
			if (isMoving) {
				Vector3[] corners = new Vector3[4];
				imageComp.rectTransform.GetWorldCorners(corners);
				Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

				if (Player.Player.MousePos.x < corners[0].x 
					|| Player.Player.MousePos.x > corners[2].x 
					|| Player.Player.MousePos.y < corners[0].y
					|| Player.Player.MousePos.y > corners[2].y) {

					isMoving = false;
					return;
				}

				Vector2 newPos = Player.Player.MousePos - new Vector2(corners[0].x, corners[0].y);

				Vector2 relativePos = newPos / (corners[2] - corners[0]);

				Ray ray = mapCam.ViewportPointToRay(relativePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f,GameWorld.EnvironmentMask)) {
					Player.Player.PlayerControls.TargetPosition = new Vector3(hit.point.x, Player.Player.Main.transform.position.y, hit.point.z);
				}
			}
		}
		
		public void OnPointerDown (PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Left) {
				isMoving = true;
			}
		}

		public void OnPointerUp (PointerEventData eventData) {
			if (eventData.button == PointerEventData.InputButton.Right) {
				Vector3[] corners = new Vector3[4];
				imageComp.rectTransform.GetWorldCorners(corners);
				Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

				if (Player.Player.MousePos.x < corners[0].x
					|| Player.Player.MousePos.x > corners[2].x
					|| Player.Player.MousePos.y < corners[0].y
					|| Player.Player.MousePos.y > corners[2].y) {

					isMoving = false;
					return;
				}

				Vector2 newPos = Player.Player.MousePos - new Vector2(corners[0].x, corners[0].y);

				Vector2 relativePos = newPos / (corners[2] - corners[0]);

				Ray ray = mapCam.ViewportPointToRay(relativePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					//Player.Main.DeliverCommand(CommandRegistry.Get<Move>("move").Construct(hit.point), Player.Include);
				}
			}

			isMoving = false;
		}
	}
}