using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

	public class SingleThreadRenderer : MonoBehaviour {

		private Player mainPlayer;

		private GameVision vision;

		private Texture2D render;

		[SerializeField]
		private Material fogMaterial;

		private Color[] texture;

		private void Awake () {
			vision = GetComponent<GameVision>();

			render = new Texture2D(vision.GridSize.x, vision.GridSize.y);
			render.filterMode = FilterMode.Point;

			texture = new Color[vision.GridSize.x * vision.GridSize.y];
		}

		private void Start () {
			mainPlayer = Player.Main;
			fogMaterial.mainTexture = render;
		}

		private void Update () {
			if (!vision.Dirty || vision.Nodes == null) return;

			for (int x = 0; x < vision.GridSize.x; x++) {
				for (int y = 0; y < vision.GridSize.y; y++) {
					float redValue = 0f;

					if ((vision.Nodes[x, y] & mainPlayer.VisionMask) == mainPlayer.VisionMask) {
						redValue = 1f;
					}
					else if ((vision.Visited[x, y] & mainPlayer.VisionMask) == mainPlayer.VisionMask) {
						redValue = 0.5f;
					}

					texture[x + y * vision.GridSize.x] = new Color(redValue, 0, 0, 1f);
				}
			}

			render.SetPixels(texture);
			render.Apply();
		}
	}
}