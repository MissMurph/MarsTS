using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.UI {

    public abstract class UnitBar : MonoBehaviour {

		protected MeshRenderer _barRenderer;

		private MaterialPropertyBlock _matBlock;

		private static readonly int FillShaderProperty = Shader.PropertyToID("_Fill");

		protected void UpdateBarWithFillLevel(float value)
		{
			_barRenderer.GetPropertyBlock(_matBlock);
			_matBlock.SetFloat(FillShaderProperty, value);
			_barRenderer.SetPropertyBlock(_matBlock);
		}

		protected virtual void Awake () {
			_barRenderer = GetComponent<MeshRenderer>();
			_matBlock = new MaterialPropertyBlock();
		}

		protected virtual void Update () {
			Transform camera = Camera.main.transform;
			Vector3 direction = transform.position - camera.position;

			direction.Normalize();

			Vector3 up = Vector3.Cross(direction, camera.right);

			transform.rotation = Quaternion.LookRotation(direction, up);
		}
	}
}