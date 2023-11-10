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
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Prefabs;

namespace MarsTS.Units {

	public class Unit : MonoBehaviour, ISelectable, ITaggable<Unit>, IRegistryObject<Unit> {

		public int Health {
			get {
				return currentHealth;
			}
		}

		public int MaxHealth {
			get {
				return maxHealth;
			}
		}

		[SerializeField]
		private int maxHealth;

		[SerializeField]
		private int currentHealth;

		public Faction Owner {
			get {
				return owner;
			}
			set {
				owner = value;
			}
		}

		[SerializeField]
		protected Faction owner;

		protected Entity entityComponent;

		[SerializeField]
		private string type;

		//protected List<Node> path = new List<Node>();

		public Commandlet CurrentCommand { get; protected set; }

		public string Key {
			get {
				return "selectable";
			}
		}

		public Type Type {
			get {
				return typeof(Unit);
			}
		}

		public GameObject GameObject {
			get {
				return gameObject; 
			}
		}

		public int ID {
			get {
				return entityComponent.ID;
			}
		}

		public string RegistryType {
			get {
				return "unit";
			}
		}

		public string RegistryKey {
			get {
				return RegistryType + ":" + UnitType;
			}
		}

		public string UnitType {
			get {
				return type;
			}
		}

		public Queue<Commandlet> CommandQueue = new Queue<Commandlet>();
		
		[SerializeField]
		private string[] boundCommands;

		protected Transform target;
		private Vector3 targetOldPos;

		protected Path currentPath = Path.Empty;
		private float angle;
		protected int pathIndex;

		//[SerializeField]
		//private float turnSpeed;

		private GameObject selectionCircle;

		protected Rigidbody body;

		const float minPathUpdateTime = .5f;
		const float pathUpdateMoveThreshold = .5f;

		[SerializeField]
		private float waypointCompletionDistance;

		private EventAgent eventAgent;

		protected virtual void Awake () {
			selectionCircle = transform.Find("SelectionCircle").gameObject;
			selectionCircle.SetActive(false);
			body = GetComponent<Rigidbody>();
			entityComponent = GetComponent<Entity>();
			eventAgent = GetComponent<EventAgent>();
			currentHealth = maxHealth;
		}

		protected virtual void Start () {
			StartCoroutine(UpdatePath());

			selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();
		}

		protected virtual void Update () {
			UpdateCommands();

			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];
				float distance = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).magnitude;

				if (distance <= waypointCompletionDistance) {
					pathIndex++;
				}

				if (pathIndex >= currentPath.Length) {
					currentPath = Path.Empty;
				}
			}
		}

		protected void UpdateCommands () {
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

		private void OnPathFound (Path newPath, bool pathSuccessful) {
			if (pathSuccessful) {
				currentPath = newPath;
				pathIndex = 0;
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
			currentPath = Path.Empty;
			target = null;
		}

		protected virtual void Move (Commandlet order) {
			if (order.TargetType.Equals(typeof(Vector3))) {
				Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

				SetTarget(deserialized.Target);
			}
		}

		protected IEnumerator UpdatePath () {
			if (Time.timeSinceLevelLoad < .5f) {
				yield return new WaitForSeconds(.5f);
			}

			float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;

			while (true) {
				yield return new WaitForSeconds(minPathUpdateTime);

				if (target != null && (target.position - targetOldPos).sqrMagnitude > sqrMoveThreshold) {
					PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
					targetOldPos = target.position;
				}
				/*else if (!currentPath.IsEmpty) {
					PathRequestManager.RequestPath(transform.position, currentPath.End, OnPathFound);
				}*/
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
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			CommandQueue.Enqueue(order);
		}

		public void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
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

		public Relationship GetRelationship (Faction other) {
			return owner.GetRelationship(other);
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public void Attack (int damage) {
			currentHealth -= damage;

			eventAgent.Local(new EntityHurtEvent(eventAgent, this));

			if (currentHealth <= 0) {
				eventAgent.Global(new EntityDeathEvent(eventAgent, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public IRegistryObject<Unit> GetRegistryEntry () {
			throw new NotImplementedException();
		}
	}
}