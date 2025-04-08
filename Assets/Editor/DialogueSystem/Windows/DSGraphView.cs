
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Elements;
    public class DSGraphView : GraphView
    {
        public DSGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
        }

        private DSNode CreateNode(Vector2 position)
        {
            DSNode node = new DSNode();
            node.Initialize(position);
            node.Draw();
            return node;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(CreateNodeContextualMenu());
            this.AddManipulator(new ContentDragger());
        }

        private IManipulator CreateNodeContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Create Node", actionEvent => AddElement(CreateNode(actionEvent.eventInfo.localMousePosition))));

            return contextualMenuManipulator;
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }
        private void AddStyles()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("DialogueSystem/DSGraphViewStyles.uss") as StyleSheet;
        
            styleSheets.Add(styleSheet);
        }
    }
    
    
}
