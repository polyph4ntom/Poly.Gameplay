using System;
using Arc.Main;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyGraphNodeSpawnerDesktop : PointerManipulator
	{
		private readonly VPolyGrid viewport;
		public VisualElement topDrawer;
		public VisualElement bottomDrawer;
		
		private int pointerId;
		private bool isActive;

		private VPolyNodeBase draggedNode;
		private OPolyNode draggedNodeData;
		
		public MPolyGraphNodeSpawnerDesktop(VPolyGrid graphViewport)
		{
			this.viewport = graphViewport;
			
			pointerId = -1;
			isActive = false;
			activators.Add(new ManipulatorActivationFilter{ button = MouseButton.LeftMouse });
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			target.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		}

		public void OnNodeSpawnRequest(FPolyNodeSpawnRequestEvent request)
		{
			if (isActive)
			{
				request.EventHandler.StopImmediatePropagation();
				return;
			}
			
			if (!CanStartManipulation(request.EventHandler))
			{
				return;
			}

			if (request.Node.CountPerGraph > 0 && viewport.GetCountInRegistry(request.Node.NodeVisualClass.Type) >= request.Node.CountPerGraph)
			{
				return;
			}
			
			var newNode = (VPolyNodeBase)Activator.CreateInstance(request.Node.NodeVisualClass.Type);
			if (newNode == null)
			{
				target.ReleasePointer(request.EventHandler.pointerId);
				return;
			}

			newNode.NodeData = request.Node;
			newNode.visible = false;
			newNode.SetTitle(request.Node.NodeName);
			newNode.SetIsDeletable(request.Node.IsDeletable);
			newNode.NodeName = request.Node.NodeName;
			newNode.NodeIcon = request.Node.Icon;
			newNode.NodeDescription = request.Node.NodeDesc;
			newNode.NodeTexture = request.Node.NodeTexture;
			newNode.TintColor = request.Node.ColorTint;

			if (newNode is VArcCheckerNode checkerNode)
			{
				checkerNode.CheckerIcon = request.Node.Icon;
				checkerNode.CheckerText = request.Node.NodeInsideName;
				checkerNode.IconColor = request.Node.NodeIconColor;
			}

			target.Add(newNode);

			draggedNode = newNode;
			draggedNodeData =  request.Node;
			pointerId = request.EventHandler.pointerId;
			
			
			isActive = true;
			target.CapturePointer(pointerId);
			request.EventHandler.StopPropagation();
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || draggedNode == null)
			{
				return;
			}
			
			draggedNode.SetPosition(evt.position);
			evt.StopPropagation();
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || draggedNode == null || !CanStopManipulation(evt))
			{
				return;
			}
			
			if (viewport.ContainsPoint(evt.position))
			{
				viewport.AddNodeToGridRegistry(draggedNode, draggedNodeData);
				draggedNode.SetPosition(evt.position);
			}
			else
			{
				target.Remove(draggedNode);
			}

			draggedNode = null;
			draggedNodeData = null;
			isActive = false;
			target.ReleasePointer(pointerId);
			evt.StopPropagation();
		}
	}
}