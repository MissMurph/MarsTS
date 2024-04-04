using MarsTS.Commands;
using MarsTS.Prefabs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Roster {

        public string RegistryKey { get; private set; }
        public Type Type { get; private set; }
        public List<string> Commands { get; private set; }
        public List<ICommandable> Orderable {
            get {
                if (commandables == null) return new();

                List<ICommandable> output = new();

                foreach (ICommandable commandable in commandables.Values) {
                    output.Add(commandable);
                }

                return output;
            }
        }

		private Dictionary<int, ISelectable> instances;
        private Dictionary<int, ICommandable> commandables;

		public int Count {
            get {
                return instances.Count;
            }
        }

        public Roster (string registryKey, ISelectable[] units) {
            RegistryKey = registryKey;

			instances = new Dictionary<int, ISelectable>();
			Commands = new List<string>();

            //Commands.AddRange(Registry.Get<ISelectable>(registryKey).Commands());

            //Commands.AddRange(UnitRegistry.Unit(Type).Commands());
            //Commands.AddRange(units[0].Commands());

            foreach (ISelectable unit in units) {
                if (TryAdd(unit)) {
                    if (unit is ICommandable orderable) {
                        if (Commands.Count == 0) Commands.AddRange(orderable.Commands());
						if (commandables == null) commandables = new Dictionary<int, ICommandable>();

                        commandables[unit.ID] = orderable;
                    }
                }
            }
		}

        public Roster () {
			instances = new Dictionary<int, ISelectable>();
			Commands = new List<string>();
		}

        public ISelectable Get () {
            foreach (ISelectable unit in instances.Values) {
                return unit;
            }

            return null;
        }

        public ISelectable Get (int id) {
            return instances.TryGetValue(id, out ISelectable unit) ? unit : null;
        }

        public List<ISelectable> List () {
            return new List<ISelectable>(instances.Values);
        }

        public bool TryAdd (ISelectable entity) {
            if (RegistryKey == null) {
                RegistryKey = entity.RegistryKey;
            }

            if (!entity.RegistryKey.Equals(RegistryKey)) {
				Debug.LogWarning("Unit type " + entity.RegistryKey + " doesn't match roster's registered type of " + RegistryKey + "!");
                return false;
			}

            if (!instances.TryAdd(entity.ID, entity)) {
				//Debug.LogWarning("Unit " + unit.Id() + " already added to Roster of " + Type + " type!");
				return false;
			}

            if (entity is ICommandable orderable) {
                if (Commands.Count == 0) Commands.AddRange(orderable.Commands());
				if (commandables == null) commandables = new Dictionary<int, ICommandable>();

				commandables[entity.ID] = orderable;
			}

            return true;
        }

        public void Remove (params int[] ids) {
            foreach (int id in ids) {
                instances.Remove(id);
            }
        }

        public bool Contains (int id) {
            return instances.ContainsKey(id);
        }

        public void Clear () {
            instances.Clear();
        }
    }
}