using Poly.Events;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	public class FPolyNodeSpawnRequestEvent : IPolyEvent
	{
		public PointerDownEvent EventHandler { get; }
		public OPolyNode Node { get; }

		public FPolyNodeSpawnRequestEvent(OPolyNode node, PointerDownEvent eventHandler)
		{
			Node = node;
			EventHandler = eventHandler;
		}
	}
}
