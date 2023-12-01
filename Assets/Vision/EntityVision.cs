using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class EntityVision : MonoBehaviour {
        
        public int Mask { get { return owner.Mask; } }

		private Faction owner;

		private EventAgent bus;

		public int Range { get { return visionRange; } }

		[SerializeField]
		private int visionRange;

		//Will evenly divide the amount of raycasts in a circle around the entity
		[SerializeField]
		private int rayCount;

		[SerializeField]
		private bool drawGizmos;

		[SerializeField]
		private float visionUpdateTime;

		private List<Vector2Int> visible;

		private void Awake () {
			bus = GetComponent<EventAgent>();

			visible = new List<Vector2Int>();

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		private void Update () {
			float increment = 360f / rayCount;

			if (owner != null) {
				for (int i = 0; i < rayCount; i++) {
					float radian = (increment * i) * Mathf.Deg2Rad;

					Vector3 direction = new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));

					float rayDistance = visionRange;

					if (Physics.Raycast(transform.position, direction, out RaycastHit hit, visionRange, GameWorld.EnvironmentMask)) {
						rayDistance = hit.distance;
					}

					//if (drawGizmos) Debug.DrawRay(transform.position, direction * rayDistance, Color.cyan, 0.1f);
				}
			}
		}

		private IEnumerator UpdateVision () {
			if (Time.timeSinceLevelLoad < .5f) {
				yield return new WaitForSeconds(.5f);
			}

			int sqrRange = visionRange * visionRange;

			while (true) {
				yield return new WaitForSeconds(visionUpdateTime);

				Vector2Int pos = Vision.GetGridPosFromWorldPos(transform.position);

				Dictionary<int, List<Vector2Int>> distanceDic = new Dictionary<int, List<Vector2Int>>();

				List<Vector2Int> canSee = new List<Vector2Int>();
				List<Vector2Int> blocked = new List<Vector2Int>();

				for (int x = pos.x - visionRange; x < pos.x + visionRange + 1; x++) {
					for (int y = pos.y - visionRange; y < pos.y + visionRange + 1; y++) {
						int xDistance = Mathf.Abs(x - pos.x);
						int yDistance = Mathf.Abs(y - pos.y);

						int distance = new Vector2Int(xDistance, yDistance).sqrMagnitude;

						if (distance <= sqrRange) {
							List<Vector2Int> set = distanceDic.GetValueOrDefault(distance, new List<Vector2Int>());

							if (!distanceDic.ContainsKey(distance)) distanceDic[distance] = set;

							set.Add(new Vector2Int(x,y));
						}
					}
				}

				foreach (KeyValuePair<int, List<Vector2Int>> entry in distanceDic) {
					foreach (Vector2Int node in entry.Value) {
						if (canSee.Contains(node) || blocked.Contains(node)) {
							continue;
						}

						float maxDistance = sqrRange;

						if (Physics.Linecast(transform.position, Vision.WorldPosFromGridPos(node), out RaycastHit hit, GameWorld.EnvironmentMask)) {
							maxDistance = (Vision.GetGridPosFromWorldPos(hit.point) - pos).sqrMagnitude;
						}

						List<Vector2Int> toInspect = GridLine(pos.x, pos.y, node.x, node.y);

						foreach (Vector2Int inLine in toInspect) {
							int distanceInLine = (inLine - pos).sqrMagnitude;
							if (distanceInLine > maxDistance) blocked.Add(inLine);
							else canSee.Add(inLine);

							if (inLine != node) distanceDic[distanceInLine].Remove(inLine);
						}
					}
				}

				visible = canSee;
			}
		}

		//Bresenham's line algorithm for speed I think?
		//Stolen from rosettacode.org
		private List<Vector2Int> GridLine (int x1, int y1, int x2, int y2) {
			int dx = Mathf.Abs(x2 - x1);
			int sx = x1 < x2 ? 1 : -1;

			int dy = Mathf.Abs(y2 - y1);
			int sy = y1 < y2 ? 1 : -1;

			int error = (dx > dy ? dx : -dy) / 2, e2;

			List<Vector2Int> output = new List<Vector2Int>();

			for (; ; ) {
				output.Add(new(x1, y1));

				if (x1 == x2 && y1 == y2) break;
				e2 = error;
				if (e2 > -dx) { error -= dy; x1 += sx; }
				if (e2 < dy) { error += dx; y1 += sy; }
			}

			return output;
		}

		private void OnEntityInit (EntityInitEvent _event) {
			Vision.Register(gameObject.name, this);

			owner = _event.ParentEntity.Get<ISelectable>("selectable").Owner;

			
		}

		public VisionEntry Collect () {
			return new VisionEntry {
				gridPos = Vision.GetGridPosFromWorldPos(transform.position),
				range = visionRange,
				height = Mathf.RoundToInt(transform.position.y),
				mask = Mask
			};
		}

		private void OnDrawGizmos () {
			if (drawGizmos && visible != null) {
				foreach (Vector2Int pos in visible) {
					Gizmos.color = Color.cyan;
					Gizmos.DrawCube(Vision.WorldPosFromGridPos(pos), Vector3.one / 4f);
				}
			}
		}
	}
}