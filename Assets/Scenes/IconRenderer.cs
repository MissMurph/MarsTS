using System;
using System.IO;
using UnityEngine;

namespace Scenes
{
	public class IconRenderer : MonoBehaviour {

		[SerializeField]
		private RenderDetails[] renderEntries;

		[SerializeField]
		private string key;

		private Camera renderCam;

		private void Awake () {
			renderCam = GetComponent<Camera>();
		}

		private void Start () {
			Capture();
		}

		private void Capture () {
			RenderTexture activeTexture = RenderTexture.active;
			RenderTexture.active = renderCam.targetTexture;

			renderCam.Render();

			Texture2D image = new Texture2D(renderCam.targetTexture.width, renderCam.targetTexture.height);
			image.ReadPixels(new Rect(0, 0, renderCam.targetTexture.width, renderCam.targetTexture.height), 0, 0);
			image.Apply();
			RenderTexture.active = activeTexture;

			byte[] bytes = image.EncodeToPNG();
			Destroy(image);

			File.WriteAllBytes(Application.dataPath + "/Icons/" + key + ".png", bytes);
		}
	}

	[Serializable]
	public class RenderDetails {
		public string name;
		public float viewPortSize;
	}
}