using UnityEngine;

namespace Ratworx.MarsTS.Pathfinding {

	public class Node : IHeapItem<Node> {

		public int HeapIndex { get; set; }

		public Vector2Int GridPos { get; private set; }
		public Vector3 Position { get; private set; }
		public int MovePenalty { get; set; }

		public int FCost { get { return GCost + HCost; } }
		public int GCost { get; set; }
		public int HCost { get; set; }
		public bool Walkable { get; set; }

		public Node parent;

		public Node (int _x, int _y, Vector3 _position, bool _walkable, int _modifier) {
			GridPos = new Vector2Int(_x, _y);
			MovePenalty = _modifier;
			Position = _position;
			Walkable = _walkable;
		}

		public int CompareTo (Node nodeToCompare) {
			int compare = FCost.CompareTo(nodeToCompare.FCost);

			if (compare == 0) {
				compare = HCost.CompareTo(nodeToCompare.HCost);
			}

			return -compare;
		}
	}
}