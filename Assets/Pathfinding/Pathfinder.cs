using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World.Pathfinding {

	public class Pathfinder : MonoBehaviour {

		[SerializeField]
		private GameWorld world;

		public void FindPath (PathRequest request, Action<PathResult> callback) {
			Vector3[] waypoints = new Vector3[0];
			bool pathSuccess = false;

			Node originNode = world.GetNodeFromWorldPos(request.pathStart);
			Node targetNode = world.GetNodeFromWorldPos(request.pathEnd);

			if (originNode.Walkable && targetNode.Walkable) {
				Heap<Node> openSet = new Heap<Node>(world.MaxGridSize);
				HashSet<Node> closedSet = new HashSet<Node>();
				openSet.Add(originNode);

				while (openSet.Count > 0) {
					Node currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);

					if (currentNode == targetNode) {
						pathSuccess = true;
						break;
					}

					foreach (Node neighbour in world.GetNeighbours(currentNode)) {
						if (!neighbour.Walkable || closedSet.Contains(neighbour)) continue;

						int newMoveCost = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovePenalty;

						if (newMoveCost < neighbour.GCost || !openSet.Contains(neighbour)) {
							neighbour.GCost = newMoveCost;
							neighbour.HCost = GetDistance(neighbour, targetNode);
							neighbour.parent = currentNode;

							if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
							else openSet.UpdateItem(neighbour);
						}
					}
				}

				if (pathSuccess) {
					waypoints = RetracePath(originNode, targetNode);
					pathSuccess = waypoints.Length > 0;
				}

				callback(new PathResult(waypoints, pathSuccess, request.callback));
			}
		}

		private Vector3[] RetracePath (Node originNode, Node endNode) {
			List<Node> path = new List<Node>();

			Node currentNode = endNode;

			while (currentNode != originNode) {
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}

			Vector3[] waypoints = SimplifyPath(path);

			Array.Reverse(waypoints);

			return waypoints;
		}

		//There's something wrong with this, it keeps losing the last node in the path
		//NO LONGER
		private Vector3[] SimplifyPath (List<Node> path) {
			//We have to add the start position otherwise it's not returned
			List<Vector3> waypoints = new List<Vector3>();

			if (path.Count > 0) waypoints.Add(path[0].Position);

			Vector2 directionOld = Vector2.zero;

			for (int i = 1; i < path.Count; i++) {
				Vector2 directionNew = new Vector2(path[i - 1].GridPos.x - path[i].GridPos.x, path[i - 1].GridPos.y - path[i].GridPos.y);

				//Since this is the first node from the target the direction starts as the old direction we check against
				if (i == 1) {
					directionOld = directionNew;
				}

				if (directionNew != directionOld) {
					waypoints.Add(path[i].Position);
				}

				directionOld = directionNew;
			}

			return waypoints.ToArray();
		}

		private int GetDistance (Node originNode, Node targetNode) {
			int distX = Mathf.Abs(originNode.GridPos.x - targetNode.GridPos.x);
			int distY = Mathf.Abs(originNode.GridPos.y - targetNode.GridPos.y);

			if (distX > distY) return 14 * distY + 10 * (distX - distY);
			return 14 * distX + 10 * (distY - distX);
		}
	}
}