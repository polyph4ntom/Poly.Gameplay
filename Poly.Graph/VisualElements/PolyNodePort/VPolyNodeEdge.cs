using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class VPolyNodeEdge : VisualElement
	{
		internal VPolyNodePort fromPort;
		internal VPolyNodePort toPort;

		internal Vector2 start;
		internal Vector2 end;

		private Color lineColor;

		public VPolyNodeEdge(Color color)
		{
			style.position = Position.Absolute;
			pickingMode = PickingMode.Ignore;
			generateVisualContent += OnGenerateVisualContent;

			lineColor = color;
			lineColor.a = 0.3f;
		}

		private void OnGenerateVisualContent(MeshGenerationContext ctx)
		{
			var painter = ctx.painter2D;
			painter.lineWidth = 5f;
			painter.strokeColor = lineColor;

			// --- STEP 1: Fixed offset direction ---
			var offsetLength = 10f;

			var startOffsetDir = fromPort.portType == PolyPortType.Output ? Vector2.right * 1.5f : Vector2.left * 1.5f;
			var endOffsetDir = startOffsetDir * -1; 

			var startPos = this.start + startOffsetDir * offsetLength;
			var endPos   = this.end   + endOffsetDir   * offsetLength;

			// --- STEP 2: Tangents ---
			var handleLength = Mathf.Min(100f, Mathf.Abs(end.x - start.x) * 0.5f);

			var startTangent = startPos + startOffsetDir * handleLength;
			var endTangent   = endPos + endOffsetDir * handleLength;

			// --- STEP 3: Draw path ---
			painter.BeginPath();

			// 1. Flat start line
			painter.MoveTo(start);
			painter.LineTo(start);

			// 2. Bézier between intermediate points
			painter.BezierCurveTo(startTangent, endTangent, end);

			// 3. Flat end line
			painter.LineTo(end);

			painter.Stroke();
		}

		public void Refresh()
		{
			var portCenterOffset = new Vector3(fromPort.resolvedStyle.width / 2, fromPort.resolvedStyle.height / 2);
			
			var portPosition = fromPort.resolvedStyle.translate;
			portPosition += portCenterOffset;
			
			start = parent.WorldToLocal(fromPort.LocalToWorld(portPosition));
			
			portPosition = toPort.resolvedStyle.translate;
			portPosition += portCenterOffset;
			
			end = parent.WorldToLocal(toPort.LocalToWorld(portPosition));
			lineColor.a = 1f;
			
			MarkDirtyRepaint();
		}
	}
}