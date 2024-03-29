using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.WSA;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Units {

    public class RoughneckSquad : InfantrySquad {

		public int Stored { get { return storageComp.Amount; } }

		public int Capacity { get { return storageComp.Capacity; } }

		public ResourceStorage storageComp;

		private Transform resourceBar;

		protected override void Awake () {
			base.Awake();

			resourceBar = transform.Find("BarOrientation");
			storageComp = GetComponent<ResourceStorage>();
		}

		protected override void Update () {
			base.Update();

			foreach (MemberEntry entry in members.Values) {
				resourceBar.transform.position = entry.member.transform.position;
				break;
			}
		}

		protected override void RegisterMember (InfantryMember unit) {
			base.RegisterMember(unit);

			EventAgent unitEvents = unit.GetComponent<EventAgent>();
			unitEvents.AddListener<ResourceHarvestedEvent>(OnMemberHarvest);
			unitEvents.AddListener<HarvesterDepositEvent>(OnMemberDeposit);
		}

		public void OnMemberHarvest (ResourceHarvestedEvent _event) {
			bus.Global(new ResourceHarvestedEvent(bus, _event.Deposit, this, ResourceHarvestedEvent.Side.Harvester, _event.HarvestAmount, _event.Resource, Stored, Capacity));
		}

		public void OnMemberDeposit (HarvesterDepositEvent _event) {
			bus.Global(new HarvesterDepositEvent(bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity, _event.Bank));
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "sneak":
					SquadSneak(order);
					return;
				case "attack":

					break;
				case "repair":

					break;
				case "harvest":

					break;
				case "deposit":

					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		private void SquadSneak (Commandlet order) {
			if (!CanCommand(order.Name)) return;
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			commands.Activate(order, deserialized.Target);

			foreach (MemberEntry entry in members.Values) {
				entry.member.Order(order, false);
			}
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IHarvestable harvestable
				&& Stored < Capacity
				&& harvestable.StoredAmount > 0
				&& harvestable.CanHarvest(storageComp.Resource, this)) {
				return CommandRegistry.Get("harvest");
			}

			if (target is IDepositable
			&& Stored > 0) {
				return CommandRegistry.Get("deposit");
			}

			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IHarvestable harvestable
				&& Stored < Capacity
				&& harvestable.StoredAmount > 0
				&& harvestable.CanHarvest(storageComp.Resource, this)) {
				return CommandRegistry.Get<Harvest>("harvest").Construct(harvestable);
			}

			if (target is IDepositable depositable
				&& Stored > 0) {
				return CommandRegistry.Get<Deposit>("deposit").Construct(depositable);
			}

			if (target is IAttackable attackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(attackable);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		protected override void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				base.OnUnitInfoDisplayed(_event);

				StorageInfo storage = _event.Info.Module<StorageInfo>("storage");
				storage.CurrentUnit = this;
				storage.CurrentValue = Stored;
				storage.MaxValue = Capacity;
			}
		}

		public override bool CanCommand (string key) {
			if (key == "deposit") return true;

			return base.CanCommand(key);
		}
	}
}