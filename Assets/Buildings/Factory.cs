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

		protected Queue<Commandlet> rallyOrders = new Queue<Commandlet>();

		//This queue is required so that timing isn't broken with the Player selection code, if a unit calls its death
		//While the selection event is still processing the collection it'll crash
		//protected Queue<Commandlet> newCommands = new Queue<Commandlet>();

		protected Commandlet exitOrder;

		[SerializeField]
		protected string[] rallyCommands;

		private List<Collider> colliders;

		[SerializeField]
		private GameObject queueInfo;

		protected override void Awake () {
			base.Awake();

			colliders = new List<Collider>(model.GetComponentsInChildren<Collider>());
		}

		protected override void Start () {
			base.Start();

			exitOrder = CommandRegistry.Get<Move>("move").Construct(transform.position + (Vector3.forward * 10f));
		}

		/*protected override void Update () {
			UpdateQueue();

			if (CurrentCommand != null && CurrentCommand is ProductionCommandlet production) {
				timeToStep -= Time.deltaTime;

				if (timeToStep <= 0) {
					production.ProductionProgress++;

					if (production.ProductionProgress >= production.ProductionRequired) {
						ProduceUnit(production);
					}
					else bus.Global(ProductionEvent.Step(bus, this));

					timeToStep += stepTime;
				}
			}
		}*/

		protected virtual void Produce (CommandStartEvent _event) {
			bus.AddListener<CommandCompleteEvent>(ProductionComplete);
		}

		protected virtual void ProductionComplete (CommandCompleteEvent _event) {
			bus.RemoveListener<CommandCompleteEvent>(ProductionComplete);

			IProducable order = _event.Command as IProducable;
			ISelectable newUnit = Instantiate(order.Product, transform.position + (Vector3.up), Quaternion.Euler(0f, 0f, 0f)).GetComponent<ISelectable>();

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
			commandable.Order(exit, false);

			foreach (Commandlet newCommand in rallyOrders) {
				commandable.Order(newCommand.Clone(), true);
			}

			//CurrentCommand = null;
			bus.Global(new ProductionCompleteEvent(bus, newUnit.GameObject, this, production, order));
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "stop":

					break;
				case "move":

					break;
				case "produce":
					production.Enqueue(order);
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) rallyOrders.Enqueue(order);
			else {
				rallyOrders.Clear();
				rallyOrders.Enqueue(order);
			}
		}

		protected override void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "produce":
					Produce(_event);
					break;
				case "stop":
					Stop();
					break;
				default:
					base.ExecuteOrder(_event);
					break;
			}
		}

		protected virtual void Stop () {
			//CommandCompleteEvent currentCancel = new CommandCompleteEvent(bus, CurrentCommand, true, this);

			//CurrentCommand.Callback.Invoke(currentCancel);

			//bus.Global(currentCancel);

			/*foreach (Commandlet order in commandQueue) {
				order.Callback.Invoke(new CommandCompleteEvent(bus, order, true, this));
			}*/

			//commandQueue.Clear();
			//CurrentCommand = null;

			commands.Clear();
			production.Clear();
			rallyOrders.Clear();
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
	}
}