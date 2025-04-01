using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Networking {

    public static class NetworkObjectReferenceExtensions {

        public static GameObject GameObject (this NetworkObjectReference reference) {
            return (GameObject)reference;
        }
    }
}