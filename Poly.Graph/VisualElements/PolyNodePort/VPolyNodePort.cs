using System;
using System.Collections.Generic;
using Poly.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public enum PolyPortType
	{
		Unknown,
		Input,
		Output,
	}

	public struct FPolyConnectionData
	{
		public Guid nodeId;
		public int portIdx;
	}

	public class VPolyNodePort : VisualElement
	{
		private Guid owningNodeId;
		private int portIdx;

		//TODO: Should be private
		public readonly List<VPolyNodeEdge> connections = new();
 
		private MPolyPortConnection manipulator;

		internal readonly PolyPortType portType;
		internal readonly System.Type compatibleDataType = null;
		private readonly bool isMultiConnectionAllowed;

		internal FPolyAction<FPolyConnectionData, FPolyConnectionData> OnConnectionApplied { get; } = new();
		internal FPolyAction<FPolyConnectionData, FPolyConnectionData> OnConnectionRemoved { get; } = new();
		
		public VPolyNodePort(int portIndex, Guid nodeId, PolyPortType portType, System.Type compatibleDataType, bool multiConnectionAllowed)
		{
			this.portType = portType;
			this.compatibleDataType = compatibleDataType;
			this.owningNodeId = nodeId;
			this.portIdx = portIndex;

			AddToClassList("poly-node-port");
			pickingMode = PickingMode.Position;

			isMultiConnectionAllowed = this.portType == PolyPortType.Output && multiConnectionAllowed;

			style.backgroundColor = new StyleColor(new Color(0,0,0,0f));
			style.unityBackgroundImageTintColor = FPolyGraphAssembly.portColorsMapping[compatibleDataType];
		}

		internal void SetConnection(VPolyNodeEdge connectionEdge, VPolyNodePort otherPort)
		{
			if (!isMultiConnectionAllowed)
			{
				DisconnectAll();
			}

			if (!otherPort.isMultiConnectionAllowed)
			{
				otherPort.DisconnectAll();
			}

			connectionEdge.toPort = this;
			
			ApplyConnection(connectionEdge, false);
			otherPort.ApplyConnection(connectionEdge, true);

			connectionEdge.Refresh();
		}

		internal void DisconnectAll()
		{
			foreach (var connection in connections)
			{
				if (connection.fromPort == this)
				{
					connection.toPort.DropConnectionWith(this);
				}
				else
				{
					connection.fromPort.DropConnectionWith(this);
				}

				connection.RemoveFromHierarchy();
			}
			
			connections.Clear();
		}
		
		private void ApplyConnection(VPolyNodeEdge connectionEdge, bool silent)
		{
			foreach (var connection in connections)
			{
				if (connection.fromPort == connectionEdge.fromPort && connection.toPort == connectionEdge.toPort)
				{
					return;
				}
				
				if (connection.fromPort == connectionEdge.toPort && connection.toPort == connectionEdge.fromPort)
				{
					return;
				}
			}

			var from = new FPolyConnectionData
			{
				nodeId = connectionEdge.fromPort.owningNodeId,
				portIdx = connectionEdge.fromPort.portIdx
			};

			var to = new FPolyConnectionData()
			{
				nodeId = connectionEdge.toPort.owningNodeId,
				portIdx = connectionEdge.toPort.portIdx
			};
			
			connections.Add(connectionEdge);
			if (!silent)
			{
				OnConnectionApplied.Broadcast(from, to);
			}

		}

		private void DropConnectionWith(VPolyNodePort requester)
		{
			foreach (var connection in connections)
			{
				if (connection.fromPort == requester || connection.toPort == requester)
				{
					var from = new FPolyConnectionData
					{
						nodeId = connection.fromPort.owningNodeId,
						portIdx = connection.fromPort.portIdx
					};

					var to = new FPolyConnectionData()
					{
						nodeId = connection.toPort.owningNodeId,
						portIdx = connection.toPort.portIdx
					};
					
					OnConnectionRemoved.Broadcast(from, to);
					connections.Remove(connection);
					break;
				}
			}
		}

		internal bool CanConnectTo(VPolyNodePort otherPort)
		{
			if (portType == PolyPortType.Unknown || otherPort.portType == PolyPortType.Unknown)
			{
				return false;
			}

			if (portType == otherPort.portType)
			{
				return false;
			}

			if (compatibleDataType != otherPort.compatibleDataType)
			{
				return false;
			}

			return true;
		}

		internal void RedrawConnections()
		{
			foreach (var connection in connections)
			{
				connection.Refresh();
			}
		}

		private void ClearConnection()
		{
			// Edge?.RemoveFromHierarchy();
			// Edge = null;
			// connectedPort = null;
		}
	}
}