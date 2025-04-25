
using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using Data.Error;
    using Data.Save;
    
    
    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;
        private SerializedDictionary<string, DSNodeErrorData> ungroupedNodes;
        private SerializedDictionary<string, DSGroupErrorData> groups;
        private SerializedDictionary<Group, SerializedDictionary<string, DSNodeErrorData>> groupedNodes;
        private int _namesErrorCount;
        public int NamesErrorCount
        {
            get => _namesErrorCount;
            set
            {
                _namesErrorCount = value;
                if (_namesErrorCount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (_namesErrorCount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }
        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            ungroupedNodes = new SerializedDictionary<string, DSNodeErrorData>();
            groups = new SerializedDictionary<string, DSGroupErrorData>();
            groupedNodes = new SerializedDictionary<Group, SerializedDictionary<string, DSNodeErrorData>>();
            
            AddManipulators();
            AddSearchWindow();
            AddGridBackground();
            
            OnElementsDeleted(); 
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();
            
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
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))));
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
            
            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (selectedElement is DSNode node)
                {
                    group.AddElement(node);
                }
            }
            
            return group;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                List<DSNode> nodesToDelete = new List<DSNode>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<DSGroup> groupsToDelete = new List<DSGroup>();
                foreach (GraphElement element in selection)
                {
                    if (element is DSNode node)
                    {
                        nodesToDelete.Add(node);
                    }
                    
                    if (element is Edge edge)
                    {
                        edgesToDelete.Add(edge);
                    }

                    if (element is DSGroup group)
                    {
                        groupsToDelete.Add(group);
                    }
                }

                foreach (DSGroup group in groupsToDelete)
                {
                    List<DSNode> groupedNodes = new List<DSNode>();

                    foreach (GraphElement element in group.containedElements)
                    {
                        if (element is DSNode node)
                        {
                            groupedNodes.Add(node);
                        }
                    }
                    
                    group.RemoveElements(groupedNodes);
                    
                    RemoveGroup(group);
                    
                    RemoveElement(group);
                }
                
                DeleteElements(edgesToDelete);
                
                foreach (DSNode node in nodesToDelete)
                {
                    node.Group?.RemoveElement(node);
                    
                    RemoveUngroupedNode(node);
                    
                    node.DisconnectAllPorts();
                    
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

                dsGroup.title = newTitle.RemoveWhiteSpaces().RemoveSpecialCharacters();
                
                if(string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        ++NamesErrorCount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        --NamesErrorCount;
                    }
                }
                
                RemoveGroup(dsGroup);
                
                dsGroup.OldTitle = dsGroup.title;
                
                AddGroup(dsGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = edge.input.node as DSNode;
                        
                        DSNodeSaveData choiceData = edge.output.userData as DSNodeSaveData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            DSChoiceSaveData choiceData = edge.output.userData as DSChoiceSaveData;
                            
                            choiceData.NodeID = "";
                        }
                    }
                }

                return changes;
            };
        }

        #endregion
        
        #region Repeated Elements
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

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
                ++NamesErrorCount;
                
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }
        
        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            
            ungroupedNodesList.Remove(node);
            
            node.ResetStyle();

            switch (ungroupedNodesList.Count)
            {
                case 1:
                    --NamesErrorCount;  
                    ungroupedNodesList[0].ResetStyle();
                    return;
                case 0:
                    ungroupedNodes.Remove(nodeName);
                    break;
            }
        }
        private void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();

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
                ++NamesErrorCount;
                groupsList[0].SetErrorStyle(errorColor);
            }
        }
        
        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle.ToLower();
            
            List<DSGroup> groupsList = groups[oldGroupName].Groups;
            
            groupsList.Remove(group);
            
            group.ResetStyle();

            switch (groupsList.Count)
            {
                case 1:
                    --NamesErrorCount;
                    groupsList[0].ResetStyle();
                    return;
                case 0:
                    groups.Remove(oldGroupName);
                    break;
            }
        }
        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();
            
            node.Group = group;
            
            if (!groupedNodes.ContainsKey(group)){
                groupedNodes.Add(group, new SerializedDictionary<string, DSNodeErrorData>());
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
                ++NamesErrorCount;
                groupNodesList[0].SetErrorStyle(errorColor);
            }
        }
        
        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;
            
            List<DSNode> groupNodesList = groupedNodes[group][nodeName].Nodes;
            
            groupNodesList.Remove(node);
            
            node.ResetStyle();

            if (groupNodesList.Count == 1)
            {
                --NamesErrorCount;
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
