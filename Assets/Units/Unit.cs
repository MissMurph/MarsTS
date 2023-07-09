using MarsTS.Units.Commands;
using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using MarsTS.World.Pathfinding;
using MarsTS.Teams;

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

		protected Transform target;
		private Vector3 targetOldPos;

		protected Path currentPath = Path.Empty();
		private float angle;
		protected int pathIndex;

		//[SerializeField]
		//private float turnSpeed;

		[SerializeField]
		private GameObject selectionCircle;

		protected Rigidbody body;

		const float minPathUpdateTime = .2f;
		const float pathUpdateMoveThreshold = .5f;

		protected virtual void Awake () {
			selectionCircle.SetActive(false);
			body = GetComponent<Rigidbody>();
			//type = gameObject.name;
		}

		private void Start () {
			StartCoroutine(UpdatePath());
		}

		protected virtual void Update () {
			UpdateCommands();


		}

		protected void UpdateCommands () {
			if (CurrentCommand is null && CommandQueue.TryDequeue(out Commandlet order)) {

				CurrentCommand = order;

				ProcessOrder(order);
			}
		}

		protected virtual void ProcessOrder (Commandlet order) {
			if (!ReferenceEquals(order.Commander, owner)) return;

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

				selectionCircle.GetComponent<MeshRenderer>().material = Relationship(Player.Main).Material();
			}
		}

		private void OnPathFound (Path newPath, bool pathSuccessful) {
			if (pathSuccessful) {
				currentPath = newPath;
				//if (movementCoroutine != null) StopCoroutine(movementCoroutine);
				//movementCoroutine = StartCoroutine(FollowPath());
			}
		}

		protected void SetTarget (Vector3 _target) {
			PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
		}

		protected void SetTarget (Transform _target) {
			SetTarget(_target.position);
			target = _target;
		}

		protected virtual void Stop () {
			//if (movementCoroutine != null) StopCoroutine(movementCoroutine);
			currentPath = Path.Empty();
			target = null;
		}

		protected virtual void Move (Commandlet order) {
			if (order.TargetType.Equals(typeof(Vector3))) {
				Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

				SetTarget(deserialized.Target);
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

		public void OnDrawGizmos () {
			if (!currentPath.IsEmpty) {
				for (int i = pathIndex; i < currentPath.Length; i++) {
					Gizmos.color = Color.black;
					Gizmos.DrawCube(currentPath[i], Vector3.one / 2);

					if (i == pathIndex) {
						Gizmos.DrawLine(transform.position, currentPath[i]);
					}
					else {
						Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
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