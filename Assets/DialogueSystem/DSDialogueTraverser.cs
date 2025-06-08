using System;
using System.Collections.Generic;
using CharacterController;
using DialogueSystem.Data;
using DialogueSystem.Enumerations;
using DialogueSystem.ScriptableObjects;
using Obvious.Soap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem
{
    [RequireComponent(typeof(DSDialogue))]
    public class DSDialogueTraverser: MonoBehaviour
    {
        private DSDialogueSO CurrentDialogue
        {
            get => _currentDialogue;
            set
            {
                if (!value)
                {
                    Destroy(gameObject);
                    return;
                }
                
                _currentDialogue = value;
                UpdateDialogueDisplay();
            }   
        }
        private DSDialogueSO _currentDialogue;
        
        private List<DSDialogueChoiceData> choices;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private InputReaderSO inputReader;

        private bool _firstUpdate;
        public void OnEnable()
        {
            inputReader.SetInputState(InputState.Dialogue);
            inputReader.RequestNextDialogue += GetNextDialogue;
            
            var dialogueSelector = GetComponent<DSDialogue>();
            CurrentDialogue = dialogueSelector.StartingDialogue;
            
            _firstUpdate = true;
        }

        private void Update()
        {
            if (_firstUpdate)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContainer.GetComponent<RectTransform>());
                _firstUpdate = false;
            }
        }

        public void OnDisable()
        {
            inputReader.SetInputState(InputState.Any);
            inputReader.RequestNextDialogue -= GetNextDialogue;
        }

        private void GetNextDialogue()
        {
            if (CurrentDialogue.DialogueType == DSDialogueType.MultipleChoice) return;

            CurrentDialogue = CurrentDialogue.Choices[0].NextDialogue;
        }

        private void UpdateDialogueDisplay()
        {
            foreach (Transform option in choiceContainer)
            {
                Destroy(option.gameObject);
            }
            
            speakerName.text = CurrentDialogue.SpeakerName;
            dialogueText.text = CurrentDialogue.Text;

            if (CurrentDialogue.DialogueType == DSDialogueType.MultipleChoice)
            {
                foreach (var choice in CurrentDialogue.Choices)
                {
                    CreateChoiceButton(choice);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContainer.GetComponent<RectTransform>());
        }

        private void CreateChoiceButton(DSDialogueChoiceData choiceText)
        {
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            Button buttonComponent = choiceButton.GetComponent<Button>();
            TextMeshProUGUI textMesh = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = choiceText.Text;

            buttonComponent.onClick.AddListener(() =>
            {
                CurrentDialogue = choiceText.NextDialogue;
            });
        }
        
    }
}