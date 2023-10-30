using MarsTS.Prefabs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Roster {

        public string Type { get; private set; }
        public List<string> Commands { get; private set; }

		private Dictionary<int, ISelectable> instances;

		public int Count {
            get {
                return instances.Count;
            }
        }

        public Roster (string type, params ISelectable[] units) {
            Type = type;

			instances = new Dictionary<int, ISelectable>();
			Commands = new List<string>();

            Commands.AddRange(UnitRegistry.Unit(Type).Commands());

            foreach (ISelectable unit in units) {
                TryAdd(unit);
            }
		}

        public ISelectable Get (int id) {
            return instances.TryGetValue(id, out ISelectable unit) ? unit : null;
        }

        public List<ISelectable> List () {
            return new List<ISelectable>(instances.Values);
        }

        public bool TryAdd (ISelectable entity) {
            if (!entity.Name().Equals(Type)) {
				Debug.LogWarning("Unit type " + entity.Name() + " doesn't match roster's registered type of " + Type + "!");
                return false;
			}

            if (!instances.TryAdd(entity.ID, entity)) {
                //Debug.LogWarning("Unit " + unit.Id() + " already added to Roster of " + Type + " type!");
                return false;
            }

            return true;
        }

        public void Remove (params int[] ids) {
            foreach (int id in ids) {
                instances.Remove(id);
            }
        }

        public void Clear () {
            instances.Clear();
        }
    }
}