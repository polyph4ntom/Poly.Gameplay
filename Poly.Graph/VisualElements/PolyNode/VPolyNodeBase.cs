using System;
using Poly.Common;
using Poly.Events;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poly.Graph
{
    public struct FPolyNewConnectionStatus
    {
        public bool isConnectionRequest;
        public FPolyConnectionData from;
        public FPolyConnectionData to;
    }

    [UxmlElement]
    public abstract partial class VPolyNodeBase : VisualElement
    {
        public Guid Id { get; } = Guid.NewGuid();
        public OPolyNode NodeData { get; set; }
        
        public FPolyAction<Guid> OnNodeClicked { get; } = new();
        public FPolyAction<Guid> OnNodeDeletionRequest { get; } = new();
        public FPolyAction<OPolyNode> OnNodeInfoRequest { get; } = new();
        public FPolyAction<FPolyNewConnectionStatus> OnConnectionStatusUpdateRequest { get; } = new();
        
        protected readonly VisualElement inputArea;
        protected readonly VisualElement outputArea;
        protected readonly VisualElement content;
        protected readonly VisualElement contentParent;
        protected readonly VisualElement topBar;

        private readonly TextElement nodeTitle;
        private readonly Button deleteNode;
        private readonly Button infoNode;
        
#if UNITY_EDITOR
        [UxmlAttribute]
        public bool DebugSelected { get; set; }
#endif        
        [UxmlAttribute]
        public string NodeName { get; set; }
        
        [UxmlAttribute]
        public Sprite NodeTexture { get; set; }
        
        public string NodeDescription { get; set; }
        public Sprite NodeIcon { get; set; }
        
        [UxmlAttribute]
        public Color TintColor { get; set; } = new Color(1,1,1,0.6f);
        
        private MPolyNodeDragDesktop dragDesktop;

        private bool isNodeDeletable = true;

        public abstract void HideEdges();
        public abstract void ShowEdges();
        

        protected VPolyNodeBase()
        {
            AddToClassList("poly-node");
            pickingMode = PickingMode.Ignore;
            
            inputArea = new VisualElement
            {
                 name = "inputArea"
            };
            inputArea.AddToClassList("poly-node__inputArea");
            inputArea.pickingMode = PickingMode.Ignore;
            
            outputArea = new VisualElement
            {
                 name = "outputArea"
            };
            outputArea.AddToClassList("poly-node__outputArea");
            outputArea.pickingMode = PickingMode.Ignore;
            
            nodeTitle = new TextElement
            {
                 name = "nodeTitle",
                 text = NodeName
            };
            nodeTitle.AddToClassList("poly-node__title");
            nodeTitle.pickingMode = PickingMode.Ignore;
            
            deleteNode = new Button
            {
                name = "deleteNode",
            };
            deleteNode.AddToClassList("poly-node__delete");
            deleteNode.pickingMode = PickingMode.Position;
            deleteNode.visible = false;
            
            infoNode = new Button
            {
                name = "infoNode",
            };
            infoNode.AddToClassList("poly-node__info");
            infoNode.pickingMode = PickingMode.Position;
            
            contentParent = new VisualElement
            {
                name = "innerContainer",
            };
            contentParent.AddToClassList("poly-node__inner");
            contentParent.pickingMode = PickingMode.Position;
            contentParent.style.backgroundImage = new StyleBackground(NodeTexture);
            
            content = new VisualElement
            {
                name = "innerContainerContent"
            };
            content.AddToClassList("poly-node__content");
            content.pickingMode = PickingMode.Ignore;
            
            topBar = new VisualElement
            {
                name = "nodeTopBar"
            };
            topBar.AddToClassList("poly-node__topBar");
            topBar.pickingMode = PickingMode.Ignore;
            topBar.Add(deleteNode);
            topBar.Add(nodeTitle);
            topBar.Add(infoNode);
            
            Add(topBar);
            
            contentParent.Add(content);
            Add(contentParent);
            
            Add(inputArea);
            Add(outputArea);
            
            RegisterCallback<PointerDownEvent>(evt =>
            {
                OnNodeClicked.Broadcast(Id);
            }, TrickleDown.TrickleDown);

            deleteNode.clicked += () =>
            {
                OnNodeDeletionRequest.Broadcast(Id);
            };

            infoNode.clicked += () =>
            {
                OnNodeInfoRequest.Broadcast(NodeData);
            };

            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateElement();
            });
        }

        private void UpdateElement()
        {
            nodeTitle.text = NodeName;
            contentParent.style.backgroundImage = new StyleBackground(NodeTexture);
            contentParent.style.unityBackgroundImageTintColor = TintColor;

#if UNITY_EDITOR
            if (DebugSelected)
            {
                MarkAsSelected();
            }
            else
            {
                ClearSelection();
            }
#endif
        }

        internal abstract void OnNodeRegisteredToGrid(VisualElement gridContentElement);
        internal abstract void OnNodeRemovedFromGrid();
        internal abstract void OnPositionUpdated();
        internal abstract void OnConnectionApplied(FPolyConnectionData from, FPolyConnectionData to);
        internal abstract void OnConnectionRemoved(FPolyConnectionData from, FPolyConnectionData to);

        internal void MarkAsSelected()
        {
            contentParent.style.unityBackgroundImageTintColor = FPolyColor.FromRGBA(255,165,0,0.8f);
            deleteNode.visible = isNodeDeletable;
            infoNode.visible = NodeData.HasTutorial;
        }

        internal void ClearSelection()
        {
            contentParent.style.unityBackgroundImageTintColor = TintColor;
            deleteNode.visible = false;
            infoNode.visible = false;
        }

        internal void RegisterDragManipulator()
        {
            dragDesktop = new MPolyNodeDragDesktop();
            contentParent.AddManipulator(dragDesktop);
        }

        internal void UnregisterDragManipulator()
        {
            contentParent.RemoveManipulator(dragDesktop);
            dragDesktop = null;
        }

        //TODO: FIXME: THAT SHOULD BE INTERNAL
        public void SetPosition(Vector3 position)
        {
            var localPos = parent.WorldToLocal(position);

            var size = new Vector3(resolvedStyle.width, resolvedStyle.height);
            var center = size * 0.5f;
            var final = localPos - (Vector2) center;

            visible = true;
            style.translate = new StyleTranslate(new Translate(final.x, final.y));
        }
        
        public void SetTitle(string title)
        {
            this.nodeTitle.text = title;
        }

        public void SetIsDeletable(bool isDeletable)
        {
            this.isNodeDeletable = isDeletable;
        }
    }
}