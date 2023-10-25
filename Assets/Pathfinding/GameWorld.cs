﻿using MarsTS.World.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World {

	public class GameWorld : MonoBehaviour {

		private static GameWorld instance;

		public Vector2Int GridSize;

		[SerializeField]
		private bool DisplayGizmos;

		[SerializeField]
		private TerrainType[] walkableRegions;

		[SerializeField]
		private float nodeSize = 1;

		private Node[,] grid;

		private Dictionary<int, int> walkableRegionsDic = new Dictionary<int, int>();

		public int obstaclePenalty = 10;

		private int penaltyMin = 0;
		private int penaltyMax = 10;

		[SerializeField]
		private LayerMask walkableMask;
		public static LayerMask WalkableMask { get { return instance.walkableMask; } }

		[SerializeField]
		private LayerMask unwalkableMask;
		public static LayerMask UnwalkableMask { get { return instance.unwalkableMask; } }

		[SerializeField]
		private LayerMask selectableMask;
		public static LayerMask SelectableMask { get { return instance.selectableMask; } }

		public int MaxGridSize {
			get {
				return GridSize.x * GridSize.y;
			}
		}

		private Vector3 BottomLeft {
			get {
				return new Vector3(transform.position.x - nodeSize * GridSize.x / 2, 0, transform.position.z - nodeSize * GridSize.y / 2);
			}
		}

		private void Awake () {
			instance = this;

			foreach (TerrainType region in walkableRegions) {
				//AddLayerToMask(region.terrainMask.value, walkableMask);
				walkableRegionsDic.Add(Mathf.RoundToInt(Mathf.Log(region.terrainMask.value, 2)), region.terrainPenalty);
			}

			CreateGrid();
		}

		private void AddLayerToMask (int layer, LayerMask mask) {
			mask = mask | (1 << layer);
		}

		public static bool IsInLayerMask (int layer, LayerMask mask) {
			//Debug.Log("Layer: " + LayerMask.LayerToName(layer) + "   |   Mask: " + LayerMask.LayerToName(mask));
			return mask == (mask | (1 << layer));
		}

		public List<Node> GetNeighbours (Node node) {
			List<Node> neighbours = new List<Node>();

			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					if (x == 0 && y == 0) continue;

					int checkX = node.GridPos.x + x;
					int checkY = node.GridPos.y + y;

					if (checkX >= 0 && checkX < GridSize.x && checkY >= 0 && checkY < GridSize.y) {
						neighbours.Add(grid[checkX, checkY]);
					}
				}
			}

			return neighbours;
		}

		public Node GetNodeFromWorldPos (Vector3 worldPos) {

			float percentX = (worldPos.x - BottomLeft.x) / (GridSize.x * nodeSize);
			float percentY = (worldPos.z - BottomLeft.z) / (GridSize.y * nodeSize);

			percentX = Mathf.Clamp01(percentX);
			percentY = Mathf.Clamp01(percentY);

			int x = Mathf.RoundToInt((GridSize.x - 1) * percentX);
			int y = Mathf.RoundToInt((GridSize.y - 1) * percentY);

			return grid[x, y];
		}

		private void CreateGrid () {
			grid = new Node[GridSize.x, GridSize.y];

			for (int x = 0; x < GridSize.x; x++) {
				for (int y = 0; y < GridSize.y; y++) {
					Vector3 worldPos = BottomLeft + new Vector3(x * nodeSize + (nodeSize / 2), 0, y * nodeSize + (nodeSize / 2));

					bool walkable = !Physics.Raycast(worldPos + (Vector3.up * 100f), Vector3.down, 500f, UnwalkableMask);

					int penalty = 0;

					if (walkable) {
						Physics.Raycast(worldPos + (Vector3.up * 10f), Vector3.down, out RaycastHit hit, 100f, WalkableMask);

						if (hit.collider != null) walkableRegionsDic.TryGetValue(hit.collider.gameObject.layer, out penalty);
					}
					else {
						penalty += obstaclePenalty;
					}

					grid[x, y] = new Node(x, y, worldPos, walkable, penalty);
				}
			}

			BlurPenaltyMap(3);
		}

		void BlurPenaltyMap (int blurSize) {
			int kernelSize = blurSize * 2 + 1;
			int kernelExtents = (kernelSize - 1) / 2;

			int[,] penaltiesHorizontalPass = new int[GridSize.x, GridSize.y];
			int[,] penaltiesVerticalPass = new int[GridSize.x, GridSize.y];

			for (int y = 0; y < GridSize.y; y++) {
				for (int x = -kernelExtents; x <= kernelExtents; x++) {
					int sampleX = Mathf.Clamp(x, 0, kernelExtents);
					penaltiesHorizontalPass[0, y] += grid[sampleX, y].MovePenalty;
				}

				for (int x = 1; x < GridSize.x; x++) {
					int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, GridSize.x);
					int addIndex = Mathf.Clamp(x + kernelExtents, 0, GridSize.x - 1);

					penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].MovePenalty + grid[addIndex, y].MovePenalty;
				}
			}

			for (int x = 0; x < GridSize.x; x++) {
				for (int y = -kernelExtents; y <= kernelExtents; y++) {
					int sampleY = Mathf.Clamp(y, 0, kernelExtents);
					penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
				}

				int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
				grid[x, 0].MovePenalty = blurredPenalty;

				for (int y = 1; y < GridSize.y; y++) {
					int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, GridSize.y);
					int addIndex = Mathf.Clamp(y + kernelExtents, 0, GridSize.y - 1);

					penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
					blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
					grid[x, y].MovePenalty = blurredPenalty;

					if (blurredPenalty > penaltyMax) penaltyMax = blurredPenalty;
					if (blurredPenalty < penaltyMin) penaltyMin = blurredPenalty;
				}
			}
		}



		private void OnDrawGizmos () {
			if (DisplayGizmos) {
				Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x * nodeSize, 1, GridSize.y * nodeSize));

				if (grid != null) {

					foreach (Node n in grid) {
						Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.MovePenalty));

						if (!n.Walkable) Gizmos.color = Color.red;

						Gizmos.DrawWireCube(n.Position, Vector3.one * nodeSize);
					}
				}
			}
		}

		[System.Serializable]
		public class TerrainType {
			public LayerMask terrainMask;
			public int terrainPenalty;
		}
	}
}