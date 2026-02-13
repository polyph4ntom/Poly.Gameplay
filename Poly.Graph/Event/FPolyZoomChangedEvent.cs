using Poly.Events;

namespace Poly.Graph
{
    public class FPolyZoomChangedEvent : IPolyEvent
    {
        public float CurrentZoom { get; private set; }

        public FPolyZoomChangedEvent(float currentZoom)
        {
            CurrentZoom = currentZoom;
        }
    }
}
