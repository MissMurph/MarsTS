using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Path {
		public Vector3[] Waypoints { get; private set; }

		public int Length {
			get {
				return Waypoints.Length;
			}
		}

		public bool IsEmpty {
			get {
				return Waypoints.Length == 0;
			}
		}

		public Path (params Vector3[] waypoints) {
			Waypoints = waypoints;
		}

		public Vector3 this[int index] {
			get {
				return Waypoints[index];
			}
			set {
				Waypoints[index] = value;
			}
		}

		public static Path Empty () {
			return new Path() {
				Waypoints = new Vector3[0]
			};
		}
	}
}