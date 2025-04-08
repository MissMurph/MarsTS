using System;
using System.Collections;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Pathfinding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units
{
    public class UnitPathfinder : MonoBehaviour, IEntityComponent<UnitPathfinder>, IEntityUpdate
    {
        public UnitPathfinder Get() => this;
        public string Key => "pathing";
        public Path CurrentPath { get; set; } = Path.Empty;
        public int PathIndex;
        
        private const float PathUpdateMoveThreshold = .5f;
        private const float SqrMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;

        [SerializeField]
        private float _waypointCompletionDistance;
        private EventAgent _eventAgent;
        private UnitTargetManager _targetManager;
        private Vector3 _targetOldPos;
        
        private void Awake() {
            _eventAgent = GetComponent<EventAgent>();
            _targetManager = GetComponent<UnitTargetManager>();
        }

        public void UpdateServer() {
            CheckWaypointDistance();
            CheckForTrackedTransformPathUpdate();
        }

        public void UpdateClient() {
            if (NetworkManager.Singleton.IsServer) return;
            CheckWaypointDistance();
            CheckForTrackedTransformPathUpdate();
        }

        private void CheckWaypointDistance()
        {
            if (CurrentPath.IsEmpty) return;
            
            Vector3 targetWaypoint = CurrentPath[PathIndex];

            float distance = new Vector3(targetWaypoint.x - transform.position.x, 0,
                targetWaypoint.z - transform.position.z).magnitude;

            if (distance <= _waypointCompletionDistance) PathIndex++;

            if (PathIndex < CurrentPath.Length) return;
            
            _eventAgent.Local(new PathCompleteEvent(_eventAgent, true));
            CurrentPath = Path.Empty;
        }

        private void CheckForTrackedTransformPathUpdate() {
            if (_targetManager.TargetTransform is null) return;
            
            Vector3 currentTargetPosition = _targetManager.TargetTransform.position;

            if (!((currentTargetPosition - _targetOldPos).sqrMagnitude > SqrMoveThreshold)) 
                return;
            
            PathRequestManager.RequestPath(transform.position, currentTargetPosition, OnPathFound);
            _targetOldPos = currentTargetPosition;
        }

        private void OnPathFound(Path newPath, bool pathSuccessful) {
            if (!pathSuccessful) return;
            
            CurrentPath = newPath;
            PathIndex = 0;
        }

        // Uncomment below for debugging
        /*public void OnDrawGizmos() {
            if (CurrentPath.IsEmpty) return;
            
            for (int i = PathIndex; i < CurrentPath.Length; i++) {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(CurrentPath[i], Vector3.one / 2);

                if (i == PathIndex)
                    Gizmos.DrawLine(transform.position, CurrentPath[i]);
                else
                    Gizmos.DrawLine(CurrentPath[i - 1], CurrentPath[i]);
            }
        }*/
    }
}