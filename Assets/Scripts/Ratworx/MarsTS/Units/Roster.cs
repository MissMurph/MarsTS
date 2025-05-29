using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;

namespace Ratworx.MarsTS.Units
{
    public class Roster : IEnumerable<Entity>
    {
        public string RegistryKey { get; private set; }
        public string RegistryType { get; private set; }

        private readonly Dictionary<int, Entity> instances;
        public int Count => instances.Count;

        public Roster(string registryKey, Entity[] units) {
            RegistryKey = registryKey;
            RegistryType = units[0].RegistryType;
            instances = new Dictionary<int, Entity>();
        }

        public Roster() => instances = new Dictionary<int, Entity>();

        public Entity GetFirst() => instances.Values.FirstOrDefault();

        public Entity Get(int id) => instances.GetValueOrDefault(id);

        public List<Entity> List() => new List<Entity>(instances.Values);

        public bool TryAdd(Entity unit) {
            if (string.IsNullOrEmpty(RegistryKey)) {
                RegistryKey = unit.RegistryKey;
                RegistryType = unit.RegistryType;
            }

            if (!unit.RegistryKey.Equals(RegistryKey)) {
                RatLogger.Error?.Log($"Unit type {unit.RegistryKey} doesn't match roster's registered type of {RegistryKey}!");
                return false;
            }

            if (instances.TryAdd(unit.Id, unit)) return true;

            RatLogger.Message?.Log($"Unit {unit.Id} already added to Roster of {RegistryType} type!");
            return false;
        }

        public void Remove(params int[] ids) {
            foreach (int id in ids) {
                instances.Remove(id);
            }
        }

        public bool Contains(int id) => instances.ContainsKey(id);

        public void Clear() => instances.Clear();

        public IEnumerator<Entity> GetEnumerator() => instances.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}