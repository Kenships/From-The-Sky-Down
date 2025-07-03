using UnityEngine;

namespace Interaction.InteractObjects
{
    public class DialogueInteractObject : InteractObjectBase
    {
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private GameObject dialoguePrefab;
        
        
        private GameObject _dialogue;
        
        public override void Interact()
        {
            base.Interact();
            Instantiate(dialoguePrefab, canvasTransform);
        }

        
    }
}
