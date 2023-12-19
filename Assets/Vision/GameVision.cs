using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;

namespace MarsTS.Vision {

    public class GameVision : MonoBehaviour {

        private static GameVision instance;

		//2D array of bitmasks, determine which players can currently see the nodes
		public int[,] Nodes { get; private set; }

		public int[,] Visited { get; private set; }

		private Queue<int[,]> results;
		private Queue<VisionEntry[]> requests;

		private int[,] heights;

		[SerializeField]
		private float nodeSize;

		public static float NodeSize { get { return instance.nodeSize; } }

		private Dictionary<string, EntityVision> registeredVision;

		public Vector2Int GridSize;

		[SerializeField]
		private float visionUpdateTime;

		[SerializeField]
		private bool drawGizmos;

		public static bool DrawGizmos { get { return instance.drawGizmos; } }

		private Vector3 BottomLeft { get; set; }

		private Thread currentThread;

		private bool running;

		public bool Dirty;

		private EventAgent bus;

		private bool initialized = false;

		public static bool Initialized {
			get {
				if (instance == null) return false;
				else return instance.initialized;
			}
		}

		private void Awake () {
			instance = this;

			bus = GetComponent<EventAgent>();

			registeredVision = new Dictionary<string, EntityVision>();
			results = new Queue<int[,]>();
			requests = new Queue<VisionEntry[]>();

			int width = GridSize.x;
			int height = GridSize.y;

			Nodes = new int[width, height];
			Visited = new int[width, height];
			heights = new int[width, height];

			BottomLeft = new Vector3(transform.position.x - nodeSize * GridSize.x / 2, 0, transform.position.z - nodeSize * GridSize.y / 2);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (Physics.Raycast(WorldPosFromGridPos(new(x, y)) + (Vector3.up * 100f), Vector3.down, out RaycastHit hit, 200f, GameWorld.EnvironmentMask)) {
						heights[x, y] = Mathf.RoundToInt(hit.point.y);
					}
				}
			}

			running = true;
			Dirty = false;
			initialized = false;

			Application.quitting += Quitting;
		}

		private void Start () {
			StartCoroutine(EnqueueUpdate());

			EventBus.AddListener<EntityDeathEvent>(OnEntityDeath);

			ThreadStart workerThread = delegate { ProcessUpdate(); };

			currentThread = new Thread(workerThread);
			currentThread.Start();
		}

		private void Update () {
			if (results.Count > 0) {
				lock (results) {
					Nodes = results.Dequeue();
				}

				for (int x = 0; x < GridSize.x; x++) {
					for (int y = 0; y < GridSize.y; y++) {
						Visited[x,y] |= Nodes[x, y];
					}
				}

				Dirty = true;
				if (!initialized) {
					bus.Global(new VisionInitEvent(bus));
					initialized = true;
					return;
				}
				bus.Global(new VisionUpdateEvent(bus));
			}
		}

		private void Quitting () {
			running = false;
		}

		private IEnumerator EnqueueUpdate () {
			if (Time.timeSinceLevelLoad < .5f) {
				yield return new WaitForSeconds(.5f);
			}

			while (true) {
				yield return new WaitForSeconds(visionUpdateTime);

				List<VisionEntry> sources = new List<VisionEntry>();

				foreach (EntityVision vision in registeredVision.Values) {
					sources.Add(vision.Collect());
				}

				lock (requests) {
					requests.Enqueue(sources.ToArray());
				}
			}
		}

		private void ProcessUpdate () {
			while (running) {
				if (requests.Count > 0) {
					VisionEntry[] toProcess;

					lock (requests) {
						toProcess = requests.Dequeue();
					}

					CalculateVision(toProcess);
				}
			}
		}

		private void CalculateVision (VisionEntry[] sources) {
			int[,] newNodes = new int[GridSize.x, GridSize.y];

			foreach (VisionEntry vision in sources) {
				int sqrRange = vision.range * vision.range;

				Dictionary<int, List<Vector2Int>> distanceDic = new Dictionary<int, List<Vector2Int>>();

				for (int x = vision.gridPos.x - vision.range; x < vision.gridPos.x + vision.range + 1; x++) {
					if (x < 0 || x >= GridSize.x) continue;
					for (int y = vision.gridPos.y - vision.range; y < vision.gridPos.y + vision.range + 1; y++) {
						if (y < 0 || y >= GridSize.y) continue;

						int xDistance = Mathf.Abs(x - vision.gridPos.x);
						int yDistance = Mathf.Abs(y - vision.gridPos.y);

						int distance = new Vector2Int(xDistance, yDistance).sqrMagnitude;

						if (distance <= sqrRange) {
							List<Vector2Int> set = distanceDic.GetValueOrDefault(distance, new List<Vector2Int>());

							if (!distanceDic.ContainsKey(distance)) distanceDic[distance] = set;

							set.Add(new Vector2Int(x, y));
						}
					}
				}

				foreach (KeyValuePair<int, List<Vector2Int>> entry in distanceDic) {
					foreach (Vector2Int node in entry.Value) {
						if ((newNodes[node.x, node.y] & vision.mask) == vision.mask) {
							continue;
						}

						LinkedList<Vector2Int> toInspect = GridLine(vision.gridPos.x, vision.gridPos.y, node.x, node.y);

						while (toInspect.Count > 0) {
							Vector2Int inLine = toInspect.First.Value;
							toInspect.RemoveFirst();

							if ((newNodes[node.x, node.y] & vision.mask) == vision.mask) {
								continue;
							}

							if (heights[inLine.x, inLine.y] > vision.height) {
								break;
							}
							else {
								newNodes[inLine.x, inLine.y] |= vision.mask;
								//visited[inLine.x, inLine.y] |= vision.mask;
							}
						}
					}
				}
			}
			

			FinishCalculatingVision(newNodes);
		}

		private void FinishCalculatingVision (int[,] newVision) {
			lock (results) {
				results.Enqueue(newVision);
			}
		}

		//Bresenham's line algorithm for speed I think?
		//Stolen from rosettacode.org
		private LinkedList<Vector2Int> GridLine (int x1, int y1, int x2, int y2) {
			int dx = Mathf.Abs(x2 - x1);
			int sx = x1 < x2 ? 1 : -1;

			int dy = Mathf.Abs(y2 - y1);
			int sy = y1 < y2 ? 1 : -1;

			int error = (dx > dy ? dx : -dy) / 2, e2;

			LinkedList<Vector2Int> output = new LinkedList<Vector2Int>();

			for (; ; ) {
				output.AddLast(new Vector2Int(x1, y1));

				if (x1 == x2 && y1 == y2) break;
				e2 = error;
				if (e2 > -dx) { error -= dy; x1 += sx; }
				if (e2 < dy) { error += dx; y1 += sy; }
			}

			return output;
		}

		public static void Register (string entityName, EntityVision toRegister) {
			if (!instance.registeredVision.TryAdd(entityName, toRegister)) {
				Debug.LogWarning("Could not register vision component for " + entityName + "! Potentially already registered!");
			}
		}

		public bool IsVisible (int x, int y, int players) {
			//Logical bitwise AND operator will only return 1 if both the node & the player return 1
			int nodeMask = Nodes[x, y];
			int resultingMask = nodeMask & players;
			return resultingMask > 0;
		}

		public static bool IsVisible (GameObject _object, int players) {
			if (instance.registeredVision.TryGetValue(_object.transform.root.name, out EntityVision visionComp)) {
				return (visionComp.VisibleTo & players) > 0;
			}
			
			Vector2Int pos = GetGridPosFromWorldPos(_object.transform.position);
			return instance.IsVisible(pos.x, pos.y, players);
		}

		public int VisibleTo (int x, int y) {
			return Nodes[x, y];
		}
		public static int VisibleTo (GameObject _object) {
			Vector2Int pos = GetGridPosFromWorldPos(_object.transform.position);
			return instance.VisibleTo(pos.x, pos.y);
		}

		public static bool IsVisible (string entityName, int players) {
			if (instance.registeredVision.TryGetValue(entityName, out EntityVision unitVision)) {
				return IsVisible(unitVision.gameObject, players);
			}

			return true;
		}

		public bool WasVisited (int x, int y, int players) {
			return (Visited[x, y] & players) > 0;
		}

		public static bool WasVisited (GameObject _object, int players) {
			Vector2Int pos = GetGridPosFromWorldPos(_object.transform.position);
			return instance.WasVisited(pos.x, pos.y, players);
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

		private void OnEntityDeath (EntityDeathEvent _event) {
			if (registeredVision.TryGetValue(_event.Unit.GameObject.name, out EntityVision vision)) {
				registeredVision.Remove(_event.Unit.GameObject.name);
			}
		}

		private void OnDrawGizmos () {
			if (drawGizmos) {
				Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x * nodeSize, 1, GridSize.y * nodeSize));

				if (Nodes != null) {
					Gizmos.color = Color.cyan;

					for (int x = 0; x < GridSize.x; x++) {
						for (int y = 0; y < GridSize.y; y++) {
							if (Nodes[x, y] > 0) Gizmos.DrawCube(WorldPosFromGridPos(new(x, y)), Vector3.one / 2f);
						}
					}
				}
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}

	public struct VisionEntry {
		public Vector2Int gridPos;
		public int range;
		public int height;
		public int mask;
	}
}