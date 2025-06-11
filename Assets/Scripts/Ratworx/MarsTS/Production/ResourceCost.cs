using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

namespace Ratworx.MarsTS.Production
{
    [Serializable]
    public class ResourceCost
    {
        public string key;
        public int amount;
    }

    public struct SerializedResourceCosts : INetworkSerializable
    {
        public NativeArray<FixedString32Bytes> ResourceKeys;
        public int[] Amounts;

        public SerializedResourceCosts(ResourceCost[] costs) {
            string[] keys = costs.Select(cost => cost.key).ToArray();
            ResourceKeys = new NativeArray<FixedString32Bytes>(keys.Length, Allocator.Temp);

            for (int i = 0; i < ResourceKeys.Length - 1; i++) {
                ResourceKeys[i] = keys[i];
            }

            Amounts = costs.Select(cost => cost.amount).ToArray();
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref ResourceKeys, Allocator.Temp);
            serializer.SerializeValue(ref Amounts);
        }

        public ResourceCost[] GetDeserializedCosts() {
            var output = new ResourceCost[ResourceKeys.Length];

            for (int i = 0; i < ResourceKeys.Length - 1; i++) {
                output[i] = new ResourceCost
                {
                    key = ResourceKeys[i].ToString(),
                    amount = Amounts[i],
                };
            }

            return output;
        }
    }
}