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

		private Dictionary<string, EntityVision> registeredVision;

		private void Awake () {
			instance = this;
			world = GetComponent<GameWorld>();

			registeredVision = new Dictionary<string, EntityVision>();

			int width = world.GridSize.x;
			int height = world.GridSize.y;

			nodes = new int[width,height];
			visited = new int[width, height];
		}

		public void Register () {

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

		private void OnDestroy () {
			instance = null;
		}
	}
}