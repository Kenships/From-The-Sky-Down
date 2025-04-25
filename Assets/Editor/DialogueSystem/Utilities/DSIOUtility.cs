using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Codice.Client.BaseCommands.BranchExplorer;
using DialogueSystem.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Utilities
{
    using Data.Save;
    using ScriptableObjects;
    using Elements;
    using Windows;
    public class DSIOUtility
    {
        private static DSGraphView graphView;
        
        private static string graphFileName;
        private static string containerFolderPath;

        private static List<DSGroup> groups;
        private static List<DSNode> nodes;

        private static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, DSDialogueSO> createdDialogues;
        
        public static void Initialize(DSGraphView dsGraphView, string graphName)
        {
            graphView = dsGraphView;
            graphFileName = graphName;
            containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphFileName}";

            groups = new List<DSGroup>();
            nodes = new List<DSNode>();
            
            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogues = new Dictionary<string, DSDialogueSO>();
        }
        
        
        #region Save Methods
        
        public static void Save()
        {
            CreateStaticFolder();

            GetElementsFromGraphView();

            DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");
            
            graphData.Initialize(graphFileName);

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);
           
            dialogueContainer.Initialize(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            
            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        

        #region Groups
        private static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();
            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);
                
                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            DSGroupSaveData groupData = new DSGroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            
            graphData.Groups.Add(groupData);
        }

        private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);
            dialogueGroup.Initialize(groupName);
            
            createdDialogueGroups.Add(group.ID, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }
            
            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        #endregion

        #region Nodes
        private static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializedDictionary<string, List<string>> groupedNodeNames = new SerializedDictionary<string, List<string>>();
            List<string> ungroupedNodeName = new List<string>();
            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                    
                    continue;
                }
                
                ungroupedNodeName.Add(node.DialogueName);
            }

            UpdateDialogueChoiceConnections();
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeName, graphData);
        }

        

        private static void UpdateDialogueChoiceConnections()
        {
            /*foreach (DSNode node in nodes)
            {
                DSDialogueSO dialogue = createdDialogues[node.ID];
                
                for(int choiceIndex = 0; choiceIndex < node.Choices.Count; choiceIndex++)
                {
                    DSChoiceSaveData choice = node.Choices[choiceIndex];
                    
                    if (!string.IsNullOrEmpty(choice.NodeID))
                    {
                        dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[choice.NodeID];
                        SaveAsset(dialogue);
                    }
                }
            }*/
        }

        private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();
            foreach (DSChoiceSaveData choice in node.Choices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };
                
                choices.Add(choiceData);
            }
            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                NodeID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };
            
            graphData.Nodes.Add(nodeData);
        }

        private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.Group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
                
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }
            
            dialogue.Initialize(node.DialogueName, node.Text, ConvertNodeChoicesToDialogueChoices(node.Choices), node.DialogueType, node.IsStartNode());
            
            createdDialogues.Add(node.ID, dialogue);
            
            SaveAsset(dialogue);
        }

        private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List <DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();
            
            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    Text = nodeChoice.Text,
                };
                
                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }
        private static void UpdateOldGroupedNodes(SerializedDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach(KeyValuePair<string, List<string>> oldGroupedNodes in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();
                    
                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNodes.Key))
                    {
                        nodesToRemove = oldGroupedNodes.Value.Except(currentGroupedNodeNames[oldGroupedNodes.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNodes.Key}/Dialogues", nodeToRemove);
                    }
                }
            }
            
            graphData.OldGroupedNodeNames = new SerializedDictionary<string, List<string>>(currentGroupedNodeNames);
        }
        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeName, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldUngroupedNames != null && graphData.OldUngroupedNames.Count != 0)
            {
                List<string> nodesToRemove = graphData.OldUngroupedNames.Except(currentUngroupedNodeName).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }
            
            graphData.OldGroupNames = new List<string>(currentUngroupedNodeName);
        }

        

        #endregion

        #endregion

        #region Creation Methods
        private static void CreateStaticFolder()
        {
            CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
            CreateFolder("Assets", "DialogueSystem");
            CreateFolder("Assets/DialogueSystem", "Dialogues");
            CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "dialogues");
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
        
        #region Utility Methods
        private static void CreateFolder(string path, string folderName)
        {
            if(AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }
        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }
        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";

            T asset = AssetDatabase.LoadAssetAtPath<T>(fullPath);

            if (!asset)
            {
                asset = ScriptableObject.CreateInstance<T>();
            }
            
            AssetDatabase.CreateAsset(asset, fullPath);

            return asset;
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        private static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
