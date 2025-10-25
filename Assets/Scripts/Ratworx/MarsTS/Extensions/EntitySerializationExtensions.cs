using System.Linq;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.WorldObject;
using Unity.Collections;
using Unity.Netcode;

namespace Ratworx.MarsTS.Extensions
{
    /// <summary>
    /// This class provides extension methods for Unity's <see cref="FastBufferReader"/> and
    /// <see cref="FastBufferWriter"/> classes to Serialize the <see cref="Entity"/> class without resulting in
    /// erroneous Entity object constructions. These methods will transfer only the <see cref="Entity.Id"/> of the
    /// Entity class, and then utilize the <see cref="EntityCache"/> to retrieve the Entity instance on the receiver
    /// side.
    /// </summary>
    public static class EntitySerializationExtensions
    {
        public static void ReadValueSafe(this FastBufferReader reader, out Entity entity) {
            reader.ReadValueSafe(out int id);

            if (!EntityCache.TryGetEntity(id, out entity))
                RatLogger.Error?.Log($"Error reading entity with Id {id} from network call!");
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in Entity entity)
            => writer.WriteValueSafe(entity.Id);

        public static void ReadValueSafe<T>(this FastBufferReader reader, out T unit) where T : IUnitInterface {
            reader.ReadValueSafe(out int id);

            if (EntityCache.TryGetEntity(id, out Entity entity)
                && entity.TryGetEntityComponent(out unit)) 
                return;
            
            RatLogger.Error?.Log($"Error reading entity with Id {id} from network call!");
            unit = default;
        }

        public static void WriteValueSafe<T>(this FastBufferWriter writer, in T unit) where T : IUnitInterface
            => writer.WriteValueSafe(unit.Entity.Id);

        public static void ReadValueSafe(this FastBufferReader reader, out IAttackable unit)
            => ReadValueSafe<IAttackable>(reader, out unit);

        public static void WriteValueSafe(this FastBufferWriter writer, in IAttackable unit)
            => WriteValueSafe<IAttackable>(writer, unit);
        
        public static void ReadValueSafe(this FastBufferReader reader, out ISelectable unit)
            => ReadValueSafe<ISelectable>(reader, out unit);

        public static void WriteValueSafe(this FastBufferWriter writer, in ISelectable unit)
            => WriteValueSafe<ISelectable>(writer, unit);
        
        public static void ReadValueSafe(this FastBufferReader reader, out IDepositable unit)
            => ReadValueSafe<IDepositable>(reader, out unit);

        public static void WriteValueSafe(this FastBufferWriter writer, in IDepositable unit)
            => WriteValueSafe<IDepositable>(writer, unit);

        public static void ReadValueSafe(this FastBufferReader reader, out IHarvestable unit)
            => ReadValueSafe<IHarvestable>(reader, out unit);

        public static void WriteValueSafe(this FastBufferWriter writer, in IHarvestable unit)
            => WriteValueSafe<IHarvestable>(writer, unit);

        public static void ReadValueSafe(this FastBufferReader reader, out ProductionOption option) {
            option = new ProductionOption();
            
            reader.ReadValueSafe(out option.OptionKey);
            reader.ReadValueSafe(out option.ProductionRequired);
            reader.ReadValueSafe(out option.ProductKey);
            reader.ReadValueSafe(out option.ProductionType);
            reader.ReadValueSafe(out option.Cost);
            reader.ReadValueSafe(out option.Description);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in ProductionOption option) {
            writer.WriteValueSafe(option.OptionKey);
            writer.WriteValueSafe(option.ProductionRequired);
            writer.WriteValueSafe(option.ProductKey);
            writer.WriteValueSafe(option.ProductionType);
            writer.WriteValueSafe(option.Cost);
            writer.WriteValueSafe(option.Description);
        }
        
        public static void ReadValueSafe(this FastBufferReader reader, out ConstructionOption option) {
            option = new ConstructionOption();
            
            reader.ReadValueSafe(out option.OptionKey);
            reader.ReadValueSafe(out option.ConstructionRequired);
            reader.ReadValueSafe(out option.BuildingKey);
            reader.ReadValueSafe(out option.Cost);
            reader.ReadValueSafe(out option.Description);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in ConstructionOption option) {
            writer.WriteValueSafe(option.OptionKey);
            writer.WriteValueSafe(option.ConstructionRequired);
            writer.WriteValueSafe(option.BuildingKey);
            writer.WriteValueSafe(option.Cost);
            writer.WriteValueSafe(option.Description);
        }

        public static void ReadValueSafe(this FastBufferReader reader, out ResourceCost[] costs) {
            reader.ReadValueSafe(out NativeArray<FixedString32Bytes> keys, Allocator.Temp);
            reader.ReadValueSafe(out int[] amounts);

            costs = new ResourceCost[keys.Length];

            for (int i = 0; i < keys.Length; i++) {
                costs[i] = new ResourceCost
                {
                    key = keys[i].ToString(),
                    amount = amounts[i]
                };
            }
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in ResourceCost[] costs) {
            string[] keys = costs.Select(cost => cost.key).ToArray();
            var resourceKeys = new NativeArray<FixedString32Bytes>(keys.Length, Allocator.Temp);

            for (int i = 0; i < resourceKeys.Length; i++) {
                resourceKeys[i] = keys[i];
            }

            int[] amounts = costs.Select(cost => cost.amount).ToArray();
            
            writer.WriteValueSafe(resourceKeys);
            writer.WriteValueSafe(amounts);
        }
    }
}