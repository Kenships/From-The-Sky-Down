using System;
using DialogueSystem;
using UnityEngine;

namespace Interaction
{
    public class DialogueInteractObject : InteractObjectBase
    {
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private GameObject dialoguePrefab;
        
        private GameObject _dialogue;
        
        public override void Interact()
        {
            Instantiate(dialoguePrefab, canvasTransform);
        }

        public override void CancelInteract()
        {
            
        }
    }
}
