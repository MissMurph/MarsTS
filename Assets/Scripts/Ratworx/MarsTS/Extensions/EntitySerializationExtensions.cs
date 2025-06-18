using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Units;
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
    }
}