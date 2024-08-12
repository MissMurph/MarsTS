using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Vision;
using MarsTS.World;
using MarsTS.World.Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class InfantryMember : MonoBehaviour, ISelectable, ITaggable<InfantryMember>, IAttackable, ICommandable {

		public GameObject GameObject => gameObject;
		public IUnit Unit => this;

		/*	IAttackable Properties	*/

		public int Health { get { return currentHealth; } }

		public int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		protected int maxHealth;

		[SerializeField]
		protected int currentHealth;

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.Id; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "unit:" + UnitType; } }

		public Sprite Icon { get { return icon; } }

		public Faction Owner { get { return squad.Owner; } }

		[SerializeField]
		private Sprite icon;

		[SerializeField]
		private string type;

		[SerializeField]
		protected Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(AbstractUnit); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get { return squad.CurrentCommand; } }

		public Commandlet[] CommandQueue { get { return squad.CommandQueue; } }

		public List<string> Active => throw new NotImplementedException();

		public List<Timer> Cooldowns => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		/*	Infantry Fields	*/

		protected Entity entityComponent;

		public InfantrySquad squad;

		[SerializeField]
		protected float moveSpeed;

		protected float currentSpeed;

		private GroundDetection ground;

		protected bool isSelected;

		protected Transform TrackedTarget {
			get {
				return target;
			}
			set {
				if (target != null) {
					EntityCache.TryGet(target.gameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<UnitDeathEvent>((_event) => TrackedTarget = null);
				}

				target = value;

				if (value != null) {
					EntityCache.TryGet(value.gameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<UnitDeathEvent>((_event) => TrackedTarget = null);

					SetTarget(value);
				}
			}
		}

		private Transform target;

		private Vector3 targetOldPos;

		protected Path currentPath {
			get;
			set;
		} = Path.Empty;

		private float angle;
		protected int pathIndex;

		protected Rigidbody body;

		private const float minPathUpdateTime = .5f;
		private const float pathUpdateMoveThreshold = .5f;

		[SerializeField]
		protected float waypointCompletionDistance;

		protected EventAgent bus;

		[SerializeField]
		private GameObject[] hideables;

		protected virtual void Awake () {
			body = GetComponent<Rigidbody>();
			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();

			bus.AddListener<EntityInitEvent>(OnEntityInit);

			currentHealth = maxHealth;

			ground = GetComponent<GroundDetection>();

			currentSpeed = moveSpeed;
			isSelected = false;
		}

		protected virtual void Start () {
			StartCoroutine(UpdatePath());

			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
		}

		protected virtual void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) return;

			transform.parent = null;
		}

		protected virtual void Update () {
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

		protected virtual void FixedUpdate () {
			if (ground.Grounded) {
				//Dunno why we need this check on the infantry member when we don't need it on any other unit type...
				if (!currentPath.IsEmpty && !(pathIndex >= currentPath.Length)) {
					Vector3 targetWaypoint = currentPath[pathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
					body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

					Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					Vector3 newVelocity = moveDirection * currentSpeed;

					body.velocity = newVelocity;
				}
				else {
					body.velocity = Vector3.zero;
				}
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

			//CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
			//bus.Global(_event);

			//CurrentCommand = null;
		}

		public virtual void Order (Commandlet order, bool inclusive) {
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

		/*	Commands	*/

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
		}
		
		public virtual void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
			isSelected = status;
		}

		public virtual void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				//selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!isSelected) {
				//selectionCircle.SetActive(false);
				bus.Local(new UnitHoverEvent(bus, status));
			}
		}

		public virtual void AutoCommand (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public virtual CommandFactory Evaluate (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public Relationship GetRelationship (Faction other) {
			return Owner.GetRelationship(other);
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public void Attack (int damage) {
			if (Health <= 0) return;
			if (damage < 0 && currentHealth >= maxHealth) return;
			currentHealth -= damage;

			if (currentHealth <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject);
			}
			else bus.Global(new UnitHurtEvent(bus, this));
		}

		public InfantryMember Get () {
			return this;
		}

		public string[] Commands () {
			return new string[0];
		}

		protected virtual void OnVisionUpdate (EntityVisibleEvent _event) {
			foreach (GameObject hideable in hideables) {
				hideable.SetActive(_event.Visible);
			}
		}

		public bool CanCommand (string key) {
			throw new NotImplementedException();
		}
	}
}