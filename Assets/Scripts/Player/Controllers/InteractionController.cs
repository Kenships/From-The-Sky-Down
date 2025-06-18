using Interaction;
using Interaction.Interfaces;
using Player.Input;
using UnityEngine;

namespace Player.Controllers
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private InputReaderSO inputReader;
        [SerializeField] private IInteractableVariable currentInteract;

        private void Start()
        {
            inputReader.RequestInteract += Interact;
            inputReader.CancelInteract += CancelInteract;
            currentInteract.OnValueChanged += CancelInteract;
        }

        private void Interact()
        {
            currentInteract.Value?.Interact();
        }
        
        private void CancelInteract()
        {
            currentInteract.Value?.CancelInteract();
        }

        private void CancelInteract(IInteractable interactable)
        {
            currentInteract.PreviousValue?.CancelInteract();
        }
    }
}
