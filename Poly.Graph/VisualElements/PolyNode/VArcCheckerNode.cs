using Poly.Graph;
using UnityEngine;
using UnityEngine.UIElements;

namespace Arc.Main
{
    [UxmlElement]
    public partial class VArcCheckerNode : VPolyNodeOneToOne<GameObject, bool>
    {
        [UxmlAttribute]
        public string CheckerText { get; set; }
        
        [UxmlAttribute]
        public Sprite CheckerIcon { get; set; }

        [UxmlAttribute]
        public Color IconColor { get; set; } = new Color(1,1,1,1);

        private Image icon;
        private Label label;

        public VArcCheckerNode() : base(false)
        {
            var iconTextArea = new VisualElement
            {
                name = "iconTextArea",
            };
            iconTextArea.AddToClassList("poly-node__content-area");
            contentParent.pickingMode = PickingMode.Ignore;

            icon = new Image
            {
                name = "checkerIcon",
            };
            icon.AddToClassList("poly-node__content-area-icon");
            contentParent.pickingMode = PickingMode.Ignore;

            label = new Label
            {
                name =  "checkerLabel",
            };
            label.AddToClassList("poly-node__content-area-label");
            contentParent.pickingMode = PickingMode.Ignore;
            
            iconTextArea.Add(icon);
            iconTextArea.Add(label);
            content.Add(iconTextArea);
            
            RegisterCallback<AttachToPanelEvent>(evt =>
            {
                UpdateElement();
            });
        }
        
        private void UpdateElement()
        {
            label.text = CheckerText;
            icon.style.backgroundImage = new StyleBackground(CheckerIcon);
            icon.style.unityBackgroundImageTintColor = IconColor;
        }
    }
}
