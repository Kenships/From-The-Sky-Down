using Interaction;
using UnityEngine;
using Obvious.Soap;

namespace Interaction
{
    [CreateAssetMenu(fileName = "ScriptableVariable" + nameof(IInteractable), menuName = "Soap/ScriptableVariables/"+ nameof(IInteractable))]
    public class IInteractableVariable : ScriptableVariable<IInteractable>
    {
            
    }
}
