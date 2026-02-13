using Poly.Common;
using Poly.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	[UxmlElement]
	public partial class VPolyNodeButton : VisualElement
	{
		private OPolyNode node;

		public FPolyAction<PointerDownEvent, OPolyNode> OnNodeButtonClicked { get; } = new();
		
		[UxmlAttribute]
		public Sprite ButtonSprite { get; set; }

		public VPolyNodeButton()
		{
			AddToClassList("poly-node-button");
		}

		public VPolyNodeButton(OPolyNode node)
		{
			AddToClassList("poly-node-button");

			this.node = node;
			ButtonSprite = node.Icon;
			style.unityBackgroundImageTintColor = node.NodeIconColor;

			pickingMode = PickingMode.Position;

			RegisterCallback<PointerDownEvent>(evt =>
			{
				if (evt.button != 0)
				{
					return;
				}

				OnNodeButtonClicked.Broadcast(evt, node);
			});
			
			RegisterCallback<AttachToPanelEvent>(evt =>
			{
				UpdateElement();
			});
		}
		
		private void UpdateElement()
		{
			style.backgroundImage = new StyleBackground(ButtonSprite);
		}
	}
}