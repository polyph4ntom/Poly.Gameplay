using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyGridPanDesktop : PointerManipulator
	{
		private int pointerId;
		private bool isActive;
		
		private Vector3 dragViewportStart;
		private Vector3 panOffset = Vector3.zero;

		private readonly VPolyGrid parentGridElement;
		
		public MPolyGridPanDesktop(VPolyGrid parentGrid)
		{
			parentGridElement = parentGrid;
			
			pointerId = -1;
			isActive = false;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
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

			pointerId = evt.pointerId;
			dragViewportStart = evt.localPosition;
			
			isActive = true;
			target.CapturePointer(pointerId);
			evt.StopPropagation();
		}

		private void OnPointerMove(PointerMoveEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId))
			{
				return;
			}

			panOffset = parentGridElement.ScrollOffset;
			
			var currentPosition = evt.localPosition;
			var delta = currentPosition - dragViewportStart;
			
			dragViewportStart = currentPosition;
			delta *= -1;

			panOffset += delta / parentGridElement.Zoom;
			
			parentGridElement.GridContent.style.translate = new StyleTranslate(new Vector2(-panOffset.x * parentGridElement.Zoom, -panOffset.y * parentGridElement.Zoom));
			parentGridElement.GridContent.style.scale = new StyleScale(new Vector2(parentGridElement.Zoom, parentGridElement.Zoom));

			parentGridElement.ScrollOffset = panOffset;
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || !CanStopManipulation(evt))
			{
				return;
			}
				
			isActive = false;
			target.ReleaseMouse();
			evt.StopPropagation();
		}
	}
}