using System;
using DialogueSystem.Enumerations;
using UnityEngine;

namespace DialogueSystem.Data
{
    using ScriptableObjects;
    [Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public int Weighting { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }
    }
}