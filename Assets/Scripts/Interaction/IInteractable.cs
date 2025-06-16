using System;
using UnityEngine;

namespace Interaction
{
    public interface IInteractable
    {
        public void Interact();
        public void CancelInteract();
        public void HoverSelect();
        public void HoverDeselect();
    }
}
