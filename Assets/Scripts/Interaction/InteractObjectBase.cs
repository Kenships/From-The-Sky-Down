using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractObjectBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject hoverSelectVisual;
        public abstract void Interact();
        public abstract void CancelInteract();
        public virtual void HoverSelect()
        {
            hoverSelectVisual?.SetActive(true);
        }

        public virtual void HoverDeselect()
        {
            hoverSelectVisual?.SetActive(false);
        }
    }
}
