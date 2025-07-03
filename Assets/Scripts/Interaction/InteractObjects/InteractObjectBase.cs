using Interaction.Interfaces;
using UnityEngine;

namespace Interaction.InteractObjects
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractObjectBase : MonoBehaviour, IInteractable
    {
        private bool _isInteracting;

        public virtual string Name
        {
            get => _name ?? name; 
            set => _name = value; 
        }
        private string _name;

        public virtual void Interact()
        {
            if (_isInteracting)
            {
                Debug.LogWarning($"Attempted to interact with {gameObject.name} when it is already interacting");
                return;
            }
            
            _isInteracting = true;
        }

        public virtual void CancelInteract()
        {
            _isInteracting = false;
        }
        
    }
}
