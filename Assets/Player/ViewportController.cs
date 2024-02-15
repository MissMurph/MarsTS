using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Players {

    public class ViewportController : MonoBehaviour {

		/*	Movement	*/
		[Header("Movement")]

		[SerializeField]
		private float cameraSpeed;
		private float scaledCameraSpeed;

		private Vector2 moveInput;

		private Vector3 forwardsMotion;
		private Vector3 horizontalMotion;

		/*	Panning	*/
		[Header("Movement")]

		[SerializeField]
		private float panSpeed;

		private bool panning;

		/*	Zooming	*/
		[Header("Zooming")]

		//x = minimum
		//y = maximum
		[SerializeField]
		private Vector2 zoomBounds;

		[SerializeField]
		private float zoomTime;

		[SerializeField]
		private float zoomIncrement;

		private float targetZoom;
		private float currentZoom;
		private float zoomSpeed;

		/*	Rotation	*/
		[Header("Rotation")]

		[SerializeField]
		private Vector2 sensitivity;

		//x = minimumm
		//y = maximum
		[SerializeField]
		private Vector2 verticalLimits;

		//x = horizontal default
		//y = vertical default
		[SerializeField]
		private Vector2 defaultAngles;

		private bool rotating;
		private Vector2 lookDelta;

		private float verticalAngle;
		private float horizontalAngle;

		/*	General	*/

		private Transform cameraPos;
		private Transform gimbal;

		private void Awake () {
			cameraPos = GetComponentInChildren<Camera>().transform;
			gimbal = transform.Find("CameraGimbal");

			currentZoom = Mathf.Abs(cameraPos.localPosition.z);
			targetZoom = currentZoom;
		}

		private void Update () {
			/*	Gimbal Height	*/

			Ray groundRay = new Ray(transform.position, Vector3.down);

			if (Physics.Raycast(groundRay, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
				Vector3 newGimbalPos = gimbal.position;

				//newGimbalPos.y = Mathf.SmoothDamp(gimbal.position.y, hit.point.y, ref zoomSpeed, zoomTime);

				newGimbalPos.y = hit.point.y;

				gimbal.transform.position = newGimbalPos;
			}

			/*	Rotation	*/

			if (rotating) {
				horizontalAngle += lookDelta.x * sensitivity.x;
				verticalAngle -= lookDelta.y * sensitivity.y;
			}
			else {
				horizontalAngle = defaultAngles.x;
				verticalAngle = defaultAngles.y;
			}

			verticalAngle = Mathf.Clamp(verticalAngle, verticalLimits.x, verticalLimits.y);
			
			transform.rotation = Quaternion.Euler(0, horizontalAngle, 0);
			gimbal.localRotation = Quaternion.Euler(verticalAngle, 0, 0);

			/*	Zooming	*/

			Vector3 newCamPos = cameraPos.localPosition;

			newCamPos.z = Mathf.SmoothDamp(cameraPos.localPosition.z, -targetZoom, ref zoomSpeed, zoomTime);

			currentZoom = -newCamPos.z;

			cameraPos.localPosition = newCamPos;

			/*	Movement	*/

			scaledCameraSpeed = cameraSpeed * (currentZoom / zoomBounds.y);

			if (panning) {
				forwardsMotion = -(transform.forward * lookDelta.y);
				horizontalMotion = -(transform.right * lookDelta.x);

				transform.position += panSpeed * Time.deltaTime * (forwardsMotion + horizontalMotion).normalized;
			}
			else {
				forwardsMotion = transform.forward * moveInput.y;
				horizontalMotion = transform.right * moveInput.x;

				transform.position += scaledCameraSpeed * Time.deltaTime * (forwardsMotion + horizontalMotion).normalized;
			}
		}

		public void Move (InputAction.CallbackContext context) {
			moveInput = context.ReadValue<Vector2>();
		}

		public void Scroll (InputAction.CallbackContext context) {
			targetZoom -= context.ReadValue<Vector2>().y * zoomIncrement;
			targetZoom = Mathf.Clamp(targetZoom, zoomBounds.x, zoomBounds.y);
		}

		public void Rotate (InputAction.CallbackContext context) {
			if (context.performed) rotating = true;
			if (context.canceled) rotating = false;
		}

		public void LookDelta (InputAction.CallbackContext context) {
			lookDelta = context.ReadValue<Vector2>();
		}

		public void Grab (InputAction.CallbackContext context) {
			if (context.performed) panning = true;
			if (context.canceled) panning = false;
		}
	}
}