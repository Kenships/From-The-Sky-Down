using System.Collections.Generic;
using Interaction.Interfaces;
using ObjectRadar;
using Player.Input;
using UI.Elements;
using UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Interaction.InteractionMenu
{
    public class InteractSelectMenu : MonoBehaviour
    {
        [SerializeField] private ScriptableEventRadarBogieInfo radarBogieInfo;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private IInteractableVariable currentInteractable;
        private readonly Dictionary<IInteractable, CallbackButton> _activeButtonsDictionary = new();
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
            if (transform.childCount == 0)
            {
                currentInteractable.Value = null;
                return;
            }
            
            if (Mathf.Approximately(direction, -1))
            {
                _currentSelectionIndex = (_currentSelectionIndex + 1) % transform.childCount;
            }
            else if (Mathf.Approximately(direction, 1))
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
            IInteractable[] interactables = bogie.GetComponents<IInteractable>();
            
            foreach (IInteractable interactable in interactables){
                if (bogieInfo.InRange)
                {
                    CallbackButton button = Instantiate(choiceButtonPrefab, transform).GetComponent<CallbackButton>();

                    Navigation noNav = new Navigation
                    {
                        mode = Navigation.Mode.None
                    };
                    button.navigation = noNav;
                    _activeButtonsDictionary.Add(interactable, button);
                    button.SetText(interactable.Name);
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

                    if (currentInteractable.Value == null)
                    {
                        button.Select();
                    }
                }
                else
                {
                    CallbackButton button = _activeButtonsDictionary[interactable];
                    Destroy(button.gameObject);
                    
                    int buttonIndex = button.transform.GetSiblingIndex();

                    if (_currentSelectionIndex == buttonIndex)
                    {
                        ScrollSelect(-1f);
                    }
                    
                    _activeButtonsDictionary.Remove(interactable);
                    
                    if (_activeButtonsDictionary.Count == 0)
                    {
                        currentInteractable.Value = null;
                    }
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            
            if (bogie.TryGetComponent(out IHoverable hoverable))
            {
                if (bogieInfo.InRange)
                {
                    hoverable.HoverSelect();
                }
                else
                {
                    hoverable.HoverDeselect();
                }
            }
        }
    }
}
