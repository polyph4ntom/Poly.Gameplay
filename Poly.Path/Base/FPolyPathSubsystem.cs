using Poly.GameFramework.Core;
using Poly.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine;

namespace Poly.Path
{
    public class FArcPathRequest
    {
        public APolyPath path;
        public GameObject objectToMove;
        public Action onComplete;
    }
    
    public class MovingObjectState
    {
        public GameObject objectToMove;
        public APolyPath path;
        public int currentWaypointIndex;
        public float lerpProgress;
        public bool isPaused;
        public Action onComplete;
    }
    
    public class FPolyPathSubsystem : FPolyWorldSubsystem
    {
        private FPolyPathSubsystemSettings settings;
        private bool isForceLocked = false;

        private readonly Dictionary<Guid, Queue<FArcPathRequest>> pathMovementRequests = new();
        private readonly Dictionary<Guid, List<MovingObjectState>> activeMovementsPerPath = new();
        private readonly Dictionary<Guid, bool> pathBlockedAtEnd = new();
        private readonly Dictionary<Guid, float> lastSpawnTimePerPath = new();
        
        protected override Task InitializeSubsystem()
        {
            settings = FPolyDevSettingsDatabase.Get<FPolyPathSubsystemSettings>();
            return base.InitializeSubsystem();
        }

        protected override void Tick(float deltaTime)
        {
            if (isForceLocked)
            {
                return;
            }

            foreach (var kvp in activeMovementsPerPath)
            {
                var pathId = kvp.Key;
                var activeList = kvp.Value;

                for (var i = activeList.Count - 1; i >= 0; i--)
                {
                    var obj = activeList[i];
                    if (obj.isPaused)
                    {
                        continue;
                    }
                    
                    var path = obj.path.GetRuntimePath().ToArray();
                    
                    var start = path[obj.currentWaypointIndex];
                    var end = path[obj.currentWaypointIndex + 1];
                        
                    var segmentDistance = Vector3.Distance(start, end);
                    obj.lerpProgress += Time.deltaTime * settings.MovementSpeed / segmentDistance;
                    obj.objectToMove.transform.position = Vector3.Lerp(start, end, obj.lerpProgress);

                    if (obj.lerpProgress < 1)
                    {
                        continue;
                    }

                    obj.currentWaypointIndex++;
                    obj.lerpProgress = 0f;

                    if (obj.currentWaypointIndex < path.Length - 1)
                    {
                        continue;
                    }

                    if (IsEndBlocked(pathId))
                    {
                        obj.isPaused = true;
                        continue;
                    }
                            
                    activeList[i].objectToMove.SetActive(false);
                    activeList[i]?.onComplete.Invoke();
                    activeList.RemoveAt(i);
                }
            }
            
            foreach (var pathId in pathMovementRequests.Keys)
            {
                TryDequeueNext(pathId);
            }
        }
        
        public void EnqueueRequest(FArcPathRequest request)
        {
            var id = request.path.ID;
            if (!pathMovementRequests.ContainsKey(id))
            {
                pathMovementRequests[id] = new Queue<FArcPathRequest>();
            }
            
            request.objectToMove.gameObject.SetActive(false);
            pathMovementRequests[id].Enqueue(request);
        }
        
        private void TryDequeueNext(Guid pathId)
        {
            if (!pathMovementRequests.TryGetValue(pathId, out var queue) || queue.Count == 0)
            {
                return;
            }

            if (pathBlockedAtEnd.TryGetValue(pathId, out var blocked) && blocked)
            {
                return;
            }

            if (Time.time - lastSpawnTimePerPath.GetValueOrDefault(pathId, 0) < settings.SpawnCooldown)
            {
                return;
            }

            if (!HasEnoughSpacing(pathId, queue.Peek().path))
            {
                return;
            }

            var nextRequest = queue.Dequeue();
            StartMovement(nextRequest);
        }

        private void StartMovement(FArcPathRequest request)
        {
            var pathId = request.path.ID;
            if (!activeMovementsPerPath.ContainsKey(pathId))
            {
                activeMovementsPerPath[pathId] = new List<MovingObjectState>();
            }
            
            var objState = new MovingObjectState
            {
                objectToMove = request.objectToMove,
                path = request.path,
                currentWaypointIndex = 0,
                lerpProgress = 0f,
                isPaused = false,
                onComplete = request.onComplete
            };
            
            activeMovementsPerPath[pathId].Add(objState);
            request.objectToMove.transform.position = request.path.GetRuntimePath()[0];
            lastSpawnTimePerPath[pathId] = Time.time;
            request.objectToMove.gameObject.SetActive(true);
        }
        
        private bool HasEnoughSpacing(Guid pathId, APolyPath path)
        {
            if (!activeMovementsPerPath.TryGetValue(pathId, out var value))
            {
                return true;
            }
            
            var lastObj = value.LastOrDefault();
            if (lastObj == null) return true;
            
            var lastPos = lastObj.objectToMove.transform.position;
            var firstPoint = path.GetRuntimePath()[0];
            
            return Vector3.Distance(lastPos, firstPoint) >= settings.MinDistanceBetween;
        }
        
        private bool IsEndBlocked(Guid pathId)
        {
            return pathBlockedAtEnd.TryGetValue(pathId, out var blocked) && blocked;
        }
        
        public void SetEndBlocked(Guid pathId, bool blocked, GameObject requester = null)
        {
            pathBlockedAtEnd[pathId] = blocked;

            for (var i = activeMovementsPerPath[pathId].Count - 1; i >= 0; i--)
            {
                if (requester != null && activeMovementsPerPath[pathId][i].objectToMove == requester)
                {
                    activeMovementsPerPath[pathId].RemoveAt(i);
                    continue;
                }

                activeMovementsPerPath[pathId][i].isPaused = blocked;
            }

            if (!blocked)
            {
                TryDequeueNext(pathId);
            }
        }

        public void StopAllPaths()
        {
            isForceLocked = true;
        }
        
        public void ClearAllPaths()
        {
            lastSpawnTimePerPath.Clear();
            pathBlockedAtEnd.Clear();

            foreach (var req in pathMovementRequests)
            {
                foreach (var q in req.Value)
                {
                    World.DestroyObject(q.objectToMove.gameObject);
                }
            }
            pathMovementRequests.Clear();

            foreach (var a in activeMovementsPerPath)
            {
                foreach (var q in a.Value)
                {
                    World.DestroyObject(q.objectToMove.gameObject);
                }
            }
            activeMovementsPerPath.Clear();
            isForceLocked = false;
        }
    }
}