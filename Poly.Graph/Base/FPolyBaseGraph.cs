using System;
using System.Collections.Generic;
using System.Linq;
using Poly.Common;
using Poly.Log;

namespace Poly.Graph
{
    public abstract class FPolyBaseGraph
    {
        protected readonly Dictionary<Guid, FPolyBaseNode> nodesOnGrid = new();

        public virtual void RegisterNode(Guid nodeId, FPolyBaseNode node)
        {
            if (!nodesOnGrid.TryAdd(nodeId, node))
            {
                FPolyLog.Error("Poly.Graph", "You are trying to register the same node twice");
            }
        }

        public virtual void UnregisterNode(Guid nodeId)
        {
            if (!nodesOnGrid.Remove(nodeId))
            {
                FPolyLog.Error("Poly.Graph",  "You are trying to unregister the same node twice");
            }
        }

        public List<Guid> EnumerateNodeIds()
        {
            return nodesOnGrid.Keys.ToList();
        }

        public void ConnectNodes(int portIdx, Guid fromNodeId,  Guid toNodeId)
        {
            if (!nodesOnGrid.TryGetValue(fromNodeId, out var fromNode))
            {
                FPolyLog.Error("Poly.Graph", "Nodes connection cannot be determined as node you want to connect from is not registered on graph");
                return;
            }
            
            if (!nodesOnGrid.TryGetValue(toNodeId, out var toNode))
            {
                FPolyLog.Error("Poly.Graph", "Nodes connection cannot be determined as node you want to connect to is not registered on graph");
                return;
            }

            toNode.ConnectNode(portIdx, fromNode);
        }

        public void ClearConnection(int portIdx, Guid toNodeId)
        {
            if (!nodesOnGrid.TryGetValue(toNodeId, out var toNode))
            {
                FPolyLog.Error("Poly.Graph", "Nodes connection cannot be determined as node you want to connect to is not registered on graph");
                return;
            }

            toNode.DisconnectNode(portIdx);
        }

        public abstract EPolyNodeEvalResult EvaluateGraph();
    }
}
