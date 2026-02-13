using System;
using System.Collections.Generic;
using Poly.GameFramework.Core;
using UnityEngine;

namespace Poly.Path
{
	[ExecuteInEditMode]
	public class APolyPath : APolyMonoBehaviour
	{
        [SerializeField]
        private List<Vector3> finalPathPoints = new();

        private Guid guid;
        public Guid ID => guid;

        protected override void InitializeComponent(APolyWorldBase inWorld)
        {
            base.InitializeComponent(inWorld);
            guid = Guid.NewGuid();
        }

        public IReadOnlyList<Vector3> GetRuntimePath()
        {
            return finalPathPoints;
        }

#if UNITY_EDITOR
        [SerializeField]
        private bool autoCollectNodes = true;

        [SerializeField]
        private List<APolyPathNode> pathNodes = new();
#endif

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (autoCollectNodes)
            {
                CollectNodes();
            }

            BuildSplinePath();
#endif
        }

#if UNITY_EDITOR
        private void CollectNodes()
        {
            pathNodes.Clear();
            pathNodes.AddRange(GetComponentsInChildren<APolyPathNode>());
        }
        
        [ContextMenu("Build Spline Path")]
        private void BuildSplinePath()
        {
            finalPathPoints.Clear();

            if (pathNodes.Count == 0)
                return;

            for (var i = 0; i < pathNodes.Count; i++)
            {
                var node = pathNodes[i];
                node.RefreshPathPoints();
                
                // If first node, add entry
                if (i == 0)
                {
                    finalPathPoints.Add(node.Entry);
                }

                
                if (node.pathType != APolyPathNode.PathType.Straight)
                {
                    // Bezier interpolation (approximated as linear segments for simplicity)
                    var segments = 20;
                    for (var j = 1; j <= segments; j++)
                    {
                        var t = j / (float)segments;
                        var point = CalculateBezierPoint(t, node.Entry, node.EntryTangent, node.ExitTangent, node.Exit);
                        finalPathPoints.Add(point);
                    }
                }
                else
                {
                    // Straight line to next
                    finalPathPoints.Add(node.Exit);
                }
            }
        }
        
        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Quadratic Bezier: B(t) = (1 - t)^2 * p0 + 2(1 - t)t * p1 + t^2 * p2
            var u = 1 - t;
            return u * u * u * p0
                   + 3 * u * u * t * p1
                   + 3 * u * t * t * p2
                   + t * t * t * p3;
        }

        private void OnDrawGizmos()
        {
            if (finalPathPoints.Count < 2)
                return;

            Gizmos.color = Color.cyan;
            for (var i = 0; i < finalPathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(finalPathPoints[i], finalPathPoints[i + 1]);
            }
        }
#endif
    }
}