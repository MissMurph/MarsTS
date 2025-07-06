using System;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Production
{
    [Serializable]
    public class ProductionOption
    {
        public string OptionKey;
        public int ProductionRequired;
        /// <summary>RegistryKey to the product prefab</summary>
        public string ProductKey;
        // TODO: Turn below into enum
        /// <summary>Pre-defined types are: <br/><c>production</c><br/><c>research</c><br/><c>upgrade</c></summary>
        public string ProductionType;
        public ResourceCost[] Cost;
        public string Description;
    }

    public struct SerializedProductionOption : INetworkSerializable
    {
        public string OptionKey;
        public int ProductionRequired;
        public string ProductKey;
        public string ProductionType;
        public string Description;
        public SerializedResourceCosts Cost;

        public SerializedProductionOption(ProductionOption option) {
            OptionKey = option.OptionKey;
            ProductionRequired = option.ProductionRequired;
            ProductKey = option.ProductKey;
            ProductionType = option.ProductionType;
            Description = option.Description;
            Cost = new SerializedResourceCosts(option.Cost);
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref OptionKey);
            serializer.SerializeValue(ref ProductionRequired);
            serializer.SerializeValue(ref ProductKey);
            serializer.SerializeValue(ref ProductionType);
            serializer.SerializeValue(ref Description);
            serializer.SerializeValue(ref Cost);
        }

        public ProductionOption GetDeserializedOption() => new ProductionOption
        {
            OptionKey = OptionKey,
            ProductionRequired = ProductionRequired,
            ProductKey = ProductKey,
            ProductionType = ProductionType,
            Description = Description,
            Cost = Cost.GetDeserializedCosts()
        };
    }
}