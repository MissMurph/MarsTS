using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MarsTS.UI {

    public class MiniMapInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

		private bool isMoving;
		private RawImage imageComp;

		private void Awake () {
			imageComp = GetComponent<RawImage>();
		}

		private void Update () {
			if (isMoving) {
				Vector3[] corners = new Vector3[4];
				imageComp.rectTransform.GetWorldCorners(corners);
				Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

				if (Player.MousePos.x < corners[0].x 
					|| Player.MousePos.x > corners[2].x 
					|| Player.MousePos.y < corners[0].y
					|| Player.MousePos.y > corners[2].y) {

					isMoving = false;
					return;
				}

				Vector2 newPos = Player.MousePos - new Vector2(corners[0].x, corners[0].y);

				Vector2 relativePos = newPos / (corners[2] - corners[0]);

				Debug.Log(relativePos);
			}
		}
		
		public void OnPointerDown (PointerEventData eventData) {
			isMoving = true;

			
		}

		public void OnPointerUp (PointerEventData eventData) {
			isMoving = false;
		}
	}
}