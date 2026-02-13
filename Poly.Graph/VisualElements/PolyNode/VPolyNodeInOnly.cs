using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
    public abstract class VPolyNodeInOnly<TInType> : VPolyNodeBase
    {
        private readonly VPolyNodePort inPort;

        protected VPolyNodeInOnly()
        {
            inputArea.AddToClassList("poly-node__inputArea-centered");
            inPort = new VPolyNodePort(0, Id, PolyPortType.Input, typeof(TInType), false)
            {
                name = "inPort",
            };
            
            inPort.OnConnectionApplied.Add(this, OnConnectionApplied);
            inPort.OnConnectionRemoved.Add(this, OnConnectionRemoved);

            inputArea.Add(inPort);
        }
        
        public override void HideEdges()
        {
            foreach (var c in inPort.connections)
            {
                c.style.display = DisplayStyle.None;
            }
        }

        public override void ShowEdges()
        {
            foreach (var c in inPort.connections)
            {
                c.style.display = DisplayStyle.Flex;
            }
        }
        
        internal override void OnNodeRemovedFromGrid()
        {
            inPort.DisconnectAll();
        }

        internal override void OnNodeRegisteredToGrid(VisualElement gridContentElement)
        {
            inPort.AddManipulator(new MPolyPortConnection(inPort, gridContentElement));
            inPort.AddManipulator(new MPolyPortDropConnection(inPort));
        }

        internal override void OnPositionUpdated()
        {
            inPort.RedrawConnections();
        }
        
        internal override void OnConnectionApplied(FPolyConnectionData from, FPolyConnectionData to)
        {
            var status = new FPolyNewConnectionStatus
            {
                isConnectionRequest = true,
                from = from,
                to = to
            };
            OnConnectionStatusUpdateRequest.Broadcast(status);
        }

        internal override void OnConnectionRemoved(FPolyConnectionData from, FPolyConnectionData to)
        {
            var status = new FPolyNewConnectionStatus
            {
                isConnectionRequest = false,
                from = from,
                to = to
            };
            OnConnectionStatusUpdateRequest.Broadcast(status);
        }
    }
}
