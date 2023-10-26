using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MarsTS.World.Pathfinding {

	public class PathRequestManager : MonoBehaviour {

		private static PathRequestManager instance;

		private Queue<PathResult> results = new Queue<PathResult>();

		private Pathfinder localFinder;

		private void Awake () {
			instance = this;
			localFinder = GetComponent<Pathfinder>();
		}

		private void Update () {
			if (results.Count > 0) {
				int itemsInQueue = results.Count;

				lock (results) {
					for (int i = 0; i < itemsInQueue; i++) {
						PathResult result = results.Dequeue();
						result.callback(result.path, result.success);
					}
				}
			}
		}

		public static void RequestPath (Vector3 origin, Vector3 target, Action<Path, bool> callback) {
			ThreadStart threadStart = delegate {
				instance.localFinder.FindPath(new PathRequest(origin, target, callback), instance.FinishedProcessingPath);
			};

			threadStart.Invoke();
		}

		public void FinishedProcessingPath (PathResult result) {
			lock (results) {
				results.Enqueue(result);
			}
		}

		private void OnDestroy () {
			instance = null;
		}
	}

	public struct PathResult {
		public Path path;
		public bool success;
		public Action<Path, bool> callback;

		public PathResult (Vector3[] _path, bool _success, Action<Path, bool> _callback) {
			path = new Path(_path);
			success = _success;
			callback = _callback;
		}
	}

	public struct PathRequest {
		public Vector3 pathStart;
		public Vector3 pathEnd;

		public Action<Path, bool> callback;

		public PathRequest (Vector3 _origin, Vector3 _target, Action<Path, bool> _callback) {
			pathStart = _origin;
			pathEnd = _target;
			callback = _callback;
		}
	}
}