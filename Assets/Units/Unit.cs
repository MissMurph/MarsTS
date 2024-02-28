using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarsTS.World.Pathfinding;
using MarsTS.Teams;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Commands;
using MarsTS.UI;
using MarsTS.Vision;
using UnityEngine.ProBuilder;

namespace MarsTS.Units {

	public abstract class Unit : MonoBehaviour, ISelectable, ITaggable<Unit>, IAttackable, ICommandable {

		public GameObject GameObject { get { return gameObject; } }

		/*	IAttackable Properties	*/

		public int Health { get { return currentHealth; } }

		public int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		protected int maxHealth;

		[SerializeField]
		protected int currentHealth;

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

		public Commandlet CurrentCommand { get { return commands.Current; } }

		public Commandlet[] CommandQueue { get { return commands.Queue; } }

		public List<string> Active { get { return commands.Active; } }

		public List<Timer> Cooldowns { get { return commands.Cooldowns; } }

		public int Count { get { return commands.Count; } }

		//protected Queue<Commandlet> commandQueue = new Queue<Commandlet>();

		protected CommandQueue commands;

		[SerializeField]
		protected string[] boundCommands;

		/*	Unit Fields	*/

		protected Entity entityComponent;

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
			commands = GetComponent<CommandQueue>();

			if (currentHealth <= 0) {
				currentHealth = maxHealth;
			}
		}

		protected virtual void Start () {
			StartCoroutine(UpdatePath());

			//transform.Find("SelectionCircle").GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
			bus.AddListener<CommandStartEvent>(ExecuteOrder);

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		private void OnVisionInit (VisionInitEvent _event) {
			bool visible = GameVision.IsVisible(gameObject);

			foreach (GameObject hideable in hideables) {
				hideable.SetActive(visible);
			}
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

			commands.Clear();

			//CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
			//bus.Global(_event);

			//CurrentCommand = null;
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

		public virtual void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "move":
					break;
				case "stop":
					break;
				default:
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		protected virtual void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "move":
					Move(_event.Command);
					break;
				case "stop":
					Stop();
					break;
				default:
					break;
			}
		}

		public Unit Get () {
			return this;
		}

		public string[] Commands () {
			return boundCommands;
		}

		public abstract Command Evaluate (ISelectable target);

		public abstract Commandlet Auto (ISelectable target);

		public virtual void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public virtual void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				//selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!Player.Main.HasSelected(this)) {
				//selectionCircle.SetActive(false);
				bus.Local(new UnitHoverEvent(bus, status));
			}
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

		protected virtual void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;
			}
		}

		protected virtual void OnVisionUpdate (EntityVisibleEvent _event) {
			foreach (GameObject hideable in hideables) {
				hideable.SetActive(_event.Visible);
			}
		}

		public virtual bool CanCommand (string key) {
			bool canUse = false;

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == key) break;

				if (i >= boundCommands.Length - 1) return false;
			}

			if (commands.CanCommand(key)) canUse = true;
			//if (production.CanCommand(key)) canUse = true;

			return canUse;
		}
	}
}