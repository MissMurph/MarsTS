using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings {

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

		private Transform spawnPoint;
		
		

		protected override void Awake () {
			base.Awake();

			//colliders = new List<Collider>(model.GetComponentsInChildren<Collider>());
			//colliders.AddRange(transform.Find("Collider").GetComponentsInChildren<Collider>());

			spawnPoint = transform.Find("SpawnPoint");
		}

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

			//exitOrder = CommandRegistry.Get<Move>("move").Construct(transform.position + (Vector3.forward * 10f));
		}

		protected virtual void Produce (CommandStartEvent _event) {
			Bus.AddListener<CommandCompleteEvent>(ProductionComplete);
		}

		protected virtual void ProductionComplete (CommandCompleteEvent _event) {
			Bus.RemoveListener<CommandCompleteEvent>(ProductionComplete);

			if (_event.IsCancelled) return;

			IProducable order = _event.Command as IProducable;

			if (NetworkManager.Singleton.IsServer) {
				
				ISelectable newUnit = Instantiate(order.Product, spawnPoint.position + (Vector3.up), Quaternion.Euler(0f, 0f, 0f)).GetComponent<ISelectable>();

				newUnit.GameObject.GetComponent<NetworkObject>().Spawn();

				//List<Collider> unitColliders = newUnit.GameObject.transform.Find("Model").GetComponentsInChildren<Collider>().ToList();
				//unitColliders.AddRange(newUnit.GameObject.transform.Find("Collider").GetComponentsInChildren<Collider>());

				/*List<Collider> unitColliders = newUnit.GameObject.GetComponentsInChildren<Collider>().ToList();


				foreach (Collider unitCollider in unitColliders) {
					foreach (Collider buildingCollider in colliders) {
						Physics.IgnoreCollision(unitCollider, buildingCollider, true);
					}
				}*/

				newUnit.SetOwner(Owner);

				ICommandable commandable = newUnit as ICommandable;

				//Commandlet exit = exitOrder.Clone();
				//exit.Callback.AddListener(UnitExitCallback);
				//commandable.Order(exit, false);

				foreach (Commandlet newCommand in rallyOrders) {
					//commandable.Order(newCommand.Clone(), true);
				}
			}

			//CurrentCommand = null;
			Bus.Global(new ProductionCompleteEvent(Bus, order.Product, this, production, order));
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

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
			commands.Clear();
			production.Clear();
			rallyOrders.Clear();
		}

		protected void UnitExitCallback (CommandCompleteEvent _event) {
			/*Collider[] unitColliders = _event.Unit.GameObject.transform.Find("Model").GetComponentsInChildren<Collider>();

			foreach (Collider unitCollider in unitColliders) {
				foreach (Collider buildingCollider in colliders) {
					Physics.IgnoreCollision(unitCollider, buildingCollider, false);
				}
			}

			_event.Command.Callback.RemoveListener(UnitExitCallback);*/
		}
	}
}