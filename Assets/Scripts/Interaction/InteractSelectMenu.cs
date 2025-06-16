using System;
using System.Collections.Generic;
using CharacterController;
using DialogueSystem.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class InteractSelectMenu : MonoBehaviour
    {
        [SerializeField] private ScriptableEventRadarBogieInfo radarBogieInfo;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private IInteractableVariable currentInteractable;
        private readonly Dictionary<GameObject, CallbackButton> _activeButtonsDictionary = new();
        private int _currentSelectionIndex;
        
        
        private void Start()
        {
            radarBogieInfo.OnRaised += RegisterBogie;
            inputReader.ScrollDirection += ScrollSelect;
            inputReader.OnStateChange += SetEnableDisable;
            
        }

        private void SetEnableDisable(InputState inputState)
        {
            gameObject.SetActive(inputState == InputState.Movement);
        }

        private void ScrollSelect(float direction)
        {
            if (transform.childCount == 0) return;
            
            if (Mathf.Approximately(direction, 1))
            {
                _currentSelectionIndex = (_currentSelectionIndex + 1) % transform.childCount;
            }
            else if (Mathf.Approximately(direction, -1))
            {
                _currentSelectionIndex = (_currentSelectionIndex - 1) % transform.childCount;
            }
            
            //Deal with Negative mod numbers
            int selectIndex = _currentSelectionIndex < 0 ? _currentSelectionIndex + transform.childCount : _currentSelectionIndex;
            
            transform.GetChild(selectIndex).GetComponent<CallbackButton>().Select(); 
        }

        private void RegisterBogie(RadarBogieInfo bogieInfo)
        {
            GameObject bogie = bogieInfo.Bogie;

            //Return if not interactable
            if (bogie.TryGetComponent(out IInteractable interactable))
            {
                
                if (bogieInfo.InRange)
                {
                    interactable.HoverSelect();
                    CallbackButton button = Instantiate(choiceButtonPrefab, transform).GetComponent<CallbackButton>();

                    Navigation noNav = new Navigation
                    {
                        mode = Navigation.Mode.None
                    };
                    button.navigation = noNav;
                    _activeButtonsDictionary.Add(bogie, button);
                    button.SetText(bogie.name);
                    button.SetSelectCallBack(() =>
                    {
                        currentInteractable.Value = interactable;
                        _currentSelectionIndex = button.transform.GetSiblingIndex();
                    });
                    button.SetHoverCallBack(() =>
                    {
                        currentInteractable.Value = interactable;
                        _currentSelectionIndex = button.transform.GetSiblingIndex();
                    });
                    button.Select();
                }
                else
                {
                    interactable.HoverDeselect();
                    CallbackButton button = _activeButtonsDictionary[bogie];
                    Destroy(button.gameObject);
                    
                    int buttonIndex = button.transform.GetSiblingIndex();

                    if (_currentSelectionIndex == buttonIndex)
                    {
                        ScrollSelect(-1f);
                    }
                    
                    _activeButtonsDictionary.Remove(bogie);
                }
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            }
        }
    }
}
