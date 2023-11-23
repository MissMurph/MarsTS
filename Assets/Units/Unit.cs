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
using MarsTS.Commands;
using MarsTS.UI;

namespace MarsTS.Units {

	public abstract class Unit : MonoBehaviour, ISelectable, ITaggable<Unit>, IAttackable, ICommandable {

		public GameObject GameObject { get { return gameObject; } }

		/*	IAttackable Properties	*/

		public int Health { get { return currentHealth; } }

		public int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		private int maxHealth;

		[SerializeField]
		private int currentHealth;

		/*	ISelectable Properties	*/
		
		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "unit:" + UnitType; } }

		public Sprite Icon { get { return icon; } }

		public Faction Owner { get { return owner; } }

		[SerializeField]
		private Sprite icon;

		[SerializeField]
		private string type;

		[SerializeField]
		protected Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(Unit); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get; protected set; }

		public Commandlet[] CommandQueue { get { return commandQueue.ToArray(); } }

		private Queue<Commandlet> commandQueue = new Queue<Commandlet>();

		[SerializeField]
		private string[] boundCommands;

		/*	Unit Fields	*/

		protected Entity entityComponent;

		protected Transform TrackedTarget {
			get {
				return target;
			}
			set {
				if (target != null) {
					EntityCache.TryGet(target.gameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => TrackedTarget = null);
				}

				target = value;

				if (value != null) {
					EntityCache.TryGet(value.gameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => TrackedTarget = null);
				}
			}
		}

		private Transform target;

		private Vector3 targetOldPos;

		protected Path currentPath = Path.Empty;
		private float angle;
		protected int pathIndex;

		private GameObject selectionCircle;

		protected Rigidbody body;

		private const float minPathUpdateTime = .5f;
		private const float pathUpdateMoveThreshold = .5f;

		[SerializeField]
		private float waypointCompletionDistance;

		protected EventAgent bus;

		protected virtual void Awake () {
			selectionCircle = transform.Find("SelectionCircle").gameObject;
			selectionCircle.SetActive(false);
			body = GetComponent<Rigidbody>();
			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();
			currentHealth = maxHealth;
		}

		protected virtual void Start () {
			StartCoroutine(UpdatePath());

			selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
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
					bus.Local(new PathCompleteEvent(bus, true));
					currentPath = Path.Empty;
				}
			}
		}

		protected void UpdateCommands () {
			if (CurrentCommand is null && commandQueue.TryDequeue(out Commandlet order)) {

				ProcessOrder(order);
			}
		}

		protected virtual void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "move":
				CurrentCommand = order;
				Move(order);
				break;
				case "stop":
				CurrentCommand = order;
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
			currentPath = Path.Empty;
			target = null;

			commandQueue.Clear();

			CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
			bus.Global(_event);

			CurrentCommand = null;
		}

		protected virtual void Move (Commandlet order) {
			if (order is Commandlet<Vector3> deserialized) {
				SetTarget(deserialized.Target);

				bus.AddListener<PathCompleteEvent>(OnPathComplete);
				order.Callback.AddListener((_event) => bus.RemoveListener<PathCompleteEvent>(OnPathComplete));
			}
		}

		private void OnPathComplete (PathCompleteEvent _event) {
			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;
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
			commandQueue.Enqueue(order);
		}

		public void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			commandQueue.Clear();

			currentPath = Path.Empty;
			target = null;

			if (CurrentCommand != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, true, this);
				CurrentCommand.Callback.Invoke(_event);
				bus.Global(_event);
			}
			CurrentCommand = null;
			commandQueue.Enqueue(order);
		}

		public Unit Get () {
			return this;
		}

		public string[] Commands () {
			return boundCommands;
		}

		public abstract Command Evaluate (ISelectable target);

		public abstract Commandlet Auto (ISelectable target);

		public void Select (bool status) {
			selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!Player.Main.HasSelected(this)) {
				selectionCircle.SetActive(false);
				bus.Local(new UnitHoverEvent(bus, status));
			}
		}

		public Relationship GetRelationship (Faction other) {
			return owner.GetRelationship(other);
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public void Attack (int damage) {
			if (damage < 0 && currentHealth >= maxHealth) return;
			currentHealth -= damage;

			bus.Global(new EntityHurtEvent(bus, this));

			if (currentHealth <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		protected virtual void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;
			}
		}
	}
}