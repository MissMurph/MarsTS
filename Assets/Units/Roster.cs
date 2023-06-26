using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Roster {
        public string Type;
        private Dictionary<int, ISelectable> map = new Dictionary<int, ISelectable>();

        public int Count {
            get {
                return map.Count;
            }
        }

        public ISelectable Get (int id) {
            return map.TryGetValue(id, out ISelectable unit) ? unit : null;
        }

        public List<ISelectable> List () {
            return new List<ISelectable>(map.Values);
        }

        public bool TryAdd (ISelectable unit) {
            return !unit.Type().Equals(Type) && map.TryAdd(unit.Id(), unit);
        }

        public void Remove (params int[] ids) {
            foreach (int id in ids) {
                map.Remove(id);
            }
        }
    }
}