using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
    public abstract class VPolyNodeOutOnly<TOutType> : VPolyNodeBase
    {
        private readonly VPolyNodePort outPort;

        protected VPolyNodeOutOnly(bool isMultiConnectionAllowed)
        {
            outPort = new VPolyNodePort(0, Id, PolyPortType.Output, typeof(TOutType), isMultiConnectionAllowed)
            {
                name = "inPort",
            };
            
            outPort.OnConnectionApplied.Add(this, OnConnectionApplied);
            outPort.OnConnectionRemoved.Add(this, OnConnectionRemoved);

            outputArea.Add(outPort);
        }
        
        public override void HideEdges()
        {
            foreach (var c in outPort.connections)
            {
                c.style.display = DisplayStyle.None;
            }
        }

        public override void ShowEdges()
        {
            foreach (var c in outPort.connections)
            {
                c.style.display = DisplayStyle.Flex;
            }
        }
        
        internal override void OnNodeRemovedFromGrid()
        {
            outPort.DisconnectAll();
        }

        internal override void OnNodeRegisteredToGrid(VisualElement gridContentElement)
        {
            outPort.AddManipulator(new MPolyPortConnection(outPort, gridContentElement));
            outPort.AddManipulator(new MPolyPortDropConnection(outPort));
        }

        internal override void OnPositionUpdated()
        {
            outPort.RedrawConnections();
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
