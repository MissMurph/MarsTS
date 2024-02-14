using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Players {

    public class ViewportController : MonoBehaviour {

		[SerializeField]
		private float cameraSpeed;
		private float scaledCameraSpeed;

		/*	Zooming	*/
		[SerializeField]
		private float minHeight;
		[SerializeField]
		private float maxHeight;
		[SerializeField]
		private float targetHeight;
		private float currentHeight;

		[SerializeField]
		private float zoomTime;

		[SerializeField]
		private float zoomIncrement;

		private float zoomSpeed;

		private Vector2 moveDirection;

		private Transform cameraPos;
		private Transform gimbal;

		/*	Rotation	*/
		private bool rotating;

		private void Awake () {
			cameraPos = GetComponentInChildren<Camera>().transform;
			gimbal = transform.Find("CameraGimbal");

			currentHeight = Mathf.Abs(cameraPos.localPosition.z);
			targetHeight = currentHeight;
		}

		private void Update () {
			scaledCameraSpeed = cameraSpeed * (currentHeight / maxHeight);

			transform.position += (scaledCameraSpeed * Time.deltaTime * new Vector3(moveDirection.x, 0, moveDirection.y));

			/*	Update Gimbal	*/

			Ray groundRay = new Ray(transform.position, Vector3.down);

			if (Physics.Raycast(groundRay, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				Vector3 newGimbalPos = gimbal.position;

				//newGimbalPos.y = Mathf.SmoothDamp(gimbal.position.y, hit.point.y, ref zoomSpeed, zoomTime);

				newGimbalPos.y = hit.point.y;

				gimbal.transform.position = newGimbalPos;
			}

			/*	Update Cam	*/

			Vector3 newCamPos = cameraPos.localPosition;

			newCamPos.z = Mathf.SmoothDamp(cameraPos.localPosition.z, -targetHeight, ref zoomSpeed, zoomTime);

			currentHeight = -newCamPos.z;

			cameraPos.localPosition = newCamPos;
		}

		public void Move (InputAction.CallbackContext context) {
			moveDirection = context.ReadValue<Vector2>();
		}

		public void Scroll (InputAction.CallbackContext context) {
			targetHeight -= context.ReadValue<Vector2>().y * zoomIncrement;
			targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
		}

		public void Rotate (InputAction.CallbackContext context) {

		}

		public void Grab (InputAction.CallbackContext context) {

		}
	}
}