using MarsTS.Units.Commands;
using MarsTS.Units.Attacks;
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
		public Player Owner { get; private set; } = null;

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

		private Coroutine movementCoroutine;
		private Coroutine attackingCoroutine;

		private Transform target;
		private Vector3 targetOldPos;
		[SerializeField]
		private float moveSpeed;
		private Vector3[] path;
		private int targetIndex;
		private float angle;

		[SerializeField]
		private GameObject selectionCircle;

		private Action<bool> pathCompleteCallback;

		private Dictionary<string, Attacks.Attack> registeredAttacks = new Dictionary<string, Attacks.Attack>();

		[SerializeField]
		private Attacks.Attack[] attacksToRegister;

		const float minPathUpdateTime = .2f;
		const float pathUpdateMoveThreshold = .5f;

		private void Awake () {
			selectionCircle.SetActive(false);
			//type = gameObject.name;
		}

		private void Start () {
			StartCoroutine(UpdatePath());
		}

		private void Update () {
			if (CurrentCommand is null && CommandQueue.TryDequeue(out Commandlet order)) {

				CurrentCommand = order;

				ProcessOrder(order);
			}
		}

		private void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "move":
				Move(order);
				break;
				case "stop":
				Stop();
				break;
				case "attack":
				Attack(order);
				break;
			}
		}

		public void Init (int _id, Player _owner) {
			if (Owner is null) {
				Owner = _owner;
				id = _id;
				name = UnitType + ":" + InstanceID.ToString();
			}
		}

		private void OnPathFound (Vector3[] newPath, bool pathSuccessful) {
			if (pathSuccessful) {
				path = newPath;
				if (movementCoroutine != null) StopCoroutine(movementCoroutine);
				movementCoroutine = StartCoroutine(FollowPath());
			}
		}

		private void SetTarget (Vector3 _target, Action<bool> callback) {
			PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
			pathCompleteCallback = callback;
		}

		private void SetTarget (Transform _target, Action<bool> callback) {
			SetTarget(_target.position, callback);
			pathCompleteCallback = callback;
			target = _target;
		}

		private void Stop () {
			if (movementCoroutine != null) StopCoroutine(movementCoroutine);
			if (attackingCoroutine != null) StopCoroutine(attackingCoroutine);
			path = null;
			target = null;
		}

		private void Move (Commandlet order) {
			if (order.TargetType.Equals(typeof(Vector3))) {
				Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

				SetTarget(deserialized.Target, (result) => CurrentCommand = null);
			}
		}

		private void Attack (Commandlet order) {

		}

		IEnumerator UpdatePath () {
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

		IEnumerator FollowPath () {
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

		//True is success, false is fail/cancel
		private void CommandComplete (bool result) {
			Stop();
			CurrentCommand = null;
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

		public string Type () {
			return UnitType;
		}
	}
}