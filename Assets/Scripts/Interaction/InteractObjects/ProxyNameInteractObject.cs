using UnityEngine;

namespace Interaction.InteractObjects
{
    public class ProxyNameInteractObject : InteractObjectBase
    {
        public override string Name => "Proxy";

        public override void Interact()
        {
            base.Interact();
            Debug.Log("Interacting Proxy");
        }

        public override void CancelInteract()
        {
            base.CancelInteract();
            Debug.Log("Canceled Interacting Proxy");
        }
    }
}
