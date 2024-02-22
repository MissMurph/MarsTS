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
		[SerializeField]
		private float movementInterpolation;
		private float scaledCameraSpeed;

		private Vector2 moveInput;

		private Vector3 forwardsMotion;
		private Vector3 horizontalMotion;

		private Vector3 newPosition;

		/*	Gimbal	*/
		private Transform gimbal;
		private float targetHeight;

		/*	Panning	*/
		[Header("Panning")]

		[SerializeField]
		private float panSpeed;

		private bool panning;
		private Vector3 panStart;
		private Vector3 panCurrent;

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

		[SerializeField]
		private float rotationInterpolation;

		private bool rotating;
		private Vector2 lookDelta;

		private float verticalAngle;
		private float horizontalAngle;

		//y axis
		private Quaternion yaw;
		//x axis
		private Quaternion pitch;

		/*	General	*/

		private Transform cameraPos;
		

		private void Awake () {
			cameraPos = GetComponentInChildren<Camera>().transform;
			gimbal = transform.Find("CameraGimbal");

			currentZoom = Mathf.Abs(cameraPos.localPosition.z);
			targetZoom = currentZoom;

			newPosition = transform.position;
		}

		private void Update () {
			UpdateGimbal();
			UpdateRotation();
			UpdateZoom();
			UpdatePosition();
		}

		private void UpdateGimbal () {
			Ray groundRay = new Ray(transform.position, Vector3.down);

			if (Physics.Raycast(groundRay, out RaycastHit gimbalHit, 1000f, GameWorld.WalkableMask)) {
				targetHeight = gimbalHit.point.y;
			}

			gimbal.transform.position = Vector3.Lerp(gimbal.transform.position, new Vector3(transform.position.x, targetHeight, transform.position.z), Time.deltaTime * movementInterpolation);
		}

		private void UpdateRotation () {
			if (rotating) {
				horizontalAngle += lookDelta.x * sensitivity.x;
				verticalAngle -= lookDelta.y * sensitivity.y;
			}
			else {
				horizontalAngle = defaultAngles.x;
				verticalAngle = defaultAngles.y;
			}

			verticalAngle = Mathf.Clamp(verticalAngle, verticalLimits.x, verticalLimits.y);

			yaw = Quaternion.Euler(0, horizontalAngle, 0);
			pitch = Quaternion.Euler(verticalAngle, 0, 0);

			transform.rotation = Quaternion.Lerp(transform.rotation, yaw, Time.deltaTime * rotationInterpolation);
			gimbal.localRotation = Quaternion.Lerp(gimbal.localRotation, pitch, Time.deltaTime * rotationInterpolation);
		}

		private void UpdateZoom () {
			Vector3 newCamPos = cameraPos.localPosition;

			newCamPos.z = Mathf.SmoothDamp(cameraPos.localPosition.z, -targetZoom, ref zoomSpeed, zoomTime);

			currentZoom = -newCamPos.z;

			cameraPos.localPosition = newCamPos;
		}

		private void UpdatePosition () {
			if (panning && !rotating) {
				Plane plane = new Plane(Vector3.up, Vector3.zero);

				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				float entry;

				if (plane.Raycast(ray, out entry)) {
					panCurrent = ray.GetPoint(entry);

					newPosition = transform.position + panStart - panCurrent;
				}
			}
			else {
				scaledCameraSpeed = cameraSpeed * (currentZoom / zoomBounds.y);

				forwardsMotion = transform.forward * moveInput.y;
				horizontalMotion = transform.right * moveInput.x;

				newPosition += (forwardsMotion + horizontalMotion).normalized * scaledCameraSpeed * Time.deltaTime;
			}

			newPosition.x = Mathf.Clamp(newPosition.x, -(GameWorld.WorldSize.x / 2), GameWorld.WorldSize.x / 2);
			newPosition.z = Mathf.Clamp(newPosition.z, -(GameWorld.WorldSize.y / 2), GameWorld.WorldSize.y / 2);

			transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementInterpolation);
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
			if (context.performed && !Player.UI.IsHovering) {
				Plane plane = new Plane(Vector3.up, Vector3.zero);

				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				float entry;

				if (plane.Raycast(ray, out entry)) {
					panStart = ray.GetPoint(entry);
					panning = true;
				}
			}
			if (context.canceled) panning = false;
		}
	}
}