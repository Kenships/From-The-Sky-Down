using System.Collections.Generic;
using System.Drawing;
using DialogueSystem.Data.Save;
using DialogueSystem.Elements;
using DialogueSystem.ScriptableObjects;
using DialogueSystem.Windows;
using UnityEngine;

namespace DialogueSystem.Utilities
{
    public static class DSHistoryUtility
    {
        private static LinkedList<string> _undoHistory;
        private static LinkedList<string> _redoHistory;
        
        private static DSGraphView graphView;
        
        private static string graphFileName;

        private static List<DSGroup> groups;
        private static List<DSNode> nodes;
        

        public static void Initialize()
        {
            _undoHistory = new LinkedList<string>();
            _redoHistory = new LinkedList<string>();
        }

        public static void SetGraphInfo(DSGraphView dsGraphView, string graphName)
        {
            nodes = new List<DSNode>();
            groups = new List<DSGroup>();
            
            graphView = dsGraphView;
            graphFileName = graphName;
        }

        #region Public Methods
        
        public static void Undo()
        {
            if (_undoHistory.Count == 0) return;
            //First save the snapshot for redo
            string currentSnapshot = GetSaveSnapshot();
            _redoHistory.AddFirst(currentSnapshot);
            
            //Then perform undo action
            string lastSnapshot = _undoHistory.First.Value;
            DSIOUtility.Initialize(graphView, DSEditorWindow.fileNameTextField.value);
            DSIOUtility.LoadFromJson(lastSnapshot);
            _undoHistory.RemoveFirst();
        }

        public static void Redo()
        {
            if (_redoHistory.Count == 0) return;
            string currentSnapshot = GetSaveSnapshot();
            _undoHistory.AddFirst(currentSnapshot);
            
            string nextSnapshot = _redoHistory.First.Value;
            DSIOUtility.Initialize(graphView, DSEditorWindow.fileNameTextField.value);
            DSIOUtility.LoadFromJson(nextSnapshot);
            _redoHistory.RemoveFirst();
        }
        
        public static void SaveSnapshot()
        {
            //reset redo list
            _redoHistory.Clear();
            _undoHistory.AddLast(GetSaveSnapshot());
        }
        
        #endregion
        
        #region Save Methods
        private static void SaveGroups(DSGraphSaveData graphData)
        {
            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
            }
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveData graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            graphData.Groups.Add(groupData);
        }
        
        private static void SaveNodes(DSGraphSaveData graphData)
        {
            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
            }
        }
        
        private static void SaveNodeToGraph(DSNode node, DSGraphSaveData graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);
            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                NodeID = node.ID,
                Name = node.DialogueName,
                SpeakerName = node.SpeakerName,
                ListenerName = node.ListenerName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };
            
            graphData.Nodes.Add(nodeData);
        }
        #endregion
        
        #region Quary Methods
        private static void GetElementsFromGraphView()
        {
            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DSNode dsNode)
                {
                    nodes.Add(dsNode);

                    return;
                }

                if (graphElement is DSGroup dsGroup)
                {
                    groups.Add(dsGroup);

                    return;
                }
            });
        }
        #endregion

        #region Utilities

        private static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();
            
            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    Weighting = choice.Weighting,
                    NodeID = choice.NodeID
                };
                
                choices.Add(choiceData);
            }
            
            return choices;
        }
        
        private static string GetSaveSnapshot()
        {
            GetElementsFromGraphView();
            DSGraphSaveData graphData = new DSGraphSaveData();
            graphData.Initialize(graphFileName);
            
            SaveGroups(graphData);
            SaveNodes(graphData);

            string s = JsonUtility.ToJson(graphData, true);
            
            return s;
        }

        #endregion
    }
}