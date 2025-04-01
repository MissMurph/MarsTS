using System.Collections.Generic;
using MarsTS.Events;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {
    public class FlareCommandlet : Commandlet<Vector3> {

        private readonly NetworkVariable<float> _cooldown = new(writePerm: NetworkVariableWritePermission.Server);

        private Dictionary<string, int> _cost = new Dictionary<string, int>();

        public void InitFlare (string commandName, Vector3 target, Faction commander, float cooldown, CostEntry[] cost) {
            _cooldown.Value = cooldown;

            foreach (CostEntry entry in cost) {
                _cost[entry.key] = entry.amount;
            }
            
            Init(commandName, target, commander);
        }

        public override string SerializerKey => "move";

        public override void CompleteCommand (EventAgent agent, ICommandable unit, bool isCancelled = false)
        {
            if (!TryGetQueue(unit, out var queue)) return;

            if (isCancelled) {
                foreach (KeyValuePair<string, int> entry in _cost) {
                    Commander.GetResource(entry.Key).Deposit(entry.Value);
                }
            }
            else
                queue.Cooldown(this, _cooldown.Value);

            base.CompleteCommand(agent, unit, isCancelled);
        }

        public override Commandlet Clone()
        {
            throw new System.NotImplementedException();
        }

        protected override void Deserialize(SerializedCommandWrapper data) {
            SerializedMoveCommandlet deserialized = (SerializedMoveCommandlet)data.commandletData;

            Name = data.Name;
            Commander = TeamCache.Faction(data.Faction);
            _target = deserialized.TargetPosition;
        }
    }
}