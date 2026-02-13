using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyPortDropConnection : PointerManipulator
	{
		private readonly VPolyNodePort owningPort;
		
		private int pointerId;
		private bool isActive;

		public MPolyPortDropConnection(VPolyNodePort owningPort)
		{
			pointerId = -1;
			isActive = false;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
			
			this.owningPort = owningPort;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(OnPointerDown);
			target.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
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

			isActive = true;
			target.CapturePointer(evt.pointerId);
			evt.StopPropagation();
		}

		private void OnPointerUp(PointerUpEvent evt)
		{
			if (!isActive || !target.HasPointerCapture(pointerId) || !CanStopManipulation(evt))
			{
				return;
			}
			
			owningPort.DisconnectAll();

			isActive = false;
			target.ReleasePointer(pointerId);
			evt.StopPropagation();
		}
	}
}