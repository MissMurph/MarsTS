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

        private readonly Dictionary<int, Entity> _instances;
        public int Count => _instances.Count;

        public Roster(string registryKey, Entity[] units) {
            RegistryKey = registryKey;
            RegistryType = units[0].RegistryType;
            _instances = new Dictionary<int, Entity>();
        }

        public Roster() => _instances = new Dictionary<int, Entity>();

        public Entity GetFirst() => _instances.Values.FirstOrDefault();

        public Entity Get(int id) => _instances.GetValueOrDefault(id);

        public List<Entity> List() => new List<Entity>(_instances.Values);
    
        public bool TryAdd(Entity unit) {
            if (string.IsNullOrEmpty(RegistryKey)) {
                RegistryKey = unit.RegistryKey;
                RegistryType = unit.RegistryType;
            }

            if (!unit.RegistryKey.Equals(RegistryKey)) {
                RatLogger.Error?.Log($"Unit type {unit.RegistryKey} doesn't match roster's registered type of {RegistryKey}!");
                return false;
            }

            if (_instances.TryAdd(unit.Id, unit)) return true;

            RatLogger.Message?.Log($"Unit {unit.Id} already added to Roster of {RegistryType} type!");
            return false;
        }

        public void Remove(params int[] ids) {
            foreach (int id in ids) {
                _instances.Remove(id);
            }
        }

        public bool Contains(int id) => _instances.ContainsKey(id);

        public void Clear() => _instances.Clear();

        public IEnumerator<Entity> GetEnumerator() => _instances.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}