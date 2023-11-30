using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class Vision : MonoBehaviour {

        private static Vision instance;

		private GameWorld world;

		//2D array of bitmasks, determine which players can currently see the nodes
		private int[,] nodes;

		private int[,] visited;

		[SerializeField]
		private float nodeSize;

		private Dictionary<string, EntityVision> registeredVision;

		public Vector2Int GridSize;

		private Vector3 BottomLeft {
			get {
				return new Vector3(transform.position.x - nodeSize * GridSize.x / 2, 0, transform.position.z - nodeSize * GridSize.y / 2);
			}
		}

		private void Awake () {
			instance = this;
			world = GetComponent<GameWorld>();

			registeredVision = new Dictionary<string, EntityVision>();

			int width = GridSize.x;
			int height = GridSize.y;

			nodes = new int[width,height];
			visited = new int[width, height];
		}

		public static void Register (string entityName, EntityVision toRegister) {
			if (!instance.registeredVision.TryAdd(entityName, toRegister)) {
				Debug.LogWarning("Could not register vision component for " + entityName + "! Potentially already registered!");
			}
		}

		public void Visible (int x, int y, int players) {
			//Logical bitwise OR operator will set the bit to 1 if either the node or the player bit is 1
			nodes[x, y] |= players;
			visited[x, y] |= players;
		}

		public bool IsVisible (int x, int y, int players) {
			//Logical bitwise AND operator will only return 1 if both the node & the player return 1
			return (nodes[x, y] & players) > 0;
		}

		public bool WasVisited (int x, int y, int players) {
			return (visited[x, y] & players) > 0;
		}

		public static Vector2Int GetGridPosFromWorldPos (Vector3 worldPos) {

			float percentX = (worldPos.x - instance.BottomLeft.x) / (instance.GridSize.x * instance.nodeSize);
			float percentY = (worldPos.z - instance.BottomLeft.z) / (instance.GridSize.y * instance.nodeSize);

			percentX = Mathf.Clamp01(percentX);
			percentY = Mathf.Clamp01(percentY);

			int x = Mathf.RoundToInt((instance.GridSize.x - 1) * percentX);
			int y = Mathf.RoundToInt((instance.GridSize.y - 1) * percentY);

			return new Vector2Int(x, y);
		}

		public static Vector3 WorldPosFromGridPos (Vector2Int gridPos) {
			return instance.BottomLeft + new Vector3(gridPos.x * instance.nodeSize + (instance.nodeSize / 2), 0, gridPos.y * instance.nodeSize + (instance.nodeSize / 2));
		}

		private void OnDestroy () {
			instance = null;
		}
	}
}