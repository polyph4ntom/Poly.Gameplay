using System;
using System.Collections.Generic;
using Poly.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Poly.Events;
using Poly.Log;

namespace Poly.Graph
{
    public class VPolyGrid : VisualElement
    {
        public FPolyEventBus GraphBus => parentGraph.GraphEventBus;
        
        private readonly VPolyGraph parentGraph;
        private readonly Dictionary<Guid, VPolyNodeBase> nodesRegisteredOnGrid = new();
        private VPolyNodeBase selectedNode;
        
        public FPolyAction OnGridClicked { get; } = new();

        internal VisualElement GridContent { get; }

        public FPolyAction<Guid, OPolyNode> OnNodeRegistered { get; } = new();
        public FPolyAction<Guid> OnNodeRemoved { get; } = new();
        public FPolyAction<OPolyNode> OnNodeInfoRequested { get; } = new();

        public FPolyAction<FPolyNewConnectionStatus> OnNodesConnectionStatusChanged { get; } = new();
        
        private float zoom = 1f;
        public float Zoom
        {
            get => zoom;
            internal set
            {
                zoom = value;
                MarkDirtyRepaint();
            }
        }

        private Vector2 scrollOffset = Vector2.zero;
        public Vector2 ScrollOffset
        {
            get => scrollOffset;
            internal set
            {
                scrollOffset = value;
                MarkDirtyRepaint();
            }
        }
        
        public VPolyGrid(VPolyGraph parentGraph)
        {
            this.parentGraph = parentGraph;
            
            AddToClassList("poly-grid");
            pickingMode = PickingMode.Position;

            GridContent = new VisualElement
            {
                name = "content"
            };
            GridContent.AddToClassList("poly-grid__content");
            GridContent.pickingMode = PickingMode.Ignore;
            Add(GridContent);
            
            generateVisualContent += DrawGrid;
#if UNITY_IOS || UNITY_ANDROID
            //TODO: Add Mobile Manipulators
#else
            this.AddManipulator(new MPolyGridPanDesktop(this));
            this.AddManipulator(new MPolyGridZoomDesktop(this, parentGraph.ZoomMin, parentGraph.ZoomMax, parentGraph.ZoomStep));
#endif
            
            RegisterCallback<PointerDownEvent>(_ =>
            {
                ClearSelection();
                OnGridClicked.Broadcast();
            }, TrickleDown.TrickleDown);
        }

        public void AddNodeToGridRegistry(VPolyNodeBase node, OPolyNode nodeData)
        {
            if (nodesRegisteredOnGrid.ContainsKey(node.Id))
            {
                FPolyLog.Error("Poly.Graph", $"You are trying to register the same node twice: {node}");
                return;
            }

            GridContent.Add(node);
            nodesRegisteredOnGrid.Add(node.Id, node);
            SelectNode(node.Id);

            node.RegisterDragManipulator();
            node.OnNodeRegisteredToGrid(GridContent);
            node.OnNodeClicked.AddUnique(this, SelectNode);
            node.OnNodeDeletionRequest.AddUnique(this, RemoveNodeFromGridRegistry);
            node.OnNodeInfoRequest.AddUnique(this, DisplayNodeTutorial);
            node.OnConnectionStatusUpdateRequest.AddUnique(this, OnNodesConnectionStatusChanged.Broadcast);
            
            OnNodeRegistered.Broadcast(node.Id, nodeData);
        }

        internal int GetCountInRegistry(System.Type type)
        {
            var toReturn = 0;
            foreach (var node in nodesRegisteredOnGrid.Values)
            {
                if (node.GetType() == type)
                {
                    ++toReturn;
                }
            }

            return toReturn;
        }

        private void DisplayNodeTutorial(OPolyNode data)
        {
            OnNodeInfoRequested.Broadcast(data);
        }

        private void RemoveNodeFromGridRegistry(Guid id)
        {
            if (!nodesRegisteredOnGrid.Remove(id, out var node))
            {
                FPolyLog.Error("Poly.Graph", $"You are trying to unregister node which is not present in the registry: {id}");
                return;
            }

            node.UnregisterDragManipulator();
            node.OnNodeClicked.RemoveAll();
            node.OnNodeDeletionRequest.RemoveAll();
            node.OnConnectionStatusUpdateRequest.RemoveAll();
            
            node.OnNodeRemovedFromGrid();
            
            OnNodeRemoved.Broadcast(node.Id);
            
            node.RemoveFromHierarchy();
            selectedNode = null;
        }

        //TODO: Should be private
        public void ClearSelection()
        {
            selectedNode?.ClearSelection();
            selectedNode = null;
        }

        private void SelectNode(Guid id)
        {
            if (!nodesRegisteredOnGrid.TryGetValue(id, out var node))
            {
                FPolyLog.Error("Poly.Graph", $"Node you are trying to select is not registered on grid: {id}");
                return;
            }

            selectedNode?.ClearSelection();
            selectedNode = node;
            selectedNode?.MarkAsSelected();
        }

        public void HideNodes()
        {
            foreach (var node in nodesRegisteredOnGrid.Values)
            {
                node.style.display = DisplayStyle.None;
                node.HideEdges();
            }
        }

        public void DisplayNodes(List<Guid> ids)
        {
            foreach (var node in nodesRegisteredOnGrid.Values)
            {
                if (ids.Contains(node.Id))
                {
                    node.style.display = DisplayStyle.Flex;
                    node.ShowEdges();
                }
            }
        }

        private void DrawGrid(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            DrawGridInternal(painter, parentGraph.InnerGridSpacing, 1, parentGraph.InnerGridColor);
            DrawGridInternal(painter, parentGraph.OuterGridSpacing, 2, parentGraph.OuterGridColor);
        }

        private void DrawGridInternal(Painter2D painter, float spacing, float lineWidth, Color color)
        {
            var width = contentRect.width;
            var height = contentRect.height;

            var scaledSpacing = spacing * Zoom;

            if (scaledSpacing < 10f)
            {
                return;
            }

            painter.strokeColor = color;
            painter.lineWidth = lineWidth;
            
            var xOffset = -ScrollOffset.x * Zoom % scaledSpacing;
            var yOffset = -ScrollOffset.y * Zoom % scaledSpacing;
            
            //Vertical lines
            for (var x = xOffset; x < width; x += scaledSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0));
                painter.LineTo(new Vector2(x, height));
                painter.Stroke();
            }

            // Horizontal lines
            for (var y = yOffset; y < height; y += scaledSpacing)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, y));
                painter.LineTo(new Vector2(width, y));
                painter.Stroke();
            }
        }
    }
}