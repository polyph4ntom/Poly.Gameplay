using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class MPolyGridZoomDesktop : PointerManipulator
	{
		private readonly float zoomMin;
		private readonly float zoomMax;
		private readonly float zoomStep;

		private readonly VPolyGrid parentGridElement;

		public MPolyGridZoomDesktop(VPolyGrid parentGrid, float zoomMin, float zoomMax, float zoomStep)
		{
			this.parentGridElement = parentGrid;
			this.zoomMin = zoomMin;
			this.zoomMax = zoomMax;
			this.zoomStep = zoomStep;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			parentGridElement.GraphBus.RegisterBroadcaster<FPolyZoomChangedEvent>(this);
			target.RegisterCallback<WheelEvent>(OnWheelEvent);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			parentGridElement.GraphBus.UnregisterBroadcaster<FPolyZoomChangedEvent>(this);
			target.UnregisterCallback<WheelEvent>(OnWheelEvent);
		}

		private void OnWheelEvent(WheelEvent evt)
		{
			var pointerPos = evt.mousePosition;
			var delta = -evt.delta.y;

			pointerPos.y = Screen.height - pointerPos.y;

			var pointerOnGraph = parentGridElement.WorldToLocal(pointerPos);
			var newZoom = Mathf.Clamp(parentGridElement.Zoom + (delta > 0 ? zoomStep : -zoomStep), zoomMin, zoomMax);

			// Adjust panOffset to keep mouse point pinned
			var panOffset = parentGridElement.ScrollOffset + pointerOnGraph / parentGridElement.Zoom - pointerOnGraph / newZoom;
			parentGridElement.GridContent.style.scale = new StyleScale(new Vector2(newZoom, newZoom));
			parentGridElement.GridContent.style.translate = new StyleTranslate(new Vector2(-panOffset.x * newZoom, -panOffset.y * newZoom));

			parentGridElement.Zoom = newZoom;
			parentGridElement.ScrollOffset = panOffset;
			parentGridElement.GraphBus.Broadcast(this, new FPolyZoomChangedEvent(newZoom));
		}
	}
}