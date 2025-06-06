using System.Collections.Generic;
using DialogueSystem.Enumerations;
using DialogueSystem.ScriptableObjects;
using Obvious.Soap;
using UnityEngine;
namespace DialogueSystem
{
    public class DSDialogue: MonoBehaviour
    {
        /* Dialogue Scriptable Objects */

        [SerializeField] private DSDialogueContainerSO _dialogueContainer;
        [SerializeField] private DSDialogueGroupSO _dialogueGroup;
        [SerializeField] private DSDialogueSO _dialogue;

        /* Filters */

        [SerializeField] private bool _groupedDialogues;
        [SerializeField] private bool _startingDialoguesOnly;

        /* Indices */
        
        [SerializeField] private int _selectedDialogueGroupIndex;
        [SerializeField] private int _selectedDialogueIndex;
        
        public DSDialogueSO StartingDialogue => _dialogue;
    }
}