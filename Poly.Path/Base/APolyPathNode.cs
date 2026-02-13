using System.ComponentModel;
using Poly.GameFramework.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Poly.Path
{
    [ExecuteAlways]
    public class APolyPathNode : APolyMonoBehaviour
    {
        public enum PathType
        {
            Straight,
            LeftTurn,
            RightTurn,
        }

        public PathType pathType = PathType.Straight;
        
        public Vector3 Entry { get; private set; }
        public Vector3 Exit { get; private set; }
        public Vector3 EntryTangent { get; private set; }
        public Vector3 ExitTangent { get; private set; }

#if UNITY_EDITOR      
        [SerializeField] private Transform entryT;
        [SerializeField] private Transform exitT;
        [SerializeField] private float tangentLength = 1.0f;

        public void RefreshPathPoints()
        {
            Entry = entryT != null ? entryT.position : transform.position;
            Exit = exitT != null ? exitT.position : transform.position;

            if (entryT != null)
            {
                var dir = entryT.forward * tangentLength;
                EntryTangent = pathType == PathType.LeftTurn ? Entry - dir : Entry + dir;
            }
            else
                EntryTangent = Entry;

            if (exitT != null)
            {
                var dir = exitT.forward * tangentLength;
                ExitTangent = pathType == PathType.LeftTurn ? Exit + dir : Exit - dir;
            }
            else
                ExitTangent = Exit;
        }

        private void OnDrawGizmos()
        {
            if (entryT == null || exitT == null)
                return;

            RefreshPathPoints();

            if (pathType == PathType.LeftTurn || pathType == PathType.RightTurn)
            {
                Handles.color = Color.red;
                Handles.DrawBezier(Entry, Exit, EntryTangent, ExitTangent, Handles.color, null, 2f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Entry, Exit);
                Gizmos.color = Color.white;
            }

            // Draw Entry/Exit points
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Entry, 0.05f);
            Handles.Label(Entry + Vector3.up * 0.1f, "ENTRY");

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Exit, 0.05f);
            Handles.Label(Exit + Vector3.up * 0.1f, "EXIT");

            // Optional: Draw tangent lines
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Entry, EntryTangent);
            Gizmos.DrawLine(Exit, ExitTangent);
        }
#endif
    }
}
