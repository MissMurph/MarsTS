using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Factory : Building {

		//How many steps will occur per second
		[SerializeField]
		protected float productionSpeed;

		private float stepTime;
		private float timeToStep;

		public Queue<ProductionCommandlet> ProductionQueue { get; protected set; } 

		public ProductionCommandlet CurrentProduction { get; protected set; }

		public override Commandlet CurrentCommand { get { return CurrentProduction; } }

		public override Commandlet[] CommandQueue { get { return ProductionQueue.ToArray(); } }

		protected Queue<Commandlet> rallyOrders = new Queue<Commandlet>();

		protected Commandlet exitOrder;

		[SerializeField]
		protected string[] rallyCommands;

		private List<Collider> colliders;

		[SerializeField]
		private GameObject queueInfo;

		protected override void Awake () {
			base.Awake();

			colliders = new List<Collider>(model.GetComponentsInChildren<Collider>());

			ProductionQueue = new Queue<ProductionCommandlet>();

			stepTime = 1f / productionSpeed;
			timeToStep = stepTime;
		}

		protected override void Start () {
			base.Start();

			exitOrder = CommandRegistry.Get<Move>("move").Construct(transform.position + (Vector3.forward * 5f));

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
		}

		protected void Update () {
			UpdateQueue();

			if (CurrentProduction != null) {
				timeToStep -= Time.deltaTime;

				if (timeToStep <= 0) {
					CurrentProduction.ProductionProgress++;

					if (CurrentProduction.ProductionProgress >= CurrentProduction.ProductionRequired) {
						ProduceUnit();
					}
					else bus.Global(new ProductionStepEvent(bus, CurrentProduction, this));

					timeToStep += stepTime;
				}
			}
		}

		protected void UpdateQueue () {
			if (CurrentProduction is null && ProductionQueue.TryDequeue(out ProductionCommandlet order)) {

				CurrentProduction = order;

				bus.Global(new ProductionStartedEvent(bus, CurrentProduction, ProductionQueue.ToArray(), this));
			}
		}

		protected virtual void ProduceUnit () {
			ISelectable newUnit = Instantiate(CurrentProduction.Prefab, transform.position + (Vector3.up), Quaternion.Euler(0f, 0f, 0f)).GetComponent<ISelectable>();

			Collider[] unitColliders = newUnit.GameObject.transform.Find("Model").GetComponentsInChildren<Collider>();

			foreach (Collider unitCollider in unitColliders) {
				foreach (Collider buildingCollider in colliders) {
					Physics.IgnoreCollision(unitCollider, buildingCollider, true);
				}
			}

			newUnit.SetOwner(owner);

			ICommandable commandable = newUnit as ICommandable;

			Commandlet exit = exitOrder.Clone();
			exit.Callback.AddListener(UnitExitCallback);
			commandable.Execute(exit);

			foreach (Commandlet order in rallyOrders) {
				commandable.Enqueue(order.Clone());
			}

			CurrentProduction = null;
			bus.Global(new ProductionEvent(bus, newUnit, this));
		}

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "produce":
					Produce(order);
					break;
				case "stop":
					Stop();
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		protected virtual void Produce (Commandlet order) {
			ProductionCommandlet produceOrder = order as ProductionCommandlet;

			ProductionQueue.Enqueue(produceOrder);

			bus.Global(new ProductionQueueEvent(bus, ProductionQueue.ToArray(), this));
		}

		protected virtual void Stop () {
			ProductionQueue.Clear();
			CurrentProduction = null;
		}

		protected void UnitExitCallback (CommandCompleteEvent _event) {
			Collider[] unitColliders = _event.Unit.GameObject.transform.Find("Model").GetComponentsInChildren<Collider>();

			foreach (Collider unitCollider in unitColliders) {
				foreach (Collider buildingCollider in colliders) {
					Physics.IgnoreCollision(unitCollider, buildingCollider, false);
				}
			}

			_event.Command.Callback.RemoveListener(UnitExitCallback);
		}

		public override void Enqueue (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			foreach (string command in rallyCommands) {
				if (order.Name == command) {
					rallyOrders.Enqueue(order);
					return;
				}
			}

			Execute(order);
		}

		public override void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			foreach (string command in rallyCommands) {
				if (order.Name == command) {
					rallyOrders.Clear();
					rallyOrders.Enqueue(order);
					return;
				}
			}

			ProcessOrder(order);
		}

		protected override void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				_event.Info.Module<ProductionQueue>("productionQueue").SetQueue(this, CurrentProduction, ProductionQueue.ToArray());
			}

			base.OnUnitInfoDisplayed(_event);
		}
	}
}