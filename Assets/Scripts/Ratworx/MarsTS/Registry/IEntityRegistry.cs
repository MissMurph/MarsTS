using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Registry {
    public interface IEntityRegistry<T> : IEntityRegistry {
        event Action<string, T> OnEntityObjectRegistered;
        bool TryRegisterEntity(string key, T entity);
        bool TryGetRegisteredEntity(string key, out Entity entity);
        List<T> GetAllEntityObjects();
    }
    
    public interface IEntityRegistry : IRegistry {
        event Action<string, Entity> OnEntityRegistered;
        bool TryRegisterEntity(string key, Entity entity);
        bool TryGetRegisteredEntity(string key, out Entity entity);
        List<Entity> GetAllEntities();
    }
}