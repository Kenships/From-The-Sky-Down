using System;
using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Enumerations;
using DialogueSystem.ScriptableObjects;
using Obvious.Soap;
using Player.Input;
using TMPro;
using UI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;
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
        
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private InputReaderSO inputReader;
        
        private List<Button> _activeChoices = new();
        private int activeChoiceIndex;

        private bool _firstUpdate;
        public void OnEnable()
        {
            inputReader.SetInputState(InputState.Dialogue);
            inputReader.RequestNextDialogue += GetNextDialogue;
            inputReader.DialogueScrollDirection += ScrollSelectChoice;
            
            var dialogueSelector = GetComponent<DSDialogue>();
            CurrentDialogue = dialogueSelector.StartingDialogue;
            
            _firstUpdate = true;
        }

        private void ScrollSelectChoice(float direction)
        {
            if (_activeChoices.Count == 0) return;
            
            if (Mathf.Approximately(direction, 1))
            {
                activeChoiceIndex = (activeChoiceIndex + 1) % _activeChoices.Count;
            }
            else if (Mathf.Approximately(direction, -1))
            {
                activeChoiceIndex = (activeChoiceIndex - 1) % _activeChoices.Count;
            }
            
            //Deal with Negative mod numbers
            int selectIndex = activeChoiceIndex < 0 ? activeChoiceIndex + _activeChoices.Count : activeChoiceIndex;
            
            _activeChoices[selectIndex].Select(); 
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
            inputReader.SetInputState(InputState.Default);
            inputReader.RequestNextDialogue -= GetNextDialogue;
        }

        private void GetNextDialogue()
        {
            if (CurrentDialogue.DialogueType == DSDialogueType.MultipleChoice) return;

            CurrentDialogue = CurrentDialogue.Choices[0].NextDialogue;
        }

        private void UpdateDialogueDisplay()
        {
            //Clear Choices
            _activeChoices.Clear();
            foreach (Transform option in choiceContainer)
            {
                Destroy(option.gameObject);
            }
            
            //Add new Choices
            speakerName.text = CurrentDialogue.SpeakerName;
            dialogueText.text = CurrentDialogue.Text;

            if (CurrentDialogue.DialogueType == DSDialogueType.MultipleChoice)
            {
                foreach (var choice in CurrentDialogue.Choices)
                {
                    _activeChoices.Add(CreateChoiceButton(choice));
                }
            }

            if (_activeChoices.Count > 0)
            {
                activeChoiceIndex = _activeChoices.Count - 1;
                _activeChoices[activeChoiceIndex].Select();
            }
                
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContainer.GetComponent<RectTransform>());
        }

        private Button CreateChoiceButton(DSDialogueChoiceData choiceText)
        {
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
            CallbackButton buttonComponent = choiceButton.GetComponent<CallbackButton>();
            TextMeshProUGUI textMesh = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = choiceText.Text;

            buttonComponent.onClick.AddListener(() =>
            {
                CurrentDialogue = choiceText.NextDialogue;
            });
            
            buttonComponent.SetHoverCallBack(() =>
            {
                activeChoiceIndex = _activeChoices.IndexOf(buttonComponent);
            });

            return buttonComponent;
        }
        
    }
}
