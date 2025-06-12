using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Bars {

    public abstract class UnitBar : MonoBehaviour {

		protected MeshRenderer BarRenderer;

		private MaterialPropertyBlock _matBlock;

		private static readonly int FillShaderProperty = Shader.PropertyToID("_Fill");

		protected virtual void Awake () {
			BarRenderer = GetComponent<MeshRenderer>();
			_matBlock = new MaterialPropertyBlock();
		}

		protected void UpdateBarWithFillLevel(float value)
		{
			BarRenderer.GetPropertyBlock(_matBlock);
			_matBlock.SetFloat(FillShaderProperty, value);
			BarRenderer.SetPropertyBlock(_matBlock);
		}

		protected virtual void Update () {
			if (Camera.main is null) return;
			
			Transform cam = Camera.main.transform;
			Vector3 direction = transform.position - cam.position;

			direction.Normalize();

			Vector3 up = Vector3.Cross(direction, cam.right);

			transform.rotation = Quaternion.LookRotation(direction, up);
		}
	}
}