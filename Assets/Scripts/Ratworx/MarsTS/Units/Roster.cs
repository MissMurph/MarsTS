using System;
using System.Collections;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Logging;

namespace Ratworx.MarsTS.Units
{
    public class Roster : IEnumerable<ISelectable>
    {
        public string RegistryKey { get; private set; }
        public string RegistryType { get; private set; }
        public List<string> Commands { get; }

        public List<ICommandable> Orderable
        {
            get
            {
                if (commandables == null) return new List<ICommandable>();

                List<ICommandable> output = new List<ICommandable>();

                foreach (ICommandable commandable in commandables.Values)
                {
                    output.Add(commandable);
                }

                return output;
            }
        }

        private readonly Dictionary<int, ISelectable> instances;
        private Dictionary<int, ICommandable> commandables;

        public int Count => instances.Count;

        public Roster(string registryKey, ISelectable[] units)
        {
            RegistryKey = registryKey;

            instances = new Dictionary<int, ISelectable>();
            Commands = new List<string>();

            //Commands.AddRange(Registry.Get<ISelectable>(registryKey).Commands());

            //Commands.AddRange(UnitRegistry.Unit(Type).Commands());
            //Commands.AddRange(units[0].Commands());

            foreach (ISelectable unit in units)
            {
                if (TryAdd(unit))
                    if (unit is ICommandable orderable)
                    {
                        if (Commands.Count == 0) Commands.AddRange(orderable.Commands());
                        if (commandables == null) commandables = new Dictionary<int, ICommandable>();

                        commandables[unit.Entity.Id] = orderable;
                    }
            }
        }

        public Roster()
        {
            instances = new Dictionary<int, ISelectable>();
            Commands = new List<string>();
        }
        
        public ISelectable GetFirst()
        {
            foreach (ISelectable unit in instances.Values)
            {
                return unit;
            }

            return null;
        }

        public ISelectable Get(int id) => instances.TryGetValue(id, out ISelectable unit) ? unit : null;

        public List<ISelectable> List() => new List<ISelectable>(instances.Values);

        public bool TryAdd(ISelectable unit)
        {
            if (string.IsNullOrEmpty(RegistryKey))
            {
                RegistryKey = unit.Entity.RegistryKey;
                RegistryType = unit.Entity.RegistryType;
            }

            if (!unit.Entity.RegistryKey.Equals(RegistryKey))
            {
                RatLogger.Error?.Log($"Unit type {unit.Entity.RegistryKey} doesn't match roster's registered type of {RegistryKey}!");
                return false;
            }

            if (!instances.TryAdd(unit.Entity.Id, unit)) {
                RatLogger.Message?.Log($"Unit {unit.Entity.Id} already added to Roster of {RegistryType} type!");
                return false;
            }

            if (unit is ICommandable orderable)
            {
                if (Commands.Count == 0) Commands.AddRange(orderable.Commands());
                if (commandables == null) commandables = new Dictionary<int, ICommandable>();

                commandables[unit.Entity.Id] = orderable;
            }

            return true;
        }

        public void Remove(params int[] ids)
        {
            foreach (int id in ids)
            {
                instances.Remove(id);
            }
        }

        public bool Contains(int id) => instances.ContainsKey(id);

        public void Clear()
        {
            instances.Clear();
        }

        public IEnumerator<ISelectable> GetEnumerator() => instances.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}