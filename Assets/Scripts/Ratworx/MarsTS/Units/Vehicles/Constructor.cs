using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Turrets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Vehicles {

    public class Constructor : AbstractUnit {

		[Header("Movement")]

		[SerializeField]
		private float topSpeed;

		[SerializeField]
		private float acceleration;

		[SerializeField]
		private float turnSpeed;

		private float CurrentAngle {
			get {
				float angle = transform.rotation.eulerAngles.y;
				return angle;
			}
		}

		[SerializeField]
		private float angleTolerance;

		private float _velocity;

		private GroundDetection _ground;

		private Dictionary<string, BuilderTurret> _registeredTurrets = new Dictionary<string, BuilderTurret>();

		private IAttackable RepairTarget {
			get => _repairTarget;
			set {
				if (_repairTarget != null) {
					EntityCache.TryGet(_repairTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<UnitDeathEvent>((_event) => _repairTarget = null);
				}

				_repairTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<UnitDeathEvent>((_event) => _repairTarget = null);
				}
			}
		}

		private IAttackable _repairTarget;

		protected override void Awake () {
			base.Awake();

			_ground = GetComponent<GroundDetection>();

			foreach (BuilderTurret turret in GetComponentsInChildren<BuilderTurret>()) {
				_registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();
			
			if (!NetworkManager.Singleton.IsServer) return;

			if (_repairTarget == null) return;

			if (Vector3.Distance(_repairTarget.GameObject.transform.position, transform.position) <= _registeredTurrets["turret_main"].Range) {
				TrackedTarget = null;
				CurrentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, _repairTarget.GameObject.transform)) {
				SetTarget(_repairTarget.GameObject.transform);
			}
		}

		protected virtual void FixedUpdate () {
			if (!NetworkManager.Singleton.IsServer) return;

			_velocity = Body.velocity.sqrMagnitude;

			if (_ground.Grounded) {
				if (!CurrentPath.IsEmpty) {
					Vector3 targetWaypoint = CurrentPath[PathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

					float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
					Body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

					Vector3 currentVelocity = Body.velocity;
					Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, _ground.Slope.normal);

					adjustedVelocity *= currentVelocity.magnitude;

					if (Vector3.Angle(targetDirection, transform.forward) <= angleTolerance) {
						float accelCap = 1f - (_velocity / (topSpeed * topSpeed));

						//This moves the velocity according to the rotation of the unit
						Body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

						//Relative so it can take into account the forward vector of the car
						Body.AddRelativeForce(Vector3.forward * (acceleration * accelCap * Time.fixedDeltaTime), ForceMode.Acceleration);
					}

					if (_velocity > topSpeed * topSpeed) {
						Vector3 direction = Body.velocity.normalized;
						direction *= topSpeed;
						Body.velocity = direction;
					}
				}
				else if (_velocity >= 0.5f) {
					Body.AddRelativeForce(-Body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "repair":
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		protected override void ExecuteOrder (CommandStartEvent _event) {
			RepairTarget = null;
			switch (_event.Command.Name) {
				case "repair":
					Repair(_event.Command);
					break;
				default:
					base.ExecuteOrder(_event);
					break;
			}
		}

		protected void Repair (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				IAttackable unit = deserialized.Target;

				if (unit.GetRelationship(Owner) == Relationship.Owned || unit.GetRelationship(Owner) == Relationship.Friendly) {
					RepairTarget = unit;

					EntityCache.TryGet(RepairTarget.GameObject.transform.root.name, out EventAgent targetBus);

					targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
					targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

					order.Callback.AddListener(RepairCancelled);
				}
			}
		}

		//Could potentially move these to the actual Command Classes
		private void OnTargetHealed (UnitHurtEvent _event) {
			if (_event.Targetable.Health >= _event.Targetable.MaxHealth) {
				EntityCache.TryGet(_event.Targetable.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);
			targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			Stop();
		}

		private void RepairCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.IsCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				RepairTarget = null;
			}
		}

		public override CommandFactory Evaluate (ISelectable target) {
			if (target is IAttackable attackable
				&& (target.GetRelationship(Owner) == Relationship.Owned || target.GetRelationship(Owner) == Relationship.Friendly)
				//&& (target.GameObject.CompareTag("vehicle") || target.GameObject.CompareTag("building"))
				&& attackable.Health < attackable.MaxHealth) {
				return CommandPrimer.Get("repair");
			}

			return CommandPrimer.Get("move");
		}

		public override void AutoCommand (ISelectable target) {
			if (target is IAttackable attackable
				&& (target.GetRelationship(Owner) == Relationship.Owned || target.GetRelationship(Owner) == Relationship.Friendly)
				//&& (target.GameObject.CompareTag("vehicle") || target.GameObject.CompareTag("building"))
				&& attackable.Health < attackable.MaxHealth) {
				//CommandRegistry.Get<Repair>("repair").Construct(attackable, Player.SerializedSelected);
			}

			CommandPrimer.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		public override bool CanCommand (string key) {
			string[] splitKey = key.Split("/");
			if (splitKey[0] == "construct") return true;

			return base.CanCommand(key);
		}
	}
}