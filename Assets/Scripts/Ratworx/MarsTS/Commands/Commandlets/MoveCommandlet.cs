using MarsTS.Prefabs;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

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