
using System;
using System.Collections.Generic;
using DialogueSystem.Data.Error;
using DialogueSystem.Utilities;
using Obvious.Soap;
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
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;
        private DSNodeErrorScriptableDictionary ungroupedNodes;
        private DSGroupScriptableDictionary groupedNodes;
        
        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;
            
            ungroupedNodes = ScriptableObject.CreateInstance<DSNodeErrorScriptableDictionary>();
            groupedNodes = ScriptableObject.CreateInstance<DSGroupScriptableDictionary>();
            
            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            
            OnElementsDeleted(); 
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            
            AddStyles();
        }

       

        #region Overrides Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }
        #endregion
        
        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialogueType.MultipleChoice));
            this.AddManipulator(CreateGroupContextualMenu());
        }
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator createGroupContextualMenu = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));
            return createGroupContextualMenu;
        }
        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));

            return contextualMenuManipulator;
        }
        #endregion
        
        #region Elements Creation
        public DSNode CreateNode(DSDialogueType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DS{dialogueType}Node");
            DSNode node = Activator.CreateInstance(nodeType) as DSNode;
            
            node.Initialize(position, this);
            node.Draw();

            AddUngroupedNode(node);
            
            return node;
        }

        public GraphElement CreateGroup(string dialogueGroup, Vector2 eventInfoLocalMousePosition)
        {
            Group group = new Group
            {
                title = dialogueGroup,
            };
            
            group.SetPosition(new Rect(eventInfoLocalMousePosition, Vector2.zero));
            
            return group;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                List<DSNode> nodesToDelete = new List<DSNode>();
                foreach (GraphElement element in selection)
                {
                    if (element is DSNode node)
                    {
                        nodesToDelete.Add(node);
                    }
                }

                foreach (DSNode node in nodesToDelete)
                {
                    node.Group?.RemoveElement(node);
                    RemoveUngroupedNode(node);
                    
                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is DSNode node)
                    {
                        RemoveUngroupedNode(node);
                        AddGroupedNode(node, group);
                    }
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (element is DSNode node)
                    {
                        RemoveGroupedNode(node, group);
                        AddUngroupedNode(node);
                    }
                }
            };
        }

        #endregion
        
        #region Repeated Elements
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName;

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                
                nodeErrorData.Nodes.Add(node);
                
                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }
            
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            
            ungroupedNodesList.Add(node);
            
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName;
            
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            
            ungroupedNodesList.Remove(node);
            
            node.ResetStyle();

            switch (ungroupedNodesList.Count)
            {
                case 1:
                    ungroupedNodesList[0].ResetStyle();
                    return;
                case 0:
                    ungroupedNodes.Remove(nodeName);
                    break;
            }
        }
        public void AddGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName;
            
            node.Group = group;
            
            if (!groupedNodes.ContainsKey(group)){
                groupedNodes.Add(group, ScriptableObject.CreateInstance<DSNodeErrorScriptableDictionary>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                
                nodeErrorData.Nodes.Add(node);
                
                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> groupNodesList = groupedNodes[group][nodeName].Nodes;
            
            groupNodesList.Add(node);
            
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            
            node.SetErrorStyle(errorColor);

            if (groupNodesList.Count == 2)
            {
                groupNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName;

            node.Group = null;
            
            List<DSNode> groupNodesList = groupedNodes[group][nodeName].Nodes;
            
            groupNodesList.Remove(node);
            
            node.ResetStyle();

            if (groupNodesList.Count == 1)
            {
                groupNodesList[0].ResetStyle();

                return;
            }

            if (groupNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }
        #endregion
        
        #region Elements Addtion and Styles

        private void AddSearchWindow()
        {
            if (!searchWindow)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DSGraphViewStyles.uss",
                "DialogueSystem/DSNodeStyles.uss");
            
        }
        #endregion

        #region Utilities

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }
            
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }
        #endregion
    }
    
    
}
