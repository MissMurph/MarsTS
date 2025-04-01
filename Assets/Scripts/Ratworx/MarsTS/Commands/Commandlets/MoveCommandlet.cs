using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Teams;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets {

    public class MoveCommandlet : Commandlet<Vector3> {
	    public override string SerializerKey => "move";

	    public override Commandlet Clone()
        {
	        throw new System.NotImplementedException();
        }

        protected override void Deserialize(SerializedCommandWrapper _data) {
	        SerializedMoveCommandlet deserialized = (SerializedMoveCommandlet)_data.commandletData;

	        Name = _data.Key;
	        Commander = TeamCache.Faction(_data.Faction);
	        _target = deserialized.TargetPosition;
        }
    }
}