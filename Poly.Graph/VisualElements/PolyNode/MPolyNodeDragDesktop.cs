using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyNodeDragDesktop : PointerManipulator
	{
		private int pointerId;
		private Vector3 startDragPosition;
		private bool isActive;
		
		public MPolyNodeDragDesktop()
		{
			pointerId = -1;
			isActive = false;
			activators.Add(new ManipulatorActivationFilter{ button = MouseButton.LeftMouse });
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

			startDragPosition = evt.localPosition;
			pointerId = evt.pointerId;

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

			var delta = evt.localPosition - startDragPosition;
			var currentPos = target.parent.resolvedStyle.translate;
			target.parent.style.translate = new StyleTranslate(new Translate(currentPos.x + delta.x, currentPos.y + delta.y));
			((VPolyNodeBase) target.parent).OnPositionUpdated();

			evt.StopPropagation();
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
		}
	}
}