using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Research
{
    public class Technology : NetworkBehaviour
    {
        public string Key => _key;

        [FormerlySerializedAs("key")] [SerializeField]
        private string _key;

        protected Faction _owner;

        protected virtual void Start()
        {
            _owner = GetComponentInParent<Faction>();
        }
    }
}