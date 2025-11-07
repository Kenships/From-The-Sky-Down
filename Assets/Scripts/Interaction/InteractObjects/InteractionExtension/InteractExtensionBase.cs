using Interaction.InteractObjects;
using UnityEngine;

public abstract class InteractExtensionBase : MonoBehaviour
{
    [SerializeField] private InteractObjectBase interactParent;

    protected virtual void Start() {
        if (interactParent) {
            interactParent.OnInteract += OnInteract;
            interactParent.OnCancelInteract += OnCancelInteract;
        }
    }

    protected abstract void OnInteract();

    protected abstract void OnCancelInteract();
}
