
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Elements;
    using Enumerations;
    public class DSGraphView : GraphView
    {
        public DSGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
        }

        private DSNode CreateNode(DSDialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DS{dialogueType}Node");
            DSNode node = Activator.CreateInstance(nodeType) as DSNode;
            node.Initialize(position);
            node.Draw();
            return node;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialogueType.MultipleChoice));
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, actionEvent.eventInfo.localMousePosition))));

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
            StyleSheet graphStyleSheet = EditorGUIUtility.Load("DialogueSystem/DSGraphViewStyles.uss") as StyleSheet;
            StyleSheet nodeStyleSheet = EditorGUIUtility.Load("DialogueSystem/DSNodeStyles.uss") as StyleSheet;
        
            styleSheets.Add(graphStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }
    }
    
    
}
