using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MarsTS.Vision {

	public class VisionRenderer : MonoBehaviour {

		private static VisionRenderer instance;

		private Player mainPlayer;

		private Vision vision;

		private Texture2D render;

		[SerializeField]
		private Material fogMaterial;

		private Color[] texture;

		private Thread currentThread;

		private bool running;

		private Queue<Color[]> textureUpdates;

		private void Awake () {
			instance = this;

			vision = GetComponent<Vision>();

			render = new Texture2D(vision.GridSize.x, vision.GridSize.y);
			render.filterMode = FilterMode.Point;

			texture = new Color[vision.GridSize.x * vision.GridSize.y];

			textureUpdates = new Queue<Color[]>();

			running = true;
			Application.quitting += Quitting;
		}

		private void Start () {
			mainPlayer = Player.Main;
			fogMaterial.mainTexture = render;

			ThreadStart workerThread = delegate { PrepareRender(); };

			currentThread = new Thread(workerThread);
			currentThread.Start();
		}

		private void Update () {
			if (textureUpdates.Count > 0) {
				lock (textureUpdates) {
					texture = textureUpdates.Dequeue();
				}

				render.SetPixels(texture);
				render.Apply();
			}
		}

		private void Quitting () {
			running = false;
		}

		private void PrepareRender () {
			while (running) {
				if (vision.Dirty) {
					Render();
					vision.Dirty = false;
				}
			}
		}

		private void Render () {
			Color[] newTexture = new Color[vision.GridSize.x * vision.GridSize.y];

			for (int x = 0; x < vision.GridSize.x; x++) {
				for (int y = 0; y < vision.GridSize.y; y++) {
					float redValue = 0f;

					if ((vision.Nodes[x, y] & mainPlayer.Mask) == mainPlayer.Mask) {
						redValue = 1f;
					}
					else if ((vision.Visited[x, y] & mainPlayer.Mask) == mainPlayer.Mask) {
						redValue = 0.5f;
					}

					newTexture[x + y * vision.GridSize.x] = new Color(redValue, 0, 0, 1f);
				}
			}

			FinishedRender(newTexture);
		}

		private void FinishedRender (Color[] texture) {
			lock (textureUpdates) {
				textureUpdates.Enqueue(texture);
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}