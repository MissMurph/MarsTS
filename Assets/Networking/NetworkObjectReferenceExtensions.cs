using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Networking {

    public static class NetworkObjectReferenceExtensions {

        public static GameObject GameObject (this NetworkObjectReference reference) {
            return (GameObject)reference;
        }
    }
}