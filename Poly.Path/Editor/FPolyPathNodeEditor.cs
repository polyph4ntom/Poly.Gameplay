using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poly.Path.Editor
{
    [CustomEditor(typeof(APolyPathNode))]
    public class FPolyPathNodeEditor : UnityEditor.Editor
    {
        private static bool showHandles = true; 
        
        private bool isDragging = false;
        private Vector3 dragDirection;
        private APolyPathNode dragOriginNode;
        private Vector3 dragStartPos;
        private HashSet<Vector3> operatedPositions = new();

        private const float gridSize = 0.01f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Tool", EditorStyles.boldLabel);
            showHandles = EditorGUILayout.Toggle("Enable Node Spawning", showHandles);
        }
        
        public void OnSceneGUI()
        {
            if (!showHandles) return;
            
            APolyPathNode node = (APolyPathNode)target;
            Transform t = node.transform;
            BoxCollider box = t.GetComponent<BoxCollider>();
            if (box == null) return;

            Vector3 nodeCenter = box.bounds.center;
            Vector3 topOffset = Vector3.up * box.bounds.extents.y;

            float handleDistance = Mathf.Max(box.bounds.extents.x, box.bounds.extents.z) + 0.05f;

            DrawHandle(node, Vector3.forward, handleDistance, topOffset);
            DrawHandle(node, Vector3.back, handleDistance, topOffset);
            DrawHandle(node, Vector3.left, handleDistance, topOffset);
            DrawHandle(node, Vector3.right, handleDistance, topOffset);

            HandleDragging();
        }

        private void DrawHandle(APolyPathNode node, Vector3 direction, float distance, Vector3 topOffset)
        {
            Transform t = node.transform;
            BoxCollider box = node.GetComponent<BoxCollider>();
            Vector3 center = box.bounds.center + direction * distance + topOffset;

            float handleSize = 0.15f;

            // Draw white handle
            Handles.color = Color.white;
            Handles.SphereHandleCap(0, center, Quaternion.identity, handleSize, EventType.Repaint);

            // Draw cyan arrow
            Handles.color = Color.cyan;
            Handles.ArrowHandleCap(0, center, Quaternion.LookRotation(direction), handleSize * 1.5f, EventType.Repaint);

            // Handle input
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                float dist = HandleUtility.DistanceToCircle(center, handleSize);
                if (dist < 10f)
                {
                    if (e.button == 0)
                    {
                        StartDrag(node, direction);
                        e.Use();
                    }
                    else if (e.button == 1)
                    {
                        Vector3 targetPos = GetSpawnPosition(node, direction);
                        TryDeleteNodeAt(targetPos);
                        e.Use();
                    }
                }
            }
        }

        private void StartDrag(APolyPathNode originNode, Vector3 direction)
        {
            isDragging = true;
            dragDirection = GetLockedDirection(direction.normalized);
            dragOriginNode = originNode;
            operatedPositions.Clear();

            dragStartPos = originNode.transform.position;

            Vector3 spawnPos = GetSpawnPosition(originNode, dragDirection);
            OperateOnNodeAt(originNode, spawnPos);
            operatedPositions.Add(spawnPos);
        }

        private void HandleDragging()
        {
            if (!isDragging) return;

            Event e = Event.current;
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                float tileSize = GetTileSize(dragOriginNode);
                Vector3 mouseWorld = GetMouseWorldPositionOnPlane(GetSpawnY(dragOriginNode));
                Vector3 offset = mouseWorld - dragStartPos;

                float projected = Vector3.Dot(offset, dragDirection);
                int tileCount = Mathf.FloorToInt(projected / tileSize);

                for (int i = 1; i <= tileCount; i++)
                {
                    Vector3 pos = dragStartPos + dragDirection * tileSize * i;
                    pos = GetSpawnPosition(dragOriginNode, dragDirection, i);

                    if (!operatedPositions.Contains(pos))
                    {
                        OperateOnNodeAt(dragOriginNode, pos);
                        operatedPositions.Add(pos);
                    }
                }

                SceneView.RepaintAll();
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
                dragOriginNode = null;
                e.Use();
            }
        }

        private void OperateOnNodeAt(APolyPathNode originNode, Vector3 position)
        {
            if (Event.current.shift)
            {
                TryDeleteNodeAt(position);
            }
            else
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(originNode.gameObject);
                if (prefab == null) return;

                GameObject newNode = (GameObject)PrefabUtility.InstantiatePrefab(prefab, originNode.transform.parent);
                newNode.transform.position = position;
                newNode.transform.rotation = originNode.transform.rotation;

                var nodeComp = newNode.GetComponent<APolyPathNode>();
                nodeComp.pathType = APolyPathNode.PathType.Straight;

                Undo.RegisterCreatedObjectUndo(newNode, "Spawn Path Node");
            }
        }

        private void TryDeleteNodeAt(Vector3 position)
        {
            Collider[] hits = Physics.OverlapSphere(position, 0.1f);
            foreach (var hit in hits)
            {
                var node = hit.GetComponent<APolyPathNode>();
                if (node != null)
                {
                    Undo.DestroyObjectImmediate(node.gameObject);
                    break;
                }
            }
        }

        private Vector3 GetMouseWorldPositionOnPlane(float yHeight)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0, yHeight, 0));
            plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        private Vector3 GetLockedDirection(Vector3 raw)
        {
            return Mathf.Abs(raw.x) > Mathf.Abs(raw.z)
                ? (raw.x > 0 ? Vector3.right : Vector3.left)
                : (raw.z > 0 ? Vector3.forward : Vector3.back);
        }

        private float GetSpawnY(APolyPathNode node)
        {
            BoxCollider box = node.GetComponent<BoxCollider>();
            return node.transform.position.y;
        }

        private float GetTileSize(APolyPathNode node)
        {
            BoxCollider box = node.GetComponent<BoxCollider>();
            return Mathf.Max(
                box.size.x * node.transform.lossyScale.x,
                box.size.z * node.transform.lossyScale.z
            );
        }

        private Vector3 GetSpawnPosition(APolyPathNode node, Vector3 direction, int step = 1)
        {
            BoxCollider box = node.GetComponent<BoxCollider>();
            Transform t = node.transform;

            Vector3 scale = t.lossyScale;
            Vector3 fullSize = Vector3.Scale(box.size, scale);

            Vector3 offset = Vector3.zero;
            if (direction == Vector3.forward)
                offset = new Vector3(0, 0, fullSize.z * step);
            else if (direction == Vector3.back)
                offset = new Vector3(0, 0, -fullSize.z * step);
            else if (direction == Vector3.right)
                offset = new Vector3(fullSize.x * step, 0, 0);
            else if (direction == Vector3.left)
                offset = new Vector3(-fullSize.x * step, 0, 0);

            return SnapToGrid(t.position + offset);
        }

        private Vector3 SnapToGrid(Vector3 pos)
        {
            return new Vector3(
                Mathf.Round(pos.x / gridSize) * gridSize,
                Mathf.Round(pos.y / gridSize) * gridSize,
                Mathf.Round(pos.z / gridSize) * gridSize
            );
        }
    }
}
