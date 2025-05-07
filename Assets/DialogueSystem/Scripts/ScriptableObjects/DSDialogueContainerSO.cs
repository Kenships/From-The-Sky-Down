using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DSDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializedDictionary<DSDialogueGroupSO, List<DSDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DSDialogueSO> UngroupedDialogues { get; set; }
        
        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializedDictionary<DSDialogueGroupSO, List<DSDialogueSO>>();
            UngroupedDialogues = new List<DSDialogueSO>();
        }
        
        public List<string> GetDialogueGroupNames()
        {
            List<string> groupNames = new List<string>();
            
            foreach (var group in DialogueGroups.Keys)
            {
                groupNames.Add(group.GroupName);
            }
            return groupNames;
        }
        
        public List<string> GetGroupedDialogueNames(DSDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<DSDialogueSO> dialogues = DialogueGroups[dialogueGroup];
            
            List<string> groupedDialogueNames = new List<string>();
            
            foreach (var groupedDialogue in dialogues)
            {
                if (startingDialoguesOnly && !groupedDialogue.IsStartingDialogue)
                    continue;
                groupedDialogueNames.Add(groupedDialogue.DialogueName);
            }

            return groupedDialogueNames;
        }
        
        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();
            
            foreach (var ungroupedDialogue in UngroupedDialogues)
            {
                if (startingDialoguesOnly && !ungroupedDialogue.IsStartingDialogue)
                    continue;
                ungroupedDialogueNames.Add(ungroupedDialogue.DialogueName);
            }

            return ungroupedDialogueNames;
        }
    }
}