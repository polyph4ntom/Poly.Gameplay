using UnityEngine.UIElements;

namespace Poly.Graph
{
	public abstract class VPolyNodeOneToOne<TInType, TOutType> : VPolyNodeBase
	{
		private readonly VPolyNodePort inPort;
		private readonly VPolyNodePort outPort;

		protected VPolyNodeOneToOne(bool isMultiConnectionAllowed)
		{
			inputArea.AddToClassList("poly-node__inputArea-centered");
			inPort = new VPolyNodePort(0, Id, PolyPortType.Input, typeof(TInType), false)
			{
				name = "inPort",
			};
			
			outPort = new VPolyNodePort(0, Id, PolyPortType.Output, typeof(TOutType), isMultiConnectionAllowed)
			{
				name = "outPort",
			};
			
			inPort.OnConnectionApplied.Add(this, OnConnectionApplied);
			inPort.OnConnectionRemoved.Add(this, OnConnectionRemoved);
			
			outPort.OnConnectionApplied.Add(this, OnConnectionApplied);
			outPort.OnConnectionRemoved.Add(this, OnConnectionRemoved);
			
			inputArea.Add(inPort);
			outputArea.Add(outPort);
		}

		public override void HideEdges()
		{
			foreach (var c in inPort.connections)
			{
				c.style.display = DisplayStyle.None;
			}
			
			foreach (var c in outPort.connections)
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
			
			foreach (var c in outPort.connections)
			{
				c.style.display = DisplayStyle.Flex;
			}
		}

		internal override void OnNodeRemovedFromGrid()
		{
			inPort.DisconnectAll();
			outPort.DisconnectAll();
		}

		internal override void OnNodeRegisteredToGrid(VisualElement gridContentElement)
		{
			inPort.AddManipulator(new MPolyPortConnection(inPort, gridContentElement));
			inPort.AddManipulator(new MPolyPortDropConnection(inPort));
			
			outPort.AddManipulator(new MPolyPortConnection(outPort, gridContentElement));
			outPort.AddManipulator(new MPolyPortDropConnection(outPort));
		}

		internal override void OnPositionUpdated()
		{
			inPort.RedrawConnections();
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