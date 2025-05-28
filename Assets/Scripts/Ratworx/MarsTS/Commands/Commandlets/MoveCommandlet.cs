using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets {

    public class MoveCommandlet : Commandlet<Vector3> {
	    public override string SerializerKey => "move";

        protected override void Deserialize(SerializedCommandWrapper data) {
	        SerializedMoveCommandlet deserialized = (SerializedMoveCommandlet)data.commandletData;

	        Name = data.Key;
	        Commander = TeamCache.Faction(data.Faction);
	        _target = deserialized.TargetPosition;
        }
    }
}