using System;
using System.Collections.Generic;
using Poly.Common;
using Poly.CommonUI;
using Poly.Events;
using UI.Poly.CommonUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
	[UxmlElement]
	public partial class VPolyGraph : VisualElement
	{
		public FPolyEventBus GraphEventBus { get; } = new();
		
		#region UXML Declarations
		[UxmlAttribute] 
		public float OuterGridSpacing { get; set; } = 260f;

		[UxmlAttribute] 
		public float InnerGridSpacing { get; set; } = 26f;
        
		[UxmlAttribute]
		public Color OuterGridColor { get; set; } = new(0f, 0f, 0f, 0.8f);

		[UxmlAttribute]
		public Color InnerGridColor { get; set; } = new(0.2f, 0.2f, 0.2f, 1f);

		[UxmlAttribute] 
		public float ZoomMin { get; set; } = 0.5f;
        
		[UxmlAttribute] 
		public float ZoomMax { get; set; } = 2f;
       
		[UxmlAttribute] 
		public float ZoomStep { get; set; } = 0.1f;
		#endregion
		
		public readonly MPolyGraphNodeSpawnerDesktop spawnerDesktopManipulator;

		private readonly Button exitButton;
		private readonly Label zoomLabel;
		private readonly VisualElement graphContent;

		private readonly Dictionary<string, VPolyExpandableTab> registeredExpandableTag = new();

		public FPolyAction OnExitClicked { get; } = new();
		
		public VPolyGrid GridViewport { get; private set; }
		
		public VPolyGraph()
		{
			AddToClassList("poly-graph");

			graphContent = new VisualElement()
			{
				name = "graph-content"
			};
			graphContent.AddToClassList("poly-graph__content");
			
			GridViewport = new VPolyGrid(this)
			{
				name = "viewport"
			};
			
			exitButton = new Button
			{
				name = "exit",
			};
			exitButton.AddToClassList("poly-graph__exit");
			exitButton.AddToClassList("arc-panel-icon-button");

			var movementIndicator = new VisualElement
			{
				name = "movementIndicator",
			};
			movementIndicator.AddToClassList("poly-graph__movement-indicator");

			zoomLabel = new Label
			{
				name = "zoomLabel",
				text = $"x{GridViewport.Zoom:F1}"
			};
			zoomLabel.AddToClassList("poly-graph__zoom-label");
			
			graphContent.Add(GridViewport);
			graphContent.Add(exitButton);
			graphContent.Add(movementIndicator);
			graphContent.Add(zoomLabel);

			Add(graphContent);
			
			GridViewport.OnGridClicked.AddUnique(this, OnGridClicked);
			
			spawnerDesktopManipulator = new MPolyGraphNodeSpawnerDesktop(GridViewport);
			this.AddManipulator(spawnerDesktopManipulator);
			
			GraphEventBus.Subscribe<FPolyZoomChangedEvent>(this, OnZoomUpdated);
			GraphEventBus.Subscribe<FPolyNodeSpawnRequestEvent>(this, spawnerDesktopManipulator.OnNodeSpawnRequest);
			
			exitButton.clicked += () => OnExitClicked.Broadcast();
		}
		
		public void ClosePanel()
		{
			parent.RemoveFromClassList("arc-base-panel__showNoOp");
			parent.AddToClassList("arc-base-panel__hideBottomNoOp");
			
			parent.schedule.Execute(SetDisplayToNone).ExecuteLater(300);
			
			parent.pickingMode = PickingMode.Ignore;
		}
		
		protected void SetDisplayToNone()
		{
			parent.style.display = DisplayStyle.None;
		}

		private void SetShown()
		{
			parent.RemoveFromClassList("arc-base-panel__hideBottomNoOp");
			parent.AddToClassList("arc-base-panel__showNoOp");
		}

		public void ShowPanel()
		{
			parent.style.display = DisplayStyle.Flex;
			parent.schedule.Execute(SetShown).ExecuteLater(25);
		}

		private void OnZoomUpdated(FPolyZoomChangedEvent evt)
		{
			zoomLabel.text = $"x{evt.CurrentZoom:F1}";
		}

		private void OnGridClicked()
		{
			
		}
	}
}
