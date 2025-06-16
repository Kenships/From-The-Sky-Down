using System.Collections.Generic;
using DialogueSystem.Utilities;
using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharacterController
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private IInteractableVariable currentInteractable;

        private void Start()
        {
            inputReader.RequestInteract += Interact;
            inputReader.CancelInteract += CancelInteract;
        }

        private void CancelInteract()
        {
            currentInteractable.Value.CancelInteract();
        }

        private void Interact()
        {
            currentInteractable.Value.Interact();
        }
    }
}
