using System;
using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;

namespace MarsTS.Units
{
    public class MarineSquad : InfantrySquad
    {
        public override void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "attack":

                    break;
                case "adrenaline":
                    SquadBoost(order);
                    return;
                default:
                    base.Order(order, inclusive);
                    return;
            }

            if (inclusive) _commands.Enqueue(order);
            else _commands.Execute(order);
        }

        /*	Adrenaline	*/

        private void SquadBoost(Commandlet order)
        {
            if (!CanCommand(order.Name)) return;
            var deserialized = order as Commandlet<bool>;

            _commands.Activate(order, deserialized.Target);

            _bus.AddListener<CooldownEvent>(AdrenalineCooldown);

            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Order(order, false);
            }
        }

        private void AdrenalineComplete(CommandActiveEvent _event)
        {
            _bus.RemoveListener<CommandActiveEvent>(AdrenalineComplete);

            if (!_event.Activity)
                foreach (MemberEntry entry in _members.Values)
                {
                    entry.Bus.Local(_event);
                }
        }

        private void AdrenalineCooldown(CooldownEvent _event)
        {
            if (_commands.Active.Contains(_event.CommandKey) && _event.Complete)
            {
                _bus.RemoveListener<CooldownEvent>(AdrenalineCooldown);

                foreach (MemberEntry entry in _members.Values)
                {
                    entry.Bus.Local(_event);
                }

                _commands.Deactivate(_event.CommandKey);
            }
        }

        public override CommandFactory Evaluate(ISelectable target)
        {
            if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile)
                return CommandRegistry.Get("attack");

            return CommandRegistry.Get("move");
        }

        public override void AutoCommand(ISelectable target)
        {
            if (target is IAttackable attackable && target.GetRelationship(Owner) == Relationship.Hostile)
            {
                //return CommandRegistry.Get<Attack>("attack").Construct(attackable);
            }

            //return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);

            throw new NotImplementedException();
        }

        public override bool CanCommand(string key)
        {
            if (key == "adrenaline") 
                return Owner.IsResearched("adrenaline");

            return base.CanCommand(key);
        }
    }
}