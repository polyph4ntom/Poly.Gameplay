using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyPortConnection : PointerManipulator
	{
		private readonly VPolyNodePort owningPort;
		private readonly VisualElement content;
		
		private VPolyNodeEdge tempEdge;

		private int pointerId;
		private bool isActive;

		public MPolyPortConnection(VPolyNodePort owningPort, VisualElement content)
		{
			pointerId = -1;
			isActive = false;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });

			this.content = content;
			this.owningPort = owningPort;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(OnPointerDown);
			target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			target.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent evt)
		{
			if (isActive)
			{
				evt.StopImmediatePropagation();
				return;
			}

			if (!CanStartManipulation(evt))
			{
				return;
			}
			
			tempEdge = new VPolyNodeEdge(FPolyGraphAssembly.portColorsMapping[owningPort.compatibleDataType])
			{
				fromPort = owningPort
			};
			content.Add(tempEdge);
			
			pointerId = evt.pointerId;

			var portPosition = target.resolvedStyle.translate;
			var portCenterOffset = new Vector3(target.resolvedStyle.width / 2, target.resolvedStyle.height / 2);
			portPosition += portCenterOffset;
			
			tempEdge.start = content.WorldToLocal(target.LocalToWorld(portPosition));
			tempEdge.end = content.WorldToLocal(evt.position);

			isActive = true;
			target.CapturePointer(evt.pointerId);
			evt.StopPropagation();
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || tempEdge == null)
			{
				return;
			}

			tempEdge.end = content.WorldToLocal(evt.position);
			tempEdge.MarkDirtyRepaint();
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || !CanStopManipulation(evt))
			{
				return;
			}
			isActive = false;
			target.ReleasePointer(pointerId);
			evt.StopPropagation();
			
			var hit = content.panel.Pick(evt.position);
			if (hit is VPolyNodePort targetPort && targetPort != target)
			{
				if (targetPort.CanConnectTo(owningPort))
				{
					targetPort.SetConnection(tempEdge, owningPort);
					evt.StopPropagation();
					return;
				}
			}
			
			tempEdge.RemoveFromHierarchy();
			tempEdge = null;
		}
	}
}