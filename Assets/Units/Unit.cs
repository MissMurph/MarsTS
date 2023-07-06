using MarsTS.Units.Commands;
using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using MarsTS.World.Pathfinding;

namespace MarsTS.Units {

	public class Unit : MonoBehaviour, ISelectable {

		public Faction Owner {
			get {
				return owner;
			}
			set {
				owner = value;
			}
		}

		[SerializeField]
		private Faction owner;

		private bool initialized = false;

		public int InstanceID { get { return id; } }

		[SerializeField]
		private int id;

		public string UnitType { get { return type; } }

		[SerializeField]
		private string type;

		//protected List<Node> path = new List<Node>();

		public Commandlet CurrentCommand { get; protected set; }

		public Queue<Commandlet> CommandQueue = new Queue<Commandlet>();

		public string[] boundCommands;

		protected Coroutine movementCoroutine;

		protected Transform target;
		private Vector3 targetOldPos;
		[SerializeField]
		private float moveSpeed;
		private Vector3[] path;
		private int targetIndex;
		private float angle;

		[SerializeField]
		private GameObject selectionCircle;

		private Action<bool> pathCompleteCallback;

		const float minPathUpdateTime = .2f;
		const float pathUpdateMoveThreshold = .5f;

		protected virtual void Awake () {
			selectionCircle.SetActive(false);
			//type = gameObject.name;
		}

		private void Start () {
			StartCoroutine(UpdatePath());
		}

		protected virtual void Update () {
			if (CurrentCommand is null && CommandQueue.TryDequeue(out Commandlet order)) {

				CurrentCommand = order;

				ProcessOrder(order);
			}
		}

		protected virtual void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "move":
				Move(order);
				break;
				case "stop":
				Stop();
				break;
				default:
				break;
			}
		}

		public void Init (int _id, Player _owner) {
			if (!initialized) {
				if (Owner == null) Owner = _owner;
				id = _id;
				name = UnitType + ":" + InstanceID.ToString();
				initialized = true;

				Color circleColour;

				switch (Owner.GetRelationship(Player.Main)) {
					case Players.Relationship.Owned:
						circleColour = Color.green;
						break;
					case Players.Relationship.Friendly:
						circleColour = Color.cyan;
						break;
					case Players.Relationship.Hostile:
						circleColour = Color.red;
						break;
					default:
						circleColour = Color.yellow;
						break;
				}

				selectionCircle.GetComponent<MeshRenderer>().material.color = circleColour;
			}
		}

		private void OnPathFound (Vector3[] newPath, bool pathSuccessful) {
			if (pathSuccessful) {
				path = newPath;
				if (movementCoroutine != null) StopCoroutine(movementCoroutine);
				movementCoroutine = StartCoroutine(FollowPath());
			}
		}

		protected void SetTarget (Vector3 _target, Action<bool> callback) {
			PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
			pathCompleteCallback = callback;
		}

		protected void SetTarget (Transform _target, Action<bool> callback) {
			SetTarget(_target.position, callback);
			pathCompleteCallback = callback;
			target = _target;
		}

		protected virtual void Stop () {
			if (movementCoroutine != null) StopCoroutine(movementCoroutine);
			pathCompleteCallback = null;
			path = null;
			target = null;
		}

		protected virtual void Move (Commandlet order) {
			if (order.TargetType.Equals(typeof(Vector3))) {
				Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

				SetTarget(deserialized.Target, (result) => CurrentCommand = null);
			}
		}

		protected IEnumerator UpdatePath () {
			if (Time.timeSinceLevelLoad < .3f) {
				yield return new WaitForSeconds(.3f);
			}

			float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;

			while (true) {
				yield return new WaitForSeconds(minPathUpdateTime);

				if (target != null && (target.position - targetOldPos).sqrMagnitude > sqrMoveThreshold) {
					PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
					targetOldPos = target.position;
				}
			}
		}

		protected IEnumerator FollowPath () {
			targetIndex = 0;
			Vector3 currentWaypoint = path[0];

			while (true) {
				if (transform.localPosition == currentWaypoint) {
					targetIndex++;

					if (targetIndex >= path.Length) {
						pathCompleteCallback.Invoke(true);
						target = null;
						path = null;
						yield break;
					}

					currentWaypoint = path[targetIndex];
				}

				transform.localPosition = Vector3.MoveTowards(transform.localPosition, currentWaypoint, moveSpeed * Time.deltaTime);

				yield return null;
			}
		}

		public void OnDrawGizmos () {
			if (path != null) {
				for (int i = targetIndex; i < path.Length; i++) {
					Gizmos.color = Color.black;
					Gizmos.DrawCube(path[i], Vector3.one / 2);

					if (i == targetIndex) {
						Gizmos.DrawLine(transform.position, path[i]);
					}
					else {
						Gizmos.DrawLine(path[i - 1], path[i]);
					}
				}
			}
		}

		public void Enqueue (Commandlet order) {
			CommandQueue.Enqueue(order);
		}

		public void Execute (Commandlet order) {
			CommandQueue.Clear();
			Stop();
			CurrentCommand = null;
			CommandQueue.Enqueue(order);
		}

		public Unit Get () {
			return this;
		}

		public string[] Commands () {
			return boundCommands;
		}

		public void Select (bool status) {
			if (status) selectionCircle.SetActive(true);
			else selectionCircle.SetActive(false);
		}

		public int Id () {
			return InstanceID;
		}

		public string Name () {
			return UnitType;
		}

		public Relationship Relationship (Faction other) {
			return owner.GetRelationship(other);
		}
	}
}