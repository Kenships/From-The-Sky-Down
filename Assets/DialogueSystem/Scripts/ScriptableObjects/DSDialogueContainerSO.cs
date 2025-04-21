using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    public class DSDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializedDictionary<DSDIalogueGroupSO, List<DSDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<DSDialogueSO> UngroupedDialogues { get; set; }
        
        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializedDictionary<DSDIalogueGroupSO, List<DSDialogueSO>>();
            UngroupedDialogues = new List<DSDialogueSO>();
        }
    }
}