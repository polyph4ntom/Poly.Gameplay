using UnityEngine.UIElements;

namespace Poly.Graph
{
	public abstract class VPolyNodeTwoToOne<TInType1, TInType2, TOutType> : VPolyNodeBase
	{
		private readonly VPolyNodePort inPort1;
		private readonly VPolyNodePort inPort2;

		private readonly VPolyNodePort outPort;

		protected VPolyNodeTwoToOne(bool isMultiConnectionAllowed)
		{
			inputArea.AddToClassList("poly-node__inputArea-space");
			inPort1 = new VPolyNodePort(0, Id, PolyPortType.Input, typeof(TInType1), false)
			{
				name = "inputPort1",
			};
		
			inPort2 = new VPolyNodePort(1, Id, PolyPortType.Input, typeof(TInType2), false)
			{
				name = "inputPort2",
			};
			
			outPort = new VPolyNodePort(0, Id, PolyPortType.Output, typeof(TOutType), isMultiConnectionAllowed)
			{
				name = "outputPort1",
			};
			
			inPort1.OnConnectionApplied.Add(this, OnConnectionApplied);
			inPort1.OnConnectionRemoved.Add(this, OnConnectionRemoved);
			
			inPort2.OnConnectionApplied.Add(this, OnConnectionApplied);
			inPort2.OnConnectionRemoved.Add(this, OnConnectionRemoved);
			
			outPort.OnConnectionApplied.Add(this, OnConnectionApplied);
			outPort.OnConnectionRemoved.Add(this, OnConnectionRemoved);
			
			inputArea.Add(inPort1);
			inputArea.Add(inPort2);
			outputArea.Add(outPort);
		}
		
		internal override void OnNodeRemovedFromGrid()
		{
			inPort1.DisconnectAll();
			inPort2.DisconnectAll();
			outPort.DisconnectAll();
		}

		internal override void OnNodeRegisteredToGrid(VisualElement gridContentElement)
		{
			inPort1.AddManipulator(new MPolyPortConnection(inPort1, gridContentElement));
			inPort1.AddManipulator(new MPolyPortDropConnection(inPort1));
			
			inPort2.AddManipulator(new MPolyPortConnection(inPort2, gridContentElement));
			inPort2.AddManipulator(new MPolyPortDropConnection(inPort2));
			
			outPort.AddManipulator(new MPolyPortConnection(outPort, gridContentElement));
			outPort.AddManipulator(new MPolyPortDropConnection(outPort));
		}
		
		public override void HideEdges()
		{
			foreach (var c in inPort1.connections)
			{
				c.style.display = DisplayStyle.None;
			}
			
			foreach (var c in inPort2.connections)
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
			foreach (var c in inPort1.connections)
			{
				c.style.display = DisplayStyle.Flex;
			}
			
			foreach (var c in inPort2.connections)
			{
				c.style.display = DisplayStyle.Flex;
			}
			
			foreach (var c in outPort.connections)
			{
				c.style.display = DisplayStyle.Flex;
			}
		}

		internal override void OnPositionUpdated()
		{
			inPort1.RedrawConnections();
			inPort2.RedrawConnections();
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