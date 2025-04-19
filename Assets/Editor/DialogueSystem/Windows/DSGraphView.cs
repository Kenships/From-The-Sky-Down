
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
        private DSGroupErrorScriptableDictionary groups;
        private DSGroupScriptableDictionary groupedNodes;

        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;
            
            ungroupedNodes = ScriptableObject.CreateInstance<DSNodeErrorScriptableDictionary>();
            groups = ScriptableObject.CreateInstance<DSGroupErrorScriptableDictionary>();
            groupedNodes = ScriptableObject.CreateInstance<DSGroupScriptableDictionary>();
            
            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            
            OnElementsDeleted(); 
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            
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

        public DSGroup CreateGroup(string dialogueGroup, Vector2 eventInfoLocalMousePosition)
        {
            DSGroup group = new DSGroup(dialogueGroup, eventInfoLocalMousePosition);

            AddGroup(group);
            
            return group;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            Type groupType = typeof(DSGroup);
            
            deleteSelection = (operationName, askUser) =>
            {
                List<DSNode> nodesToDelete = new List<DSNode>();
                List<DSGroup> groupsToDelete = new List<DSGroup>();
                foreach (GraphElement element in selection)
                {
                    if (element is DSNode node)
                    {
                        nodesToDelete.Add(node);
                    }

                    if (element is DSGroup group)
                    {
                        RemoveGroup(group);
                        groupsToDelete.Add(group);
                    }
                }

                foreach (DSGroup group in groupsToDelete)
                {
                    RemoveElement(group);
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
                        DSGroup nodeGroup = group as DSGroup;
                        RemoveUngroupedNode(node);
                        AddGroupedNode(node, nodeGroup);
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

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = group as DSGroup;
                
                RemoveGroup(dsGroup);
                
                dsGroup.OldTitle = newTitle;
                
                AddGroup(dsGroup);
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
        private void AddGroup(DSGroup group)
        {
            string groupName = group.title;

            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();
                
                groupErrorData.Groups.Add(group);
                
                groups.Add(groupName, groupErrorData);
                
                return;
            }
            
            List<DSGroup> groupsList = groups[groupName].Groups;
            
            groupsList.Add(group);
            
            Color errorColor = groups[groupName].ErrorData.Color;
            
            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                groupsList[0].SetErrorStyle(errorColor);
            }
        }
        
        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle;
            
            List<DSGroup> groupsList = groups[oldGroupName].Groups;
            
            groupsList.Remove(group);
            
            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }
        public void AddGroupedNode(DSNode node, DSGroup group)
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
