using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public interface ICommandSerializer {

        public string Key { get; }
        ISerializedCommand Reader ();
        ISerializedCommand Writer (Commandlet _data);
    }
}