using System;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities {
    public class EntityAttribute : NetworkBehaviour, IEntityComponent<EntityAttribute> {
        public event Action<int, int> OnAttributeChange;

        public virtual int Value
        {
            get => _internalValue.Value;
            set => _internalValue.Value = value;
        }

        [SerializeField]
        protected string _key;

        [SerializeField]
        protected int _startingValue;

        [SerializeField]
        private NetworkVariable<int> _internalValue =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        public virtual string Key => "attribute:" + _key;

        public EntityAttribute Get() => this;

        public override void OnNetworkSpawn() {
            if (NetworkManager.Singleton.IsServer)
                Value = _startingValue;

            _internalValue.OnValueChanged += OnValueChange;
        }

        private void OnValueChange(int oldValue, int newValue) => OnAttributeChange?.Invoke(oldValue, newValue);
    }
}